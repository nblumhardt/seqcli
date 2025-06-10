using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Seq.Api;
// ReSharper disable UnusedMember.Global

namespace SeqCli.Mcp.Tools;

[McpServerToolType]
class VisiblePropertiesTool(SeqConnection connection)
{
    [McpServerTool(
         Name = "seq_list_user_defined_properties",
         Destructive = false,
         OpenWorld = true,
         Idempotent = false,
         ReadOnly = true),
     Description("Lists the user-defined properties found on the most recent 10,000 events in Seq. " +
                 "Bare names correspond with properties in the `@Properties` collection.")]
    public async Task<string[]> ListBuiltInProperties(CancellationToken cancel)
    {
        var names = new HashSet<string>();
        
        await foreach (var evt in connection.Events.EnumerateAsync(count: 10_000, cancellationToken: cancel))
        {
            foreach (var prop in evt.Properties ?? [])
            {
                names.Add(IsValidIdentifier(prop.Name) ?
                    prop.Name :
                    $"@Properties[{ToStringLiteral(prop.Name)}]");
            }

            foreach (var rp in evt.Resource ?? [])
            {
                names.Add(IsValidIdentifier(rp.Name) ?
                    $"@Resource.{rp.Name}" :
                    $"@Resource[{ToStringLiteral(rp.Name)}]");
            }

            foreach (var sp in evt.Scope ?? [])
            {
                names.Add(IsValidIdentifier(sp.Name) ?
                    $"@Scope.{sp.Name}" :
                    $"@Scope[{ToStringLiteral(sp.Name)}]");
            }
        }

        return names
            .OrderBy(n => n)
            .Take(500)
            .ToArray();
    }

    static string ToStringLiteral(string propName)
    {
        return $"'{propName.Replace("'", "''")}'";
    }

    static bool IsValidIdentifier(string name)
    {
        return name.Length > 0 &&
               (char.IsLetter(name[0]) || name[0] == '_') &&
               name.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
    }
}