using System.Security.Claims;
using System.Text;
using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Entities;
using DictionaryProvider.Api.Parsers;
using DictionaryProvider.Api.Parsers.Cambridge;
using DictionaryProvider.Api.Parsers.Details;
using DictionaryProvider.Api.Services.Authentication;
using DictionaryProvider.Api.Services.Dictionary;
using DictionaryProvider.Api.Services.DictionaryLookup;
using DictionaryProvider.Api.Services.DictionaryQuery;
using DictionaryProvider.Api.Services.EnglishMorphology;
using DictionaryProvider.Api.Services.FeedbackNotifier;
using DictionaryProvider.Api.Services.LearningCollections;
using DictionaryProvider.Api.Services.PhotoTranslation;
using DictionaryProvider.Api.Services.Providers;
using DictionaryProvider.Api.Services.Token;
using DictionaryProvider.Api.Services.VocabularyList;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<CambridgeOptions>()
    .BindConfiguration(CambridgeOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Cambridge base URL is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.TranslationBaseUrl), "Cambridge translation base URL is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.AutocompleteUrl), "Cambridge autocomplete URL is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.AutocompleteDataset), "Cambridge autocomplete dataset is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserAgent), "Cambridge user agent is required.")
    .Validate(options => options.TimeoutSeconds > 0, "Cambridge timeout must be greater than 0.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.DictionaryVariant), "Cambridge dictionary variant is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.TranslationLanguage), "Cambridge translation language is required.")
    .ValidateOnStart();

builder.Services.Configure<PhotoTranslationOptions>(builder.Configuration.GetSection(PhotoTranslationOptions.SectionName));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.Configure<FeedbackNotificationOptions>(builder.Configuration.GetSection(FeedbackNotificationOptions.SectionName));

builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "JWT signing key is required.")
    .Validate(options => options.AccessTokenMinutes > 0, "JWT access token lifetime must be greater than 0.")
    .Validate(options => options.RememberMeDays > 0, "JWT remember me lifetime must be greater than 0.")
    .ValidateOnStart();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AdminAuthorization.DeveloperPolicy, policy =>
        policy.RequireRole(AdminAuthorization.AdminRole));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("LexiFlowClient", policy =>
        policy.WithOrigins(
                "https://localhost:7102",
                "http://localhost:5087",
                "https://localhost:7260",
                "http://localhost:5231",
                "https://lexi-flow-chi.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<IVocabularyListService, VocabularyListService>();
builder.Services.AddScoped<ILearningCollectionsService, LearningCollectionsService>();
builder.Services.AddScoped<IPhotoTranslationService, PhotoTranslationService>();
builder.Services.AddScoped<IFeedbackNotifierService, FeedbackNotifierService>();
builder.Services.AddScoped<ICambridgeProvider, CambridgeProvider>();
builder.Services.AddSingleton<IDictionaryQueryService, DictionaryQueryService>();
builder.Services.AddSingleton<IEnglishMorphologyService, EnglishMorphologyService>();
builder.Services.AddScoped<IDictionaryLookupService, DictionaryLookupService>();

builder.Services.AddScoped<HtmlTextNormalizer>();
builder.Services.AddScoped<WordParser>();
builder.Services.AddScoped<PronunciationParser>();
builder.Services.AddScoped<MeaningParser>();
builder.Services.AddScoped<ExampleParser>();
builder.Services.AddScoped<SynonymParser>();
builder.Services.AddScoped<AntonymParser>();
builder.Services.AddScoped<PhrasalVerbParser>();
builder.Services.AddScoped<IdiomParser>();
builder.Services.AddScoped<CollocationParser>();
builder.Services.AddScoped<TranslationParser>();

builder.Services.AddHttpClient(CambridgeProvider.HttpClientName, (provider, client) =>
{
    var options = provider.GetRequiredService<IOptions<CambridgeOptions>>().Value;

    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
});

builder.Services.AddHttpClient("PhotoTranslation", client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseCors("LexiFlowClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
