using System.Text.Json;
using System.Text.Json.Serialization;
using EngieChallenge.CORE.Domain.Enums;

public class PowerPlantJsonConverter : JsonConverter<PowerPlant>
{
    public override PowerPlant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dto = JsonSerializer.Deserialize<PowerPlantDto>(ref reader, options);
        return dto != null ? MapToDomain(dto) : null;
    }

    public override void Write(Utf8JsonWriter writer, PowerPlant value, JsonSerializerOptions options)
    {
        var dto = MapToDto(value);
        JsonSerializer.Serialize(writer, dto, options);
    }

    private PowerPlant MapToDomain(PowerPlantDto dto)
    {
        PowerPlant powerPlant = dto.Type switch
        {
            PowerPlantType.windturbine => new WindTurbine(),
            PowerPlantType.gasfired => new GasFired(),
            PowerPlantType.turbojet => new TurboJet(),
            _ => throw new ArgumentException($"Unknown PowerPlantType: {dto.Type}")
        };

        powerPlant.Name = dto.Name;
        powerPlant.PMax = dto.PMax;
        powerPlant.PMin = dto.PMin;
        powerPlant.Efficiency = dto.Efficiency;

        return powerPlant;
    }

    private PowerPlantDto MapToDto(PowerPlant powerPlant)
    {
        PowerPlantType type = powerPlant switch
        {
            WindTurbine => PowerPlantType.windturbine,
            GasFired => PowerPlantType.gasfired,
            TurboJet => PowerPlantType.turbojet,
            _ => throw new ArgumentException("Unknown PowerPlantType")
        };

        return new PowerPlantDto
        {
            Name = powerPlant.Name,
            Type = type,
            PMax = powerPlant.PMax,
            PMin = powerPlant.PMin,
            Efficiency = powerPlant.Efficiency
        };
    }
}
