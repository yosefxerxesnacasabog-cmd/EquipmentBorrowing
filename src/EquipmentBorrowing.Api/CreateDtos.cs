using System.Text.Json.Serialization;
namespace EquipmentBorrowing.Api;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record StudentCreate(string? StudentNumber, string? FullName, string? Contact);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record EquipmentCreate(string? EquipmentCode, string? Name, string? Description);
