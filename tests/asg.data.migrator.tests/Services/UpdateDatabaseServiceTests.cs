using asg.data.migrator.DbMigration.Interfaces;
using asg.data.migrator.DbMigration.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace asg.data.migrator.tests.Services;

public class UpdateDatabaseServiceTests
{
    ILogger<UpdateDatabaseService> mockLogger { get;}
    IAssemblyInformationService mockAssemblyInformation { get; }
    IHostEnvironment mockHostEnvironment { get; }
    ISeeder mockSeeder { get; }
    UpdateDatabaseService Service { get; }

    public UpdateDatabaseServiceTests(ITestOutputHelper testOutputHelper)
    {
        mockLogger = new UnitTestLogger<UpdateDatabaseService>(testOutputHelper);
        mockAssemblyInformation = Substitute.For<IAssemblyInformationService>();
        mockHostEnvironment = Substitute.For<IHostEnvironment>();
        mockSeeder = Substitute.For<ISeeder>();
        Service = new UpdateDatabaseService(mockLogger, mockAssemblyInformation, mockHostEnvironment, mockSeeder);
    }
}
