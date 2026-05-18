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

        var localOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(localConnection)
            .Options;

        var remoteOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(lexiconConnection)
            .Options;

        await using var localDb = new ApplicationDbContext(localOptions);
        await using var remoteDb = new ApplicationDbContext(remoteOptions);

        var remoteCanConnect = await remoteDb.Database.CanConnectAsync(cancellationToken);
        if (!remoteCanConnect)
        {
            throw new InvalidOperationException("Cannot connect to Lexicon SQL Server with current connection configuration.");
        }

        if (!await remoteDb.Articles.AnyAsync(cancellationToken))
        {
            await remoteDb.Database.EnsureCreatedAsync(cancellationToken);
        }
        await using var transaction = await remoteDb.Database.BeginTransactionAsync(cancellationToken);

        var total = 0;
        total += await UpsertRangeAsync(remoteDb, remoteDb.Users, await localDb.Users.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);
        total += await UpsertRangeAsync(remoteDb, remoteDb.Roles, await localDb.Roles.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);
        total += await UpsertRangeAsync(remoteDb, remoteDb.UserRoles, await localDb.UserRoles.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);
        total += await UpsertRangeAsync(remoteDb, remoteDb.UserClaims, await localDb.UserClaims.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);
        total += await UpsertRangeAsync(remoteDb, remoteDb.UserLogins, await localDb.UserLogins.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);
        total += await UpsertRangeAsync(remoteDb, remoteDb.UserTokens, await localDb.UserTokens.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);
        total += await UpsertRangeAsync(remoteDb, remoteDb.RoleClaims, await localDb.RoleClaims.AsNoTracking().ToListAsync(cancellationToken), cancellationToken);

        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.Categories, await localDb.Categories.AsNoTracking().ToListAsync(cancellationToken), "Categories", cancellationToken);
        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.SubscriptionTypes, await localDb.SubscriptionTypes.AsNoTracking().ToListAsync(cancellationToken), "SubscriptionTypes", cancellationToken);
        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.Articles, await localDb.Articles.AsNoTracking().ToListAsync(cancellationToken), "Articles", cancellationToken);
        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.Subscriptions, await localDb.Subscriptions.AsNoTracking().ToListAsync(cancellationToken), "Subscriptions", cancellationToken);
        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.ArticleLikes, await localDb.ArticleLikes.AsNoTracking().ToListAsync(cancellationToken), "ArticleLikes", cancellationToken);
        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.UnsubscribeLogs, await localDb.UnsubscribeLogs.AsNoTracking().ToListAsync(cancellationToken), "UnsubscribeLogs", cancellationToken);
        total += await UpsertWithIdentityAsync(remoteDb, remoteDb.NewsletterPreferences, await localDb.NewsletterPreferences.AsNoTracking().ToListAsync(cancellationToken), "NewsletterPreferences", cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Migrated {TotalRows} rows from local SQLite to Lexicon SQL Server.", total);

        return total;
    }

    private static async Task<int> UpsertRangeAsync<TEntity>(ApplicationDbContext remoteDb, DbSet<TEntity> targetSet, IReadOnlyCollection<TEntity> sourceItems, CancellationToken cancellationToken)
        where TEntity : class
    {
        if (sourceItems.Count == 0)
        {
            return 0;
        }

        targetSet.UpdateRange(sourceItems);
        return await remoteDb.SaveChangesAsync(cancellationToken);
    }

    private static async Task<int> UpsertWithIdentityAsync<TEntity>(ApplicationDbContext remoteDb, DbSet<TEntity> targetSet, IReadOnlyCollection<TEntity> sourceItems, string tableName, CancellationToken cancellationToken)
        where TEntity : class
    {
        if (sourceItems.Count == 0)
        {
            return 0;
        }

        await remoteDb.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{tableName}] ON", cancellationToken);
        targetSet.UpdateRange(sourceItems);
        var affected = await remoteDb.SaveChangesAsync(cancellationToken);
        await remoteDb.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{tableName}] OFF", cancellationToken);

        return affected;
    }
}
