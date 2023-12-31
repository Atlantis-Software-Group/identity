namespace asg.dbmigrator.CommandLineParser;

public class CommandLineArgs : ICommandLineArgs
{
    public CommandLineArgs(Dictionary<string, CommandLineOption> options)
    {
        Options = options;
    }

    public Dictionary<string, CommandLineOption> Options { get; set; }

    public T? GetValue<T>(string key)
    {
        if ( Options.ContainsKey(key) )
        {
            if ( !Options[key].IsCollection )
            {
                CommandLineOption<T> selectedOption = (CommandLineOption<T>)Options[key];
                return selectedOption.GetValue();
            }
        }

        return default;
    }

    public List<T>? GetCollectionValue<T>(string key)
    {
        List<T>? values = null;

        if ( Options.ContainsKey(key) )
        {
            if (Options[key].IsCollection)
            {
                CommandLineOption<T, List<T>> selectedOption = (CommandLineOption<T, List<T>>)Options[key];
                return selectedOption.GetCollectionValue();
            }
        }

        return values;
    }
}
