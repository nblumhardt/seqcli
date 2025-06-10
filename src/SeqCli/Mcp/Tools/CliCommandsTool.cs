using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Metadata;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SeqCli.Cli;

// ReSharper disable ClassNeverInstantiated.Global

namespace SeqCli.Mcp.Tools;

class CliCommandsTool
{
    readonly List<Meta<Lazy<Command>, CommandMetadata>> _availableCommands;

    public CliCommandsTool(IEnumerable<Meta<Lazy<Command>, CommandMetadata>> availableCommands)
    {
        _availableCommands = availableCommands.ToList();
    }

    public ValueTask<ListToolsResult> ListCommands()
    {
        return ValueTask.FromResult(new ListToolsResult
        {
            Tools = _availableCommands
                .Where(tool => !tool.Metadata.McpHidden)
                .Select(tool =>
                {
                    var sw = new StringWriter();
                    tool.Value.Value.PrintUsage(sw);
                    var args = sw.ToString()
                        .Replace("     ", " ")
                        .Replace("  ", " ");
                        
                    var inputSchema = JsonSerializer.SerializeToDocument(
                        new
                        {
                           type = "object",
                           properties = new
                           {
                               args = new
                               {
                                   type = "array",
                                   items = new
                                   {
                                       type = "string"
                                   },
                                   description = "The tool accepts separated CLI-style arguments. " + args
                               }
                           },
                           required = new List<string> { "args" },
                           additionalProperties = false
                        }
                    );
                    
                    var metadata = tool.Metadata;
                    return new Tool
                    {
                        Name = AsToolName(metadata.Name, metadata.SubCommand),
                        Description = metadata.HelpText,
                        InputSchema = inputSchema.RootElement,
                        Annotations = new()
                        {
                            IdempotentHint = metadata.IdempotentHint,
                            DestructiveHint = metadata.DestructiveHint,
                            OpenWorldHint = metadata.OpenWorldHint,
                            ReadOnlyHint = metadata.ReadonlyHint
                        }
                    };
                }).ToList()
        });
    }

    static string AsToolName(string name, string? subCommand)
    {
        return subCommand != null ? $"seq_{subCommand}_{name}" : $"seq_{name}";
    }

    public async ValueTask<CallToolResponse> InvokeCommand(RequestContext<CallToolRequestParams> req, CancellationToken cancel)
    {
        var name = req.Params!.Name;
        var command =
            _availableCommands.Single(cmd => AsToolName(cmd.Metadata.Name, cmd.Metadata.SubCommand) == name);
        var args = req.Params.Arguments!["args"].EnumerateArray().Select(arg => arg.GetString() ?? "").ToArray();
        try
        {
            var stdout = new StringWriter();
            var ret = await command.Value.Value.Invoke(args, stdout);
            return new CallToolResponse
            {
                IsError = ret != 0,
                Content =
                [
                    new() { Text = stdout.ToString() }
                ]
            };
        }
        catch (Exception ex)
        {
            return new CallToolResponse
            {
                IsError = true,
                Content =
                [
                    new() { Text = ex.Message }
                ]
            };
        }
    }
}
