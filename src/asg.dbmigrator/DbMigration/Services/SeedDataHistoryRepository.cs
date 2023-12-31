using System.Text;
using asg.dbmigrator.DbMigration.Interfaces;
using asg.dbmigrator.DbMigration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace asg.dbmigrator.DbMigration.Services;

public class SeedDataHistoryRepository : ISeedDataHistoryRepository
{
    /// <summary>
    ///     The default name for the Migrations history table.
    /// </summary>
    public const string DefaultTableName = "__SeedDataMigrationsHistory";

    private IModel? _model;
    
    public SeedDataHistoryRepository(HistoryRepositoryDependencies dependencies)
    {
        Dependencies = dependencies;

        var relationalOptions = RelationalOptionsExtension.Extract(dependencies.Options);
        TableName = relationalOptions.MigrationsHistoryTableName ?? DefaultTableName;
        TableSchema = relationalOptions.MigrationsHistoryTableSchema;
    }

    public virtual HistoryRepositoryDependencies Dependencies { get; }
    protected virtual string TableName { get; }
    protected virtual ISqlGenerationHelper SqlGenerationHelper
        => Dependencies.SqlGenerationHelper;

    /// <summary>
    ///     The schema that contains the history table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    protected virtual string? TableSchema { get; }    
        private IModel EnsureModel()
    {
        if (_model == null)
        {
            var conventionSet = Dependencies.ConventionSetBuilder.CreateConventionSet();

            conventionSet.Remove(typeof(DbSetFindingConvention));
            conventionSet.Remove(typeof(RelationalDbFunctionAttributeConvention));

            var modelBuilder = new ModelBuilder(conventionSet);
            modelBuilder.Entity<SeedScriptHistoryRow>(
                x =>
                {
                    ConfigureTable(x);
                    x.ToTable(TableName, TableSchema);
                });

            _model = Dependencies.ModelRuntimeInitializer.Initialize(
                (IModel)modelBuilder.Model, designTime: true, validationLogger: null);
        }

        return _model;
    }    

    /// <summary>
    ///     Configures the entity type mapped to the history table.
    /// </summary>
    /// <remarks>
    ///     Database providers can override this to add or replace configuration.
    /// </remarks>
    /// <param name="history">A builder for the <see cref="HistoryRow" /> entity type.</param>
    protected virtual void ConfigureTable(EntityTypeBuilder<SeedScriptHistoryRow> history)
    {
        history.ToTable(DefaultTableName);
        history.HasKey(h => h.Id);
        history.Property(h => h.Name).HasMaxLength(255).IsRequired();
        history.Property(h => h.MigrationName).HasMaxLength(255).IsRequired();
        history.Property(h => h.AppliedOn).HasColumnType("datetime2(7)").IsRequired();
        history.HasIndex(h => h.MigrationName);
    }

    public bool Exists()        
    => Dependencies.DatabaseCreator.Exists()
            && InterpretExistsResult(
                Dependencies.RawSqlCommandBuilder.Build(ExistsSql).ExecuteScalar(
                    new RelationalCommandParameterObject(
                        Dependencies.Connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger, CommandSource.Migrations)));
    protected string ExistsSql
    {
        get
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return "SELECT OBJECT_ID("
                + stringTypeMapping.GenerateSqlLiteral(
                    SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                + ")"
                + SqlGenerationHelper.StatementTerminator;
        }
    }

    protected bool InterpretExistsResult(object? value)
    => value != DBNull.Value;

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    => await Dependencies.DatabaseCreator.ExistsAsync(cancellationToken).ConfigureAwait(false)
        && InterpretExistsResult(
            await Dependencies.RawSqlCommandBuilder.Build(ExistsSql).ExecuteScalarAsync(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, CommandSource.Migrations),
                cancellationToken).ConfigureAwait(false));

    public IReadOnlyList<SeedScriptHistoryRow> GetAppliedSeedScripts()
    {
        var rows = new List<SeedScriptHistoryRow>();

        if (Exists())
        {
            var command = Dependencies.RawSqlCommandBuilder.Build(GetAppliedSeedScriptSql);

            using var reader = command.ExecuteReader(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, CommandSource.Migrations));
            while (reader.Read())
            {
                rows.Add(new SeedScriptHistoryRow(reader.DbDataReader.GetInt32(0), 
                                                    reader.DbDataReader.GetString(1), 
                                                    reader.DbDataReader.GetString(2), 
                                                    reader.DbDataReader.GetDateTime(3)));
            }
        }

        return rows;
    }

    public async Task<IReadOnlyList<SeedScriptHistoryRow>> GetAppliedSeedScriptsAsync(CancellationToken cancellationToken = default)
    {
        var rows = new List<SeedScriptHistoryRow>();

        if (await ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            var command = Dependencies.RawSqlCommandBuilder.Build(GetAppliedSeedScriptSql);

            var reader = await command.ExecuteReaderAsync(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, CommandSource.Migrations),
                cancellationToken).ConfigureAwait(false);

            await using var _ = reader.ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                rows.Add(new SeedScriptHistoryRow(reader.DbDataReader.GetInt32(0), 
                                                    reader.DbDataReader.GetString(1), 
                                                    reader.DbDataReader.GetString(2), 
                                                    reader.DbDataReader.GetDateTime(3)));
            }
        }

        return rows;
    }

    public IReadOnlyList<SeedScriptHistoryRow> GetAppliedSeedScriptsForMigration(string migrationName)
    {
        var rows = new List<SeedScriptHistoryRow>();

        if (Exists())
        {
            var command = Dependencies.RawSqlCommandBuilder.Build(GetSeedScriptsForMigrationSql(migrationName));    

            using var reader = command.ExecuteReader(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, CommandSource.Migrations));
            while (reader.Read())
            {
                rows.Add(new SeedScriptHistoryRow(reader.DbDataReader.GetString(1), 
                                                    migrationName));
            }
        }

        return rows;
    }

    public async Task<IReadOnlyList<SeedScriptHistoryRow>> GetAppliedSeedScriptsForMigrationAsync(string migrationName, CancellationToken cancellationToken = default)
    {
        var rows = new List<SeedScriptHistoryRow>();

        if (await ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            var command = Dependencies.RawSqlCommandBuilder.Build(GetSeedScriptsForMigrationSql(migrationName));    

            var reader = await command.ExecuteReaderAsync(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    Dependencies.CurrentContext.Context,
                    Dependencies.CommandLogger, 
                    CommandSource.FromSqlQuery),
                cancellationToken).ConfigureAwait(false);

            await using var _ = reader.ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                rows.Add(new SeedScriptHistoryRow(reader.DbDataReader.GetString(0), 
                                                    migrationName));
            }
        }

        return rows;
    }

    public async Task<SeedScriptHistoryRow> InsertHistoryRow(string name, string migrationName, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!await ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            await createSeedScriptHistoryTable(cancellationToken).ConfigureAwait(false);
        }

        SeedScriptHistoryRow row = new SeedScriptHistoryRow(name, migrationName);
        IRelationalCommand command = Dependencies.RawSqlCommandBuilder.Build(GetInsertScript(row));

        await command.ExecuteNonQueryAsync(
            new RelationalCommandParameterObject(
                Dependencies.Connection,
                null,
                null,
                Dependencies.CurrentContext.Context,
                Dependencies.CommandLogger, 
                CommandSource.FromSqlQuery
            ), 
            cancellationToken).ConfigureAwait(false);

        return row;
    }

    private async Task createSeedScriptHistoryTable(CancellationToken cancellationToken = default(CancellationToken))
    {
        IRelationalCommand command = Dependencies.RawSqlCommandBuilder.Build(GetCreateScript());

        await command.ExecuteNonQueryAsync(
            new RelationalCommandParameterObject(
                Dependencies.Connection,
                null,
                null,
                Dependencies.CurrentContext.Context,
                Dependencies.CommandLogger, 
                CommandSource.FromSqlQuery
            ), 
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Generates a SQL script to insert a row into the history table.
    /// </summary>
    /// <param name="row">The row to insert, represented as a <see cref="HistoryRow" /> entity.</param>
    /// <returns>The generated SQL.</returns>
    public virtual string GetInsertScript(SeedScriptHistoryRow row)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
        var dateTimeTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(DateTime));

        return new StringBuilder().Append("INSERT INTO ")
            .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append(" (")
            .Append(SqlGenerationHelper.DelimitIdentifier("Name"))
            .Append(", ")
            .Append(SqlGenerationHelper.DelimitIdentifier("MigrationName"))
            .Append(", ")
            .Append(SqlGenerationHelper.DelimitIdentifier("AppliedOn"))
            .AppendLine(")")
            .Append("VALUES (")
            .Append(stringTypeMapping.GenerateSqlLiteral(row.Name))
            .Append(", ")
            .Append(stringTypeMapping.GenerateSqlLiteral(row.MigrationName))
            .Append(", ")
            .Append(dateTimeTypeMapping.GenerateSqlLiteral(row.AppliedOn))
            .Append(')')
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .ToString();
    }


    /// <summary>
    ///     Generates a SQL script that will create the history table.
    /// </summary>
    /// <returns>The SQL script.</returns>
    public virtual string GetCreateScript()
    {
        var model = EnsureModel();

        var operations = Dependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel());
        var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, model);

        return string.Concat(commandList.Select(c => c.CommandText));
    }
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string GetBeginIfNotExistsScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .AppendLine("IF NOT EXISTS (")
            .Append("    SELECT * FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("    WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier("Id"))
            .Append(" = ").AppendLine(stringTypeMapping.GenerateSqlLiteral("migrationId"))
            .AppendLine(")")
            .Append("BEGIN")
            .ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string GetBeginIfExistsScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .AppendLine("IF EXISTS (")
            .Append("    SELECT * FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("    WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier("Id"))
            .Append(" = ")
            .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
            .AppendLine(")")
            .Append("BEGIN")
            .ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string GetEndIfScript()
        => new StringBuilder()
            .Append("END")
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .ToString();

    /// <summary>
    ///     Generates SQL to query for the migrations that have been applied.
    /// </summary>
    protected virtual string GetAppliedSeedScriptSql
        => new StringBuilder()
            .Append("SELECT ")
            .Append(SqlGenerationHelper.DelimitIdentifier("Id"))
            .Append(", ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier("Name"))
            .Append(", ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier("MigrationName"))
            .Append(", ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier("AppliedOn"))
            .Append("FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("ORDER BY ")
            .Append(SqlGenerationHelper.DelimitIdentifier("Id"))
            .Append(SqlGenerationHelper.StatementTerminator)
            .ToString();

    /*
        SELECT Name
        FROM __SeedDataMigrationsHistory
        where MigrationName = "{@MigrationName}"
    */
    protected virtual string GetSeedScriptsForMigrationSql(string migrationName)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .Append("SELECT ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier("Name"))
            .Append("FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier("MigrationName"))
            .Append(" = ")
            .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationName))
            .Append(SqlGenerationHelper.StatementTerminator)
            .ToString();
    }
}
