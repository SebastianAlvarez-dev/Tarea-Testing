extern alias TestAppHost;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Data.SqlClient;
using Respawn;

namespace Api.FunctionalTests.Infrastructure;

public sealed class FunctionalTestApplication : IAsyncDisposable
{
    private DistributedApplication? _app;
    private ApiWebApplicationFactory? _factory;
    private HttpClient? _client;
    private Respawner? _respawner;
    private string? _connectionString;
    private string? _previousConnectionStringEnvironmentValue;

    public IServiceProvider Services =>
        _factory?.Services
        ?? throw new InvalidOperationException("La aplicacion de pruebas no esta inicializada.");

    public HttpClient Client =>
        _client
        ?? throw new InvalidOperationException("El cliente HTTP de pruebas no esta inicializado.");

    public async Task StartAsync()
    {
        if (_app is not null)
            return;

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

        _factory = new ApiWebApplicationFactory(_connectionString);
        _ = _factory.Services;
        _client = _factory.CreateClient();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(timeout.Token);

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.SqlServer }
        );
    }

    public async Task ResetDatabaseAsync()
    {
        if (_connectionString is null || _respawner is null)
            throw new InvalidOperationException(
                "La base de datos de pruebas no esta inicializada."
            );

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        _client = null;

        if (_factory is not null)
            await _factory.DisposeAsync();
        _factory = null;

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__bd",
            _previousConnectionStringEnvironmentValue
        );
        _previousConnectionStringEnvironmentValue = null;

        if (_app is not null)
            await _app.DisposeAsync();
        _app = null;
        _respawner = null;
        _connectionString = null;
    }
}
