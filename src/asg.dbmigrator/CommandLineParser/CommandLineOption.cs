namespace asg.dbmigrator.CommandLineParser;

public abstract class CommandLineOption
{
    public abstract Type Type { get; } 

    public abstract Type CollectionType { get; }

    public virtual bool IsCollection { get; } = false;

    public bool IsFlag {
        get {
            return Type == typeof(bool);
        }
    }

    public string? ValueString { get; set; }    
}

public class CommandLineOption<T> : CommandLineOption
{
    public string Name { get; set; }

    public override Type Type 
    {
        get {
            return typeof(T);
        }
    }

    public override Type CollectionType => typeof(T);

    public CommandLineOption(string name)
    {
        Name = name;
    }

    public virtual T? GetValue()
    {
        return (T?)Convert.ChangeType(ValueString, Type);
    }
}

public class CommandLineOption<T, T1> : CommandLineOption<T>                                                             
{
    public override Type CollectionType
    {
        get {
            return typeof(T1);
        }
    }

    public override bool IsCollection { get; } = true;
    public CommandLineOption(string name) : base(name)
    {
    }

    public List<T>? GetCollectionValue()
    {
        List<T> values = new List<T>();

        if ( string.IsNullOrWhiteSpace(ValueString) )
            return values;

        string[] valuesArr = ValueString.Split(' ');

        foreach ( string val in valuesArr )
        {
            T convertedValue = (T)Convert.ChangeType(val, Type);
            values.Add(convertedValue);
        }

        return values;
    }
}