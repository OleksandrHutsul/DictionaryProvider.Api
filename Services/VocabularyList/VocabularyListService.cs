using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Dtos.Learning;
using DictionaryProvider.Api.Entities;
using DictionaryProvider.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Services.VocabularyList;

public class VocabularyListService : IVocabularyListService
{
    private const string VocabularyListSharedNotificationType = "VocabularyListShared";

    private readonly ApplicationDbContext _dbContext;

    public VocabularyListService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VocabularyListSummaryDto>> GetOwnedListsAsync(Guid userId, bool archived, CancellationToken cancellationToken)
    {
        return await _dbContext.VocabularyLists
            .AsNoTracking()
            .Where(list => list.OwnerId == userId && list.IsArchived == archived)
            .OrderByDescending(list => list.UpdatedAt)
            .Select(list => new VocabularyListSummaryDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                CreatedAt = list.CreatedAt,
                OwnerId = list.OwnerId,
                OwnerName = list.Owner!.UserName,
                WordCount = list.Words.Count,
                IsOwner = true,
                Permission = nameof(VocabularyListPermission.Owner),
                IsArchived = list.IsArchived,
                ArchivedAt = list.ArchivedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VocabularyListSummaryDto>> GetSharedListsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.VocabularyListShares
            .AsNoTracking()
            .Where(share => share.UserId == userId)
            .OrderByDescending(share => share.SharedAt)
            .Select(share => new VocabularyListSummaryDto
            {
                Id = share.VocabularyList!.Id,
                Name = share.VocabularyList.Name,
                Description = share.VocabularyList.Description,
                CreatedAt = share.VocabularyList.CreatedAt,
                OwnerId = share.VocabularyList.OwnerId,
                OwnerName = share.VocabularyList.Owner!.UserName,
                WordCount = share.VocabularyList.Words.Count,
                IsOwner = false,
                Permission = share.Permission.ToString(),
                IsArchived = share.VocabularyList.IsArchived,
                ArchivedAt = share.VocabularyList.ArchivedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<VocabularyListDetailDto?> GetListAsync(Guid listId, Guid userId, CancellationToken cancellationToken)
    {
        var access = await _dbContext.VocabularyLists
            .AsNoTracking()
            .Where(list => list.Id == listId)
            .Select(list => new
            {
                IsOwner = list.OwnerId == userId,
                Permission = list.OwnerId == userId
                    ? VocabularyListPermission.Owner
                    : list.Shares
                        .Where(share => share.UserId == userId)
                        .Select(share => (VocabularyListPermission?)share.Permission)
                        .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (access is null || access.Permission is null) return null;

        return await QueryDetail(listId, access.IsOwner, access.Permission.Value.ToString()).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<VocabularyListDetailDto> CreateListAsync(Guid userId, CreateVocabularyListRequest request, CancellationToken cancellationToken)
    {
        var list = new Entities.VocabularyList
        {
            Name = CleanRequired(request.Name, 120),
            Description = CleanOptional(request.Description, 600),
            OwnerId = userId
        };

        _dbContext.VocabularyLists.Add(list);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await QueryDetail(list.Id, true, VocabularyListPermission.Owner.ToString()).SingleAsync(cancellationToken);
    }

    public async Task<VocabularyListDetailDto?> UpdateListAsync(Guid listId, Guid userId, UpdateVocabularyListRequest request, CancellationToken cancellationToken)
    {
        var list = await _dbContext.VocabularyLists.SingleOrDefaultAsync(list => list.Id == listId && list.OwnerId == userId, cancellationToken);
        if (list is null) return null;

        list.Name = CleanRequired(request.Name, 120);
        list.Description = CleanOptional(request.Description, 600);
        list.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await QueryDetail(listId, true, VocabularyListPermission.Owner.ToString()).SingleAsync(cancellationToken);
    }

    public async Task<bool> DeleteListAsync(Guid listId, Guid userId, CancellationToken cancellationToken)
    {
        var deleted = await _dbContext.VocabularyLists
            .Where(list => list.Id == listId && list.OwnerId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task<bool> SetArchivedAsync(Guid listId, Guid userId, bool archived, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var updated = await _dbContext.VocabularyLists
            .Where(list => list.Id == listId && list.OwnerId == userId && list.IsArchived != archived)
            .ExecuteUpdateAsync(update => update
                .SetProperty(list => list.IsArchived, archived)
                .SetProperty(list => list.ArchivedAt, archived ? now : null)
                .SetProperty(list => list.UpdatedAt, now), cancellationToken);

        return updated > 0;
    }

    public async Task<VocabularyListWordDto?> AddWordAsync(Guid listId, Guid userId, AddVocabularyWordRequest request, CancellationToken cancellationToken)
    {
        var list = await _dbContext.VocabularyLists.SingleOrDefaultAsync(list => list.Id == listId && list.OwnerId == userId, cancellationToken);
        if (list is null) return null;

        var normalizedWord = Normalize(request.Word);
        if (string.IsNullOrWhiteSpace(normalizedWord)) throw new InvalidOperationException("Word is required.");

        var dictionaryWord = await ResolveDictionaryWordAsync(request.DictionaryWordId, normalizedWord, cancellationToken);
        var displayWord = dictionaryWord?.Word ?? request.Word.Trim();

        var alreadyExists = await _dbContext.VocabularyListWords
            .AnyAsync(word => word.VocabularyListId == listId && word.NormalizedWord == normalizedWord, cancellationToken);

        if (alreadyExists) throw new InvalidOperationException("This word already exists in the list.");

        var word = new VocabularyListWord
        {
            VocabularyListId = listId,
            DictionaryWordId = dictionaryWord?.Id,
            DisplayWord = displayWord,
            NormalizedWord = normalizedWord
        };

        list.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.VocabularyListWords.Add(word);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToWordDto(word);
    }

    public async Task<bool> RemoveWordAsync(Guid listId, Guid wordId, Guid userId, CancellationToken cancellationToken)
    {
        var ownsList = await _dbContext.VocabularyLists.AnyAsync(list => list.Id == listId && list.OwnerId == userId, cancellationToken);
        if (!ownsList) return false;

        var deleted = await _dbContext.VocabularyListWords
            .Where(word => word.Id == wordId && word.VocabularyListId == listId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0) return false;

        await _dbContext.VocabularyLists
            .Where(list => list.Id == listId)
            .ExecuteUpdateAsync(update => update.SetProperty(list => list.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken);

        return true;
    }

    public async Task<VocabularyListShareDto?> ShareListAsync(Guid listId, Guid userId, ShareVocabularyListRequest request, CancellationToken cancellationToken)
    {
        var list = await _dbContext.VocabularyLists
            .Where(list => list.Id == listId && list.OwnerId == userId && !list.IsArchived)
            .Select(list => new { list.Id, list.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (list is null) return null;

        var sharedUser = await FindUserAsync(request, cancellationToken);
        if (sharedUser is null || sharedUser.Id == userId) return null;

        var permission = ParsePermission(request.Permission);

        var share = await _dbContext.VocabularyListShares
            .SingleOrDefaultAsync(share => share.VocabularyListId == listId && share.UserId == sharedUser.Id, cancellationToken);

        if (share is null)
        {
            share = new VocabularyListShare
            {
                VocabularyListId = listId,
                UserId = sharedUser.Id,
                Permission = permission
            };

            _dbContext.VocabularyListShares.Add(share);
        }
        else
        {
            share.Permission = permission;
        }

        var hasNotification = await _dbContext.Notifications.AnyAsync(notification => notification.RecipientUserId == sharedUser.Id &&
            notification.RelatedListId == listId && notification.Type == VocabularyListSharedNotificationType, cancellationToken);

        if (!hasNotification)
        {
            _dbContext.Notifications.Add(new AppNotification
            {
                RecipientUserId = sharedUser.Id,
                Title = "New vocabulary list",
                Message = $"“{list.Name}” was shared with you.",
                RelatedListId = listId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VocabularyListShareDto
        {
            UserId = sharedUser.Id,
            UserName = sharedUser.UserName,
            Email = sharedUser.Email,
            Permission = share.Permission.ToString(),
            SharedAt = share.SharedAt
        };
    }

    public async Task<bool> RemoveShareAsync(Guid listId, Guid sharedUserId, Guid userId, CancellationToken cancellationToken)
    {
        var ownsList = await _dbContext.VocabularyLists.AnyAsync(list => list.Id == listId && list.OwnerId == userId, cancellationToken);
        if (!ownsList) return false;

        var deleted = await _dbContext.VocabularyListShares
            .Where(share => share.VocabularyListId == listId && share.UserId == sharedUserId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task<bool> SaveTestResultAsync(Guid listId, Guid userId, SaveListTestResultRequest request, CancellationToken cancellationToken)
    {
        if (request.TotalQuestions <= 0 || request.CorrectAnswers < 0 || request.CorrectAnswers > request.TotalQuestions)
            throw new InvalidOperationException("The test score is invalid.");

        var hasAccess = await _dbContext.VocabularyLists.AnyAsync(list => list.Id == listId &&
            (list.OwnerId == userId || list.Shares.Any(share => share.UserId == userId)), cancellationToken);

        if (!hasAccess) return false;

        var result = new VocabularyListTestResult
        {
            VocabularyListId = listId,
            UserId = userId,
            CorrectAnswers = request.CorrectAnswers,
            TotalQuestions = request.TotalQuestions,
            CompletedAt = request.CompletedAt == default ? DateTimeOffset.UtcNow : request.CompletedAt
        };

        _dbContext.VocabularyListTestResults.Add(result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<DictionaryWord?> ResolveDictionaryWordAsync(Guid? dictionaryWordId, string normalizedWord, CancellationToken cancellationToken)
    {
        if (dictionaryWordId is not null)
            return await _dbContext.DictionaryWords.SingleOrDefaultAsync(word => word.Id == dictionaryWordId, cancellationToken);

        return await _dbContext.DictionaryWords.SingleOrDefaultAsync(word => word.NormalizedWord == normalizedWord, cancellationToken);
    }

    private async Task<ApplicationUser?> FindUserAsync(ShareVocabularyListRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId is not null)
            return await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == request.UserId, cancellationToken);

        var normalizedEmail = Normalize(request.Email ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedEmail)) return null;

        return await _dbContext.Users.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    private IQueryable<VocabularyListDetailDto> QueryDetail(Guid listId, bool isOwner, string permission)
    {
        return _dbContext.VocabularyLists
            .AsNoTracking()
            .Where(list => list.Id == listId)
            .Select(list => new VocabularyListDetailDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                CreatedAt = list.CreatedAt,
                OwnerId = list.OwnerId,
                OwnerName = list.Owner!.UserName,
                WordCount = list.Words.Count,
                IsOwner = isOwner,
                Permission = permission,
                IsArchived = list.IsArchived,
                ArchivedAt = list.ArchivedAt,
                Words = list.Words
                    .OrderBy(word => word.DisplayWord)
                    .Select(word => new VocabularyListWordDto
                    {
                        Id = word.Id,
                        DictionaryWordId = word.DictionaryWordId,
                        Word = word.DisplayWord,
                        AddedAt = word.AddedAt
                    })
                    .ToList(),
                Shares = list.Shares
                    .OrderBy(share => share.User!.UserName)
                    .Select(share => new VocabularyListShareDto
                    {
                        UserId = share.UserId,
                        UserName = share.User!.UserName,
                        Email = share.User.Email,
                        Permission = share.Permission.ToString(),
                        SharedAt = share.SharedAt
                    })
                    .ToList()
            });
    }

    private static VocabularyListWordDto ToWordDto(VocabularyListWord word)
    {
        return new VocabularyListWordDto
        {
            Id = word.Id,
            DictionaryWordId = word.DictionaryWordId,
            Word = word.DisplayWord,
            AddedAt = word.AddedAt
        };
    }

    private static string CleanRequired(string value, int maxLength)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) throw new InvalidOperationException("Name is required.");

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? CleanOptional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return null;

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static VocabularyListPermission ParsePermission(string value)
    {
        return Enum.TryParse<VocabularyListPermission>(value, true, out var permission)
            ? permission
            : VocabularyListPermission.Reader;
    }
}
