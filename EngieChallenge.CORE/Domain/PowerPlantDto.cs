using System.Text.Json.Serialization;
using EngieChallenge.CORE.Domain.Enums;

namespace EngieChallenge.CORE.Domain;

public class PowerPlantDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PowerPlantTypeEnum Type { get; set; }

    [JsonPropertyName("efficiency")]
    public decimal Efficiency { get; set; }

    [JsonPropertyName("pmin")]
    public decimal PMin { get; set; }

    [JsonPropertyName("pmax")]
    public decimal PMax { get; set; }
}