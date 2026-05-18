using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class LocalToSqlServerMigrationService(IConfiguration configuration, ILogger<LocalToSqlServerMigrationService> logger) : ILocalToSqlServerMigrationService
{
    public async Task<int> MigrateAsync(CancellationToken cancellationToken = default)
    {
        var localConnection = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var lexiconConnection = configuration.GetConnectionString("LexiconConnection");
        if (string.IsNullOrWhiteSpace(lexiconConnection))
        {
            throw new InvalidOperationException("Connection string 'LexiconConnection' not found.");
        }

        var localOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(localConnection).Options;
        var remoteOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(lexiconConnection).Options;

        await using var localDb = new ApplicationDbContext(localOptions);
        await using var remoteDb = new ApplicationDbContext(remoteOptions);

        if (!await remoteDb.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException("Cannot connect to Lexicon SQL Server with current connection configuration.");
        }

        await remoteDb.Database.EnsureCreatedAsync(cancellationToken);
        await using var tx = await remoteDb.Database.BeginTransactionAsync(cancellationToken);

        var migrated = 0;

        var userIdMap = await MergeUsersAsync(localDb, remoteDb, cancellationToken);
        migrated += userIdMap.Count;

        var roleIdMap = await MergeRolesAsync(localDb, remoteDb, cancellationToken);
        migrated += roleIdMap.Count;

        migrated += await MergeUserRolesAsync(localDb, remoteDb, userIdMap, roleIdMap, cancellationToken);
        migrated += await MergeUserClaimsAsync(localDb, remoteDb, userIdMap, cancellationToken);
        migrated += await MergeUserLoginsAsync(localDb, remoteDb, userIdMap, cancellationToken);
        migrated += await MergeUserTokensAsync(localDb, remoteDb, userIdMap, cancellationToken);
        migrated += await MergeRoleClaimsAsync(localDb, remoteDb, roleIdMap, cancellationToken);

        var categoryIdMap = await MergeCategoriesAsync(localDb, remoteDb, cancellationToken);
        migrated += categoryIdMap.Count;

        var subscriptionTypeIdMap = await MergeSubscriptionTypesAsync(localDb, remoteDb, cancellationToken);
        migrated += subscriptionTypeIdMap.Count;

        var articleIdMap = await MergeArticlesAsync(localDb, remoteDb, categoryIdMap, userIdMap, cancellationToken);
        migrated += articleIdMap.Count;

        migrated += await MergeSubscriptionsAsync(localDb, remoteDb, userIdMap, subscriptionTypeIdMap, cancellationToken);
        migrated += await MergeArticleLikesAsync(localDb, remoteDb, articleIdMap, userIdMap, cancellationToken);
        migrated += await MergeUnsubscribeLogsAsync(localDb, remoteDb, userIdMap, cancellationToken);
        migrated += await MergeNewsletterPreferencesAsync(localDb, remoteDb, userIdMap, cancellationToken);

        await tx.CommitAsync(cancellationToken);
        logger.LogInformation("Aggregated local SQLite data into Lexicon SQL Server. Merged entities: {Count}.", migrated);
        return migrated;
    }

    private static async Task<Dictionary<string, string>> MergeUsersAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, CancellationToken ct)
    {
        var localUsers = await localDb.Users.AsNoTracking().ToListAsync(ct);
        var remoteUsers = await remoteDb.Users.ToListAsync(ct);
        var remoteByEmail = remoteUsers.Where(u => !string.IsNullOrWhiteSpace(u.NormalizedEmail)).ToDictionary(u => u.NormalizedEmail!, u => u);
        var map = new Dictionary<string, string>();

        foreach (var local in localUsers)
        {
            var key = local.NormalizedEmail ?? local.Email?.ToUpperInvariant() ?? local.Id;
            if (remoteByEmail.TryGetValue(key, out var existing))
            {
                map[local.Id] = existing.Id;
                continue;
            }

            var insert = CloneUser(local);
            remoteDb.Users.Add(insert);
            await remoteDb.SaveChangesAsync(ct);
            remoteByEmail[key] = insert;
            map[local.Id] = insert.Id;
        }

        return map;
    }

    private static ApplicationUser CloneUser(ApplicationUser user) => new()
    {
        UserName = user.UserName,
        NormalizedUserName = user.NormalizedUserName,
        Email = user.Email,
        NormalizedEmail = user.NormalizedEmail,
        EmailConfirmed = user.EmailConfirmed,
        PasswordHash = user.PasswordHash,
        SecurityStamp = user.SecurityStamp,
        ConcurrencyStamp = user.ConcurrencyStamp,
        PhoneNumber = user.PhoneNumber,
        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
        TwoFactorEnabled = user.TwoFactorEnabled,
        LockoutEnd = user.LockoutEnd,
        LockoutEnabled = user.LockoutEnabled,
        AccessFailedCount = user.AccessFailedCount,
        FirstName = user.FirstName,
        LastName = user.LastName,
        DateOfBirth = user.DateOfBirth,
        IsDeleted = user.IsDeleted
    };

    private static async Task<Dictionary<string, string>> MergeRolesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, CancellationToken ct)
    {
        var localRoles = await localDb.Roles.AsNoTracking().ToListAsync(ct);
        var remoteRoles = await remoteDb.Roles.ToListAsync(ct);
        var remoteByName = remoteRoles.Where(r => !string.IsNullOrWhiteSpace(r.NormalizedName)).ToDictionary(r => r.NormalizedName!, r => r);
        var map = new Dictionary<string, string>();

        foreach (var local in localRoles)
        {
            var key = local.NormalizedName ?? local.Name?.ToUpperInvariant() ?? local.Id;
            if (remoteByName.TryGetValue(key, out var existing)) { map[local.Id] = existing.Id; continue; }

            var insert = new Microsoft.AspNetCore.Identity.IdentityRole { Name = local.Name, NormalizedName = local.NormalizedName, ConcurrencyStamp = local.ConcurrencyStamp };
            remoteDb.Roles.Add(insert);
            await remoteDb.SaveChangesAsync(ct);
            remoteByName[key] = insert;
            map[local.Id] = insert.Id;
        }

        return map;
    }

    private static async Task<int> MergeUserRolesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, IReadOnlyDictionary<string, string> roleMap, CancellationToken ct)
    {
        var localItems = await localDb.UserRoles.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.UserRoles.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.UserId}|{x.RoleId}").ToHashSet();
        var inserts = new List<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>();

        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId) || !roleMap.TryGetValue(row.RoleId, out var roleId)) continue;
            var key = $"{userId}|{roleId}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string> { UserId = userId, RoleId = roleId });
            remoteKeys.Add(key);
        }

        if (inserts.Count == 0) return 0;
        await remoteDb.UserRoles.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeUserClaimsAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    {
        var localItems = await localDb.UserClaims.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.UserClaims.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.UserId}|{x.ClaimType}|{x.ClaimValue}").ToHashSet();
        var inserts = new List<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>();
        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId)) continue;
            var key = $"{userId}|{row.ClaimType}|{row.ClaimValue}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new Microsoft.AspNetCore.Identity.IdentityUserClaim<string> { UserId = userId, ClaimType = row.ClaimType, ClaimValue = row.ClaimValue });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.UserClaims.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeUserLoginsAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    { /* similar */
        var localItems = await localDb.UserLogins.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.UserLogins.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.LoginProvider}|{x.ProviderKey}|{x.UserId}").ToHashSet();
        var inserts = new List<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>();
        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId)) continue;
            var key = $"{row.LoginProvider}|{row.ProviderKey}|{userId}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new Microsoft.AspNetCore.Identity.IdentityUserLogin<string> { LoginProvider = row.LoginProvider, ProviderKey = row.ProviderKey, ProviderDisplayName = row.ProviderDisplayName, UserId = userId });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.UserLogins.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeUserTokensAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    {
        var localItems = await localDb.UserTokens.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.UserTokens.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.UserId}|{x.LoginProvider}|{x.Name}").ToHashSet();
        var inserts = new List<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>();
        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId)) continue;
            var key = $"{userId}|{row.LoginProvider}|{row.Name}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new Microsoft.AspNetCore.Identity.IdentityUserToken<string> { UserId = userId, LoginProvider = row.LoginProvider, Name = row.Name, Value = row.Value });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.UserTokens.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeRoleClaimsAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> roleMap, CancellationToken ct)
    {
        var localItems = await localDb.RoleClaims.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.RoleClaims.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.RoleId}|{x.ClaimType}|{x.ClaimValue}").ToHashSet();
        var inserts = new List<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>();
        foreach (var row in localItems)
        {
            if (!roleMap.TryGetValue(row.RoleId, out var roleId)) continue;
            var key = $"{roleId}|{row.ClaimType}|{row.ClaimValue}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = roleId, ClaimType = row.ClaimType, ClaimValue = row.ClaimValue });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.RoleClaims.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<Dictionary<int, int>> MergeCategoriesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, CancellationToken ct)
    {
        var localItems = await localDb.Categories.AsNoTracking().ToListAsync(ct);
        var remoteItems = await remoteDb.Categories.ToListAsync(ct);
        var byName = remoteItems.ToDictionary(x => x.Name.Trim().ToUpperInvariant(), x => x);
        var map = new Dictionary<int, int>();
        foreach (var row in localItems)
        {
            var key = row.Name.Trim().ToUpperInvariant();
            if (byName.TryGetValue(key, out var existing)) { map[row.Id] = existing.Id; continue; }
            var insert = new Category { Name = row.Name };
            remoteDb.Categories.Add(insert);
            await remoteDb.SaveChangesAsync(ct);
            byName[key] = insert;
            map[row.Id] = insert.Id;
        }
        return map;
    }

    private static async Task<Dictionary<int, int>> MergeSubscriptionTypesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, CancellationToken ct)
    {
        var localItems = await localDb.SubscriptionTypes.AsNoTracking().ToListAsync(ct);
        var remoteItems = await remoteDb.SubscriptionTypes.ToListAsync(ct);
        var byName = remoteItems.ToDictionary(x => x.Name.Trim().ToUpperInvariant(), x => x);
        var map = new Dictionary<int, int>();
        foreach (var row in localItems)
        {
            var key = row.Name.Trim().ToUpperInvariant();
            if (byName.TryGetValue(key, out var existing)) { map[row.Id] = existing.Id; continue; }
            var insert = new SubscriptionType { Name = row.Name, Price = row.Price };
            remoteDb.SubscriptionTypes.Add(insert);
            await remoteDb.SaveChangesAsync(ct);
            byName[key] = insert;
            map[row.Id] = insert.Id;
        }
        return map;
    }

    private static async Task<Dictionary<int, int>> MergeArticlesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<int, int> categoryMap, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    {
        var localItems = await localDb.Articles.AsNoTracking().ToListAsync(ct);
        var remoteItems = await remoteDb.Articles.ToListAsync(ct);
        var bySlug = remoteItems.Where(a => !string.IsNullOrWhiteSpace(a.Slug)).ToDictionary(a => a.Slug.Trim().ToUpperInvariant(), a => a);
        var map = new Dictionary<int, int>();
        foreach (var row in localItems)
        {
            var key = string.IsNullOrWhiteSpace(row.Slug) ? $"TITLE:{row.Title.Trim().ToUpperInvariant()}" : row.Slug.Trim().ToUpperInvariant();
            if (bySlug.TryGetValue(key, out var existing)) { map[row.Id] = existing.Id; continue; }
            var insert = new Article
            {
                Title = row.Title, Content = row.Content, Summary = row.Summary, Slug = row.Slug, ImageUrl = row.ImageUrl,
                CreatedAt = row.CreatedAt, MetaTitle = row.MetaTitle, MetaDescription = row.MetaDescription, ViewsCount = row.ViewsCount,
                IsArchived = row.IsArchived, IsDeleted = row.IsDeleted, IsEditorsChoice = row.IsEditorsChoice, IsReadyForPublish = row.IsReadyForPublish,
                IsLocked = row.IsLocked, IsPremium = row.IsPremium, AuthorName = row.AuthorName,
                CategoryId = categoryMap.GetValueOrDefault(row.CategoryId, row.CategoryId),
                AuthorId = row.AuthorId is not null && userMap.TryGetValue(row.AuthorId, out var mappedAuthorId) ? mappedAuthorId : null
            };
            remoteDb.Articles.Add(insert);
            await remoteDb.SaveChangesAsync(ct);
            bySlug[key] = insert;
            map[row.Id] = insert.Id;
        }
        return map;
    }

    private static async Task<int> MergeSubscriptionsAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, IReadOnlyDictionary<int, int> typeMap, CancellationToken ct)
    {
        var localItems = await localDb.Subscriptions.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.Subscriptions.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.UserId}|{x.SubscriptionTypeId}|{x.StartDate:O}|{x.EndDate:O}").ToHashSet();
        var inserts = new List<Subscription>();
        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId)) continue;
            var typeId = typeMap.GetValueOrDefault(row.SubscriptionTypeId, row.SubscriptionTypeId);
            var key = $"{userId}|{typeId}|{row.StartDate:O}|{row.EndDate:O}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new Subscription { UserId = userId, SubscriptionTypeId = typeId, StartDate = row.StartDate, EndDate = row.EndDate, PaymentComplete = row.PaymentComplete, RenewalReminderSentAt = row.RenewalReminderSentAt });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.Subscriptions.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeArticleLikesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<int, int> articleMap, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    {
        var localItems = await localDb.ArticleLikes.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.ArticleLikes.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.ArticleId}|{x.UserId}").ToHashSet();
        var inserts = new List<ArticleLike>();
        foreach (var row in localItems)
        {
            if (!articleMap.TryGetValue(row.ArticleId, out var articleId) || !userMap.TryGetValue(row.UserId, out var userId)) continue;
            var key = $"{articleId}|{userId}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new ArticleLike { ArticleId = articleId, UserId = userId });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.ArticleLikes.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeUnsubscribeLogsAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    {
        var localItems = await localDb.UnsubscribeLogs.AsNoTracking().ToListAsync(ct);
        var remoteKeys = (await remoteDb.UnsubscribeLogs.AsNoTracking().ToListAsync(ct)).Select(x => $"{x.UserId}|{x.Token}|{x.UnsubscribedAt:O}").ToHashSet();
        var inserts = new List<UnsubscribeLog>();
        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId)) continue;
            var key = $"{userId}|{row.Token}|{row.UnsubscribedAt:O}";
            if (remoteKeys.Contains(key)) continue;
            inserts.Add(new UnsubscribeLog { UserId = userId, Token = row.Token, UnsubscribedAt = row.UnsubscribedAt, Reason = row.Reason, WasReactivated = row.WasReactivated, ReactivatedAt = row.ReactivatedAt });
            remoteKeys.Add(key);
        }
        if (inserts.Count == 0) return 0;
        await remoteDb.UnsubscribeLogs.AddRangeAsync(inserts, ct);
        await remoteDb.SaveChangesAsync(ct);
        return inserts.Count;
    }

    private static async Task<int> MergeNewsletterPreferencesAsync(ApplicationDbContext localDb, ApplicationDbContext remoteDb, IReadOnlyDictionary<string, string> userMap, CancellationToken ct)
    {
        var localItems = await localDb.NewsletterPreferences.AsNoTracking().ToListAsync(ct);
        var remoteByUser = (await remoteDb.NewsletterPreferences.ToListAsync(ct)).ToDictionary(x => x.UserId, x => x);
        var affected = 0;
        foreach (var row in localItems)
        {
            if (!userMap.TryGetValue(row.UserId, out var userId)) continue;
            if (remoteByUser.TryGetValue(userId, out var existing))
            {
                existing.ReceiveNewsletter = existing.ReceiveNewsletter || row.ReceiveNewsletter;
                existing.Frequency = existing.UpdatedAt >= row.UpdatedAt ? existing.Frequency : row.Frequency;
                existing.SelectedCategoryIds = string.IsNullOrWhiteSpace(existing.SelectedCategoryIds) ? row.SelectedCategoryIds : existing.SelectedCategoryIds;
                existing.SelectedAuthIds = string.IsNullOrWhiteSpace(existing.SelectedAuthIds) ? row.SelectedAuthIds : existing.SelectedAuthIds;
                existing.LastSentDate = existing.LastSentDate >= row.LastSentDate ? existing.LastSentDate : row.LastSentDate;
                existing.UnsubscribeToken = existing.UnsubscribeToken ?? row.UnsubscribeToken;
                existing.UpdatedAt = existing.UpdatedAt >= row.UpdatedAt ? existing.UpdatedAt : row.UpdatedAt;
                existing.IsUnsubscribed = existing.IsUnsubscribed || row.IsUnsubscribed;
                existing.UnsubscribedAt = existing.UnsubscribedAt >= row.UnsubscribedAt ? existing.UnsubscribedAt : row.UnsubscribedAt;
                existing.UnsubscribeReason ??= row.UnsubscribeReason;
                affected++;
                continue;
            }

            remoteDb.NewsletterPreferences.Add(new NewsletterPreference
            {
                UserId = userId,
                ReceiveNewsletter = row.ReceiveNewsletter,
                Frequency = row.Frequency,
                SelectedCategoryIds = row.SelectedCategoryIds,
                SelectedAuthIds = row.SelectedAuthIds,
                LastSentDate = row.LastSentDate,
                UnsubscribeToken = row.UnsubscribeToken,
                UpdatedAt = row.UpdatedAt,
                IsUnsubscribed = row.IsUnsubscribed,
                UnsubscribedAt = row.UnsubscribedAt,
                UnsubscribeReason = row.UnsubscribeReason
            });
            affected++;
        }
        await remoteDb.SaveChangesAsync(ct);
        return affected;
    }
}
