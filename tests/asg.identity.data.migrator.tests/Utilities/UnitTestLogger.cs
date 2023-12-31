using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace asg.data.migrator.tests;

public class UnitTestLogger<T> : ILogger<T> where T : class
{
    public UnitTestLogger(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        if ( state.GetType() == typeof(IEnumerable<KeyValuePair<string, object>>))
            return ((IEnumerable<KeyValuePair<string, object>>)state).GetEnumerator();

        return ((IEnumerable<T>)state).GetEnumerator();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        StringBuilder log = new StringBuilder();

        log.AppendFormat("[{0}] ", logLevel.ToString());
        log.Append(formatter(state, exception));

        TestOutputHelper.WriteLine(log.ToString());
    }
}
