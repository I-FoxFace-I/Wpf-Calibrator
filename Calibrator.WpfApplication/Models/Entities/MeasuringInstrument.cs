using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Entities;

public class MeasuringInstrument : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public ToolConnectionMethod ConnectionMethod { get; set; }
    public MeasuringInstrumentType Type { get; set; }

    public static MeasuringInstrument CreateNew() => new();

    public void Upsert(UpsertMeasuringInstrumentDto dto)
    {
        if (dto.Id != Guid.Empty) Id = dto.Id;
        Name = dto.Name;
        SerialNumber = dto.SerialNumber;
        ConnectionMethod = dto.ConnectionMethod;
        Type = dto.Type;
    }
}


