using Calibrator.WpfApplication.Models.Enums;
using System;

namespace Calibrator.WpfApplication.Models.Dtos;

public record UpsertTransducerDto(
    Guid Id,
    string Name,
    string SerialNumber,
    decimal MinimumCapacity,
    decimal MaximumCapacity,
    TransducersType Type,
    UnitType CapacityUnit,
    UnitType MeasurementUnit);


