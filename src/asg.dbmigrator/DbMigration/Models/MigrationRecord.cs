namespace asg.dbmigrator.DbMigration.Models;

public class MigrationRecord
{
    public string Migration { get; set; }
    public bool MigrationAlreadyApplied { get; set; }

    public List<SeedDataRecord> SeedDataRecords { get; set; }

    public MigrationRecord(string migration)
        : this (migration, false) {}

    public MigrationRecord(string migration, bool migrationAlreadyApplied)
        : this (migration, migrationAlreadyApplied, new List<SeedDataRecord>()) {}

    public MigrationRecord(string migration, bool migrationAlreadyApplied, List<SeedDataRecord> seedDataRecords)
    {
        Migration = migration;
        MigrationAlreadyApplied = migrationAlreadyApplied;
        SeedDataRecords = seedDataRecords;
    }
}
