namespace NewsSite.Services.Interfaces;

public interface ILocalToSqlServerMigrationService
{
    Task<int> MigrateAsync(CancellationToken cancellationToken = default);
}
