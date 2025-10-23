using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Dtos;

public record UpsertControllerDto(
    Guid Id,
    string Name,
    string SerialNumber,
    string Identifier,
    ToolConnectionMethod ConnectionMethod,
    ControllerType Type);


