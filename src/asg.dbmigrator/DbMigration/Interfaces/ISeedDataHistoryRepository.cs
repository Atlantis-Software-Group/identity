using asg.dbmigrator.DbMigration.Models;

namespace asg.dbmigrator.DbMigration.Interfaces;

public interface ISeedDataHistoryRepository
{
    /// <summary>
    ///     Checks whether or not the history table exists.
    /// </summary>
    /// <returns><see langword="true" /> if the table already exists, <see langword="false" /> otherwise.</returns>
    bool Exists();

    /// <summary>
    ///     Checks whether or not the history table exists.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     <see langword="true" /> if the table already exists, <see langword="false" /> otherwise.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the history table for all migrations that have been applied.
    /// </summary>
    /// <returns>The list of applied migrations, as <see cref="HistoryRow" /> entities.</returns>
    IReadOnlyList<SeedScriptHistoryRow> GetAppliedSeedScripts();

    /// <summary>
    ///     Queries the history table for all migrations that have been applied.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the list of applied migrations, as <see cref="HistoryRow" /> entities.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<IReadOnlyList<SeedScriptHistoryRow>> GetAppliedSeedScriptsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the history table for all migrations that have been applied.
    /// </summary>
    /// <returns>The list of applied migrations, as <see cref="HistoryRow" /> entities.</returns>
    IReadOnlyList<SeedScriptHistoryRow> GetAppliedSeedScriptsForMigration(string migrationName);

    /// <summary>
    ///     Queries the history table for all migrations that have been applied.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains
    ///     the list of applied migrations, as <see cref="HistoryRow" /> entities.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task<IReadOnlyList<SeedScriptHistoryRow>> GetAppliedSeedScriptsForMigrationAsync(string migrationName, CancellationToken cancellationToken = default);

    Task<SeedScriptHistoryRow> InsertHistoryRow(string name, string migrationName, CancellationToken cancellationToken = default(CancellationToken));
    string GetInsertScript(SeedScriptHistoryRow row);
}
