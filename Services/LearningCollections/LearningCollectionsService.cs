using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Dtos.Learning;
using DictionaryProvider.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Services.LearningCollections;

public class LearningCollectionsService : ILearningCollectionsService
{
    private readonly ApplicationDbContext _dbContext;

    public LearningCollectionsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LearningCollectionsSnapshotDto> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var initialized = await _dbContext.LearningCollectionProfiles
            .AsNoTracking()
            .AnyAsync(profile => profile.UserId == userId, cancellationToken);

        if (!initialized)
            return new LearningCollectionsSnapshotDto { Initialized = false };

        var collections = await _dbContext.LearningCollections
            .AsNoTracking()
            .Where(collection => collection.ProfileUserId == userId)
            .Include(collection => collection.Words)
            .OrderBy(collection => collection.Name)
            .ToListAsync(cancellationToken);

        return new LearningCollectionsSnapshotDto
        {
            Initialized = true,
            Collections = collections.Select(ToDto).ToList()
        };
    }

    public async Task<LearningCollectionsSnapshotDto> SaveAsync(Guid userId, SaveLearningCollectionsRequest request, CancellationToken cancellationToken)
    {
        Validate(request.Collections);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var profile = await _dbContext.LearningCollectionProfiles
            .SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new LearningCollectionProfile
            {
                UserId = userId
            };

            _dbContext.LearningCollectionProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await _dbContext.LearningCollections
            .Where(collection => collection.ProfileUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        profile.UpdatedAt = DateTimeOffset.UtcNow;

        var collections = request.Collections.Select(collection => ToEntity(collection, userId)).ToList();

        _dbContext.LearningCollections.AddRange(collections);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetAsync(userId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid collectionId, CancellationToken cancellationToken)
    {
        var deleted = await _dbContext.LearningCollections
            .Where(collection => collection.Id == collectionId && collection.ProfileUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0) return false;

        await _dbContext.LearningCollectionProfiles
            .Where(profile => profile.UserId == userId)
            .ExecuteUpdateAsync(update => update.SetProperty(profile => profile.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken);

        return true;
    }

    private static LearningCollectionEntity ToEntity(LearningCollectionDto collection, Guid userId)
    {
        return new LearningCollectionEntity
        {
            Id = collection.Id,
            ProfileUserId = userId,
            Name = collection.Name.Trim(),
            Accent = collection.Accent.Trim(),
            IsDefault = collection.IsDefault,
            Words = collection.Words.Select(ToEntity).ToList()
        };
    }

    private static LearningCollectionWordEntity ToEntity(LearningCollectionWordDto word)
    {
        return new LearningCollectionWordEntity
        {
            Id = word.Id,
            Word = word.Word.Trim(),
            NormalizedWord = Normalize(word.Word),
            State = word.State,
            Level = word.Level,
            Definition = word.Definition,
            Translation = word.Translation,
            AddedAt = word.AddedAt,
            LastReviewedAt = word.LastReviewedAt,
            EasyCount = word.EasyCount,
            HardCount = word.HardCount,
            AgainCount = word.AgainCount
        };
    }

    private static LearningCollectionDto ToDto(LearningCollectionEntity collection)
    {
        return new LearningCollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Accent = collection.Accent,
            IsDefault = collection.IsDefault,
            Words = collection.Words
                .OrderByDescending(word => word.AddedAt)
                .Select(word => ToDto(word, collection.Name))
                .ToList()
        };
    }

    private static LearningCollectionWordDto ToDto(LearningCollectionWordEntity word, string collectionName)
    {
        return new LearningCollectionWordDto
        {
            Id = word.Id,
            Word = word.Word,
            CollectionName = collectionName,
            State = word.State,
            Level = word.Level,
            Definition = word.Definition,
            Translation = word.Translation,
            AddedAt = word.AddedAt,
            LastReviewedAt = word.LastReviewedAt,
            EasyCount = word.EasyCount,
            HardCount = word.HardCount,
            AgainCount = word.AgainCount
        };
    }

    private static void Validate(IReadOnlyList<LearningCollectionDto> collections)
    {
        if (collections.Count(collection => collection.IsDefault) > 1)
            throw new InvalidOperationException("Only one learning collection can be the default.");

        if (collections.Any(collection => collection.Id == Guid.Empty || string.IsNullOrWhiteSpace(collection.Name)))
            throw new InvalidOperationException("Every learning collection needs an id and name.");

        if (collections.GroupBy(collection => collection.Id).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Learning collection ids must be unique.");

        if (collections.GroupBy(collection => Normalize(collection.Name)).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Learning collection names must be unique.");

        var words = collections.SelectMany(collection => collection.Words).ToList();

        if (words.Any(word => word.Id == Guid.Empty || string.IsNullOrWhiteSpace(word.Word)))
            throw new InvalidOperationException("Every learning word needs an id and value.");

        if (words.GroupBy(word => word.Id).Any(group => group.Count() > 1))
            throw new InvalidOperationException("Learning word ids must be unique.");
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
