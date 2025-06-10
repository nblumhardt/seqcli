using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
// ReSharper disable UnusedMember.Global

namespace SeqCli.Mcp.Tools;

[McpServerToolType]
static class BuiltInPropertiesTool
{
    static readonly Dictionary<string, string> BuiltInProperties = new()
    {
        ["@Arrived"] = "An integer indicating the order in which the event arrived at the Seq server.",
        ["@Data"] = "The internal representation of the event as a single structured object.",
        ["@Elapsed"] = "The elapsed duration of a span, expressed in 100 nanosecond ticks. This is in the same " +
                        "domain as Seq's duration literals such as `1s`, `23ms`, or `3d`. Computed from " +
                        "`@Timestamp - @Start`. Only present on spans, not regular log events.",
        ["@EventType"] = "A numeric hash of the message template that was used to generate the event. The message template " +
                         "itself is in the `@MessageTemplate` property.",
        ["@Exception"] = "The exception associated with the event if any, as a string. This normally incorporates the " +
                         "exception type, message, and stack trace.",
        ["@Id"] = "The event's unique id in Seq.",
        ["@Level"] = "The logging level of the event, as a string. Values are source-dependent, so for example 'Error', " +
                     "'error', and 'err' would all be typical values.",
        ["@Message"] = "The text message associated with the event. This is often the result of substituting " +
                       "`@Properties` values into `@MessageTemplate`. For span events, this property carries the span " +
                       "name.",
        ["@MessageTemplate"] = "A message template, generally following the messagetemplates.org syntax. Message " +
                               "templates collectively identify events generated from the same line of " +
                               "logging/tracing code.",
        ["@ParentId"] = "The `@SpanId` of the parent of a given span, if any. The parent span will always " +
                        "belong to the same trace, that is, share a `@TraceId` value. Only present on spans, not regular " +
                        "log events.",
        ["@Properties"] = "An object containing the user-defined properties of a log event or span. Properties with names" +
                          "that are valid C-style identifiers can be accessed implicitly, so `RequestPath` is syntactically " +
                          "equivalent to `@Properties['RequestPath']`. Properties generally conform to naming conventions " +
                          "used throughout the Seq server - sometimes simple PascalCase names, and at other times using " +
                          "the OpenTelemetry semantic conventions. See also `@Resource` and `@Scope`, which also may carry " +
                          "OpenTelemetry-related event properties.",
        ["@Resource"] = "For an OpenTelemetry log event or span, the properties associated with the OpenTelemetry " +
                        "resource. These may match definitions in the OTel semantic conventions, but may also be " +
                        "domain-specific or user-defined.",
        ["@Scope"] = "For an OpenTelemetry log event or span, the properties associated with the OpenTelemetry " +
                        "scope. These may match definitions in the OTel semantic conventions, but may also be " +
                        "domain-specific or user-defined.",
        ["@SpanId"] = "The id that uniquely identifies a span within a trace. Log events recorded during the span " +
                      "carry the same `@SpanId` value as the span itself.",
        ["@Start"] = "The time at which the span started. The difference between the start time and `@Timestamp` is " +
                     "the `@Elapsed` time of the span. In the same units as Seq's duration literal syntax.",
        ["@Timestamp"] = "The time at which an event was recorded. Carried on all log events and spans. In the same " +
                         "units as Seq's duration literal syntax.",
        ["@TraceId"] = "The id that uniquely identifies a trace. All spans and log events within a trace carry the " +
                       "same trace id value.",
    };
    
    [McpServerTool(
         Name = "seq_list_built_in_properties",
         Destructive = false,
         OpenWorld = false,
         Idempotent = true,
         ReadOnly = true),
     Description("Lists the built-in properties supported by this Seq server version.")]
    public static string[] ListBuiltInProperties() => BuiltInProperties.Keys.ToArray();

    [McpServerTool(
         Name = "seq_describe_built_in_property",
         Destructive = false,
         OpenWorld = false,
         Idempotent = true,
         ReadOnly = true),
     Description("Provides documentation for a built-in Seq property.")]
    public static string DescribeBuiltInProperty(string name) => BuiltInProperties.GetValueOrDefault(name, "Not available.");
}
