using System.ComponentModel.DataAnnotations;

namespace asg.dbmigrator.DbMigration.Models;

public class SeedScriptHistoryRow
{
    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    [Required]
    public string Name { get; set; }

    [MaxLength(255)]
    public string MigrationName { get; set; }

    [Required]
    public DateTime AppliedOn { get; set; } 

    public SeedScriptHistoryRow()
    {
        Name = string.Empty;
        MigrationName = string.Empty;
        AppliedOn = DateTime.UtcNow;
    }

    public SeedScriptHistoryRow(string name, string migrationName)
        : this (0, name, migrationName, DateTime.UtcNow) {}

    public SeedScriptHistoryRow(int id, string name, string migrationName, DateTime appliedOn)    
    {
        Id = id;
        Name = name;
        MigrationName = migrationName;
        AppliedOn = appliedOn;
    }
}
