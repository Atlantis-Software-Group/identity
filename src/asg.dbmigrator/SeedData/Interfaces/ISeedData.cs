namespace asg.dbmigrator.SeedData.Interfaces;

public interface ISeedData
{
    Task<bool> Seed();
}
