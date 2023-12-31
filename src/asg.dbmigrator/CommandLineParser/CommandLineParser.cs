namespace asg.dbmigrator.CommandLineParser;

public class CommandLineArgParser
{
    string? Args { get; set; }

    Dictionary<string, CommandLineOption> options = new Dictionary<string, CommandLineOption>();
    public CommandLineArgParser(string[] args)
    {
        if ( args is not null )
            Args = string.Join(' ', args);
    }

    public void Add<T>(string key, CommandLineOption<T> option)
    {        
        options.Add(key, option);
    }

    public ICommandLineArgs Parse()
    {
        Dictionary<string, CommandLineOption> parsedOptions = new Dictionary<string, CommandLineOption>();
        if ( string.IsNullOrWhiteSpace(Args) )
            return new CommandLineArgs(new Dictionary<string, CommandLineOption>());

        if ( !options.Any() )
            return new CommandLineArgs(new Dictionary<string, CommandLineOption>());

        string[] args = Args.Split(' ');

        for (int i = 0; i < args.Length; i++ )
        {
            if ( !IsOption(args, i) )
                continue;

            string arg = args[i];

            ReadOnlySpan<char> optionSpan = arg.AsSpan(1, arg.Length - 1);
            
            options.TryGetValue(optionSpan.ToString(), out CommandLineOption? argOption);

            if ( argOption is null )
                continue;

            if ( argOption.IsFlag )
            {
                argOption.ValueString = "true";
                parsedOptions.Add(arg, argOption);
                continue;
            }
            else
            {
                i++;            
                List<string> values = new List<string>();

                while ( i < args.Length && !IsOption(args, i) )
                {
                    values.Add(args[i]);

                    if ( (i+1) < args.Length && IsOption(args, i+1))
                        break;

                    i++;
                }

                if ( values.Any() )
                {
                    argOption.ValueString = string.Join(' ', values);
                }
                else
                {
                    continue;
                }
            }

            parsedOptions.Add(arg, argOption);
        }

        return new CommandLineArgs(parsedOptions);
    }

    private static bool IsOption(string[] args, int idx)
    {
        return args[idx].StartsWith('-');
    }
}
