using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Entities;

public class Equipment : AggregateRoot
{
    public Guid EquipmentTemplateId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public UnitType MeasurementUnit { get; set; }
    public decimal MinimumTorque { get; set; }
    public decimal MaximumTorque { get; set; }
    public decimal MinimumAngle { get; set; }
    public decimal MaximumAngle { get; set; }

    public static Equipment CreateNew(Guid equipmentTemplateId) => new() { EquipmentTemplateId = equipmentTemplateId };

    public void Upsert(UpsertEquipmentDto dto)
    {
        if (dto.Id != Guid.Empty) Id = dto.Id;
        EquipmentTemplateId = dto.EquipmentTemplateId;
        SerialNumber = dto.SerialNumber;
        Identifier = dto.Identifier;
        MeasurementUnit = dto.MeasurementUnit;
        MinimumTorque = dto.MinimumTorque;
        MaximumTorque = dto.MaximumTorque;
        MinimumAngle = dto.MinimumAngle;
        MaximumAngle = dto.MaximumAngle;
    }
}


