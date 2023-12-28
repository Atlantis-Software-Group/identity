using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Identity.Client;

namespace asg.data.migrator;

public abstract class CommandLineOption
{
    public abstract Type Type { get; } 

    public abstract Type CollectionType { get; }

    public bool IsFlag {
        get {
            return Type == typeof(bool);
        }
    }

    public bool IsCollection {
        get {
            try
            {
                Type collectionType = CollectionType;
                return true;
            }
            catch
            {
                return false;
            }
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

    public override Type CollectionType => throw new NotImplementedException();

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
    public CommandLineOption(string name) : base(name)
    {
    }

    public List<T>? GetCollectionValue()
    {
        List<T> values = new List<T>();

        return values;
    }
}