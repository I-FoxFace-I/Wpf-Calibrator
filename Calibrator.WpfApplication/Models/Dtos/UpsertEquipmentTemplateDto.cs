using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Dtos;

public record UpsertEquipmentTemplateDto(
    Guid Id,
    string Name,
    EquipmentType Type,
    UnitType MeasurementUnit,
    decimal MinimumTorque,
    decimal MaximumTorque);


