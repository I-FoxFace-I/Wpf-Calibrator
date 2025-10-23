using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Entities;

public class EquipmentTemplate : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public EquipmentType Type { get; set; }
    public UnitType MeasurementUnit { get; set; }
    public decimal MinimumTorque { get; set; }
    public decimal MaximumTorque { get; set; }

    public static EquipmentTemplate CreateNew() => new();

    public void Upsert(UpsertEquipmentTemplateDto dto)
    {
        if (dto.Id != Guid.Empty) Id = dto.Id;
        Name = dto.Name;
        Type = dto.Type;
        MeasurementUnit = dto.MeasurementUnit;
        MinimumTorque = dto.MinimumTorque;
        MaximumTorque = dto.MaximumTorque;
    }
}


