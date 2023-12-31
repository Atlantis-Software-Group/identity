using asg.dbmigrator.CommandLineParser;
using Microsoft.EntityFrameworkCore;

namespace asg.dbmigrator;

public class DbMigratorOptions
{
    public ICommandLineArgs ParsedArgs { get; set; } = new CommandLineArgs(new Dictionary<string, CommandLineOption>());

    public IEnumerable<Type> DbContextTypes { get; set; } = Enumerable.Empty<Type>();
}
