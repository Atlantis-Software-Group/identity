namespace asg.dbmigrator.CommandLineParser;

public interface ICommandLineArgs
{
    Dictionary<string, CommandLineOption> Options { get; set; }
    T? GetValue<T>(string key);
    List<T>? GetCollectionValue<T>(string key);
}
