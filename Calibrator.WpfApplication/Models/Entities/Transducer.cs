using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Entities;

public class Transducer : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal MinimumCapacity { get; set; }
    public decimal MaximumCapacity { get; set; }
    public TransducersType Type { get; set; }
    public UnitType CapacityUnit { get; set; }
    public UnitType MeasurementUnit { get; set; }

    public static Transducer CreateNew() => new();

    public void Upsert(UpsertTransducerDto dto)
    {
        if (dto.Id != Guid.Empty) Id = dto.Id;
        Name = dto.Name;
        SerialNumber = dto.SerialNumber;
        MinimumCapacity = dto.MinimumCapacity;
        MaximumCapacity = dto.MaximumCapacity;
        Type = dto.Type;
        CapacityUnit = dto.CapacityUnit;
        MeasurementUnit = dto.MeasurementUnit;
    }
}


