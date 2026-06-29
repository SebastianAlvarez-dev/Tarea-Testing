using Api.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Api.FunctionalTests.Infrastructure;

public abstract class FunctionalTestFixture
{
    private static readonly FunctionalTestApplication Application = new();

    protected ISender Sender => GetRequiredService<ISender>();

    protected ApplicationDbContext DbContext => GetRequiredService<ApplicationDbContext>();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await Application.StartAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await Application.ResetDatabaseAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Application.DisposeAsync();
    }

    private TService GetRequiredService<TService>()
        where TService : notnull
    {
        return Application.Services.GetRequiredService<TService>();
    }
}
