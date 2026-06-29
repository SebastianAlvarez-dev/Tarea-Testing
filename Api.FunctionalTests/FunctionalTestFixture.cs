extern alias TestAppHost;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace Api.FunctionalTests;

public abstract class FunctionalTestFixture
{
    private DistributedApplication? _app;
    private Respawner? _respawner;
    private string? _connectionString;
    private string? _previousConnectionStringEnvironmentValue;

    protected ApiWebApplicationFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder =
            await DistributedApplicationTestingBuilder.CreateAsync<TestAppHost::Projects.Api_FunctionalTests_AppHost>();

        _app = await builder.BuildAsync();
        await _app.StartAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("bd", timeout.Token);

        _connectionString =
            await _app.GetConnectionStringAsync("bd", timeout.Token)
            ?? throw new InvalidOperationException(
                "No se pudo obtener la cadena de conexion 'bd'."
            );

        _previousConnectionStringEnvironmentValue = Environment.GetEnvironmentVariable(
            "ConnectionStrings__bd"
        );
        Environment.SetEnvironmentVariable("ConnectionStrings__bd", _connectionString);

        Factory = new ApiWebApplicationFactory(_connectionString);

        _ = Factory.Services;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(timeout.Token);

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.SqlServer }
        );
    }

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Factory.DisposeAsync();
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__bd",
            _previousConnectionStringEnvironmentValue
        );

        if (_app is not null)
            await _app.DisposeAsync();
    }

    protected IServiceScope CreateScope()
    {
        return Factory.Services.CreateScope();
    }

    private async Task ResetDatabaseAsync()
    {
        if (_connectionString is null || _respawner is null)
            throw new InvalidOperationException(
                "La base de datos de pruebas no esta inicializada."
            );

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }
}

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("FunctionalTests");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?> { ["ConnectionStrings:bd"] = _connectionString }
                );
            }
        );
    }
}
