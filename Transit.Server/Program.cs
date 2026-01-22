using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Transit.Server;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<ClientRegistry>();
        services.AddHostedService<ServerHost>();
        services.AddHostedService<HeartbeatMonitor>();
    })
    .Build();

await host.RunAsync();
