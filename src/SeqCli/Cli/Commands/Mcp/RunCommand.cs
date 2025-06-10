using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using SeqCli.Cli.Features;
using SeqCli.Connection;
using SeqCli.Mcp.Prompts;
using SeqCli.Mcp.Tools;
using Serilog;

namespace SeqCli.Cli.Commands.Mcp;

[Command("mcp", "run", "Run a Model Context Protocol server on STDIO", McpHidden = true)]
class RunCommand: Command
{
    readonly SeqConnectionFactory _connectionFactory;
        
    readonly ConnectionFeature _connection;

    public RunCommand(SeqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        _connection = Enable<ConnectionFeature>();
    }

    protected override async Task<int> Run()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.ConfigureContainer(new AutofacServiceProviderFactory(cb =>
        {
            cb.RegisterModule<SeqCliModule>();
        }));
        builder.Services.AddSerilog();
        builder.Services.AddSingleton(_ => _connectionFactory.Connect(_connection));
        // builder.Services.AddSingleton<CliCommandsTool>();
        
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithPrompts([
                typeof(QueryConstructionPrompts)
            ])
            .WithTools([
                typeof(BuiltInPropertiesTool),
                typeof(VisiblePropertiesTool)
            ]);
            // .WithListToolsHandler((req, _) =>
            //     req.Services!
            //         .GetRequiredService<CliCommandsTool>()
            //         .ListCommands())
            // .WithCallToolHandler((req, cancel) => req.Services!
            //     .GetRequiredService<CliCommandsTool>()
            //     .InvokeCommand(req, cancel));
        

        await builder.Build().RunAsync();
        return 0;
    }
}