using System.ComponentModel;
using ModelContextProtocol.Server;

namespace SeqCli.Mcp.Prompts;

[McpServerPromptType]
class QueryConstructionPrompts
{
    [McpServerPrompt(Name = "write_search"), Description("A prompt without arguments")]
    public string WriteSearch() =>
        """
        Your goal is to write syntactically-valid search expressions for Seq.
        
        The expressions must only contain keywords, functions, and properties (built-in,
        and others from structured event data) that you can verify are present and supported
        by the current Seq server, using available tools.
        
        Expressions use a SQL-style syntax. Assume the standard SQL infix
        math and comparison operators are available.
        
        Answer concisely, and don't provide variations or improvements
        without being asked. 
        """;
}
