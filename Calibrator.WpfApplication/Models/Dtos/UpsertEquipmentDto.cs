using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Dtos;

public record UpsertEquipmentDto(
    Guid Id,
    Guid EquipmentTemplateId,
    string SerialNumber,
    string Identifier,
    UnitType MeasurementUnit,
    decimal MinimumTorque,
    decimal MaximumTorque,
    decimal MinimumAngle,
    decimal MaximumAngle);


