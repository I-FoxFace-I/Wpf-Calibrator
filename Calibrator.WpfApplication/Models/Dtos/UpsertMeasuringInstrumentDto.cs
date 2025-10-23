using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Dtos;

public record UpsertMeasuringInstrumentDto(
    Guid Id,
    string Name,
    string SerialNumber,
    ToolConnectionMethod ConnectionMethod,
    MeasuringInstrumentType Type);


