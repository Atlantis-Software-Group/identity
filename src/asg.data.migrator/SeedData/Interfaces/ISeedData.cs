namespace asg.data.migrator;

public interface ISeedData
{
    Task<bool> Seed();
}
