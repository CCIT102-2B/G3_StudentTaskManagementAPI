using System.Text.Json.Serialization;

namespace G3_StudentTaskManagementAPI.Models;

// Sent and received as text: "Low", "Medium", "High"
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskPriority
{
    Low,
    Medium,
    High
}