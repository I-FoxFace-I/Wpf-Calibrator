using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public class MockControllerRepository : MockRepositoryBase<Controller>, IControllerRepository
{
    public MockControllerRepository()
    {
        // Seed with some test data
        var controller1 = Controller.CreateNew();
        controller1.Upsert(new UpsertControllerDto(
            Guid.Empty,
            "PowerFocus 6000 - Line 1",
            "PF6000-001",
            "192.168.1.10",
            ToolConnectionMethod.Ethernet,
            ControllerType.PowerFocus6000
        ));
        _data.Add(controller1.Id, controller1);

        var controller2 = Controller.CreateNew();
        controller2.Upsert(new UpsertControllerDto(
            Guid.Empty,
            "PowerMACS 4000 - Station A",
            "PM4000-002",
            "COM3",
            ToolConnectionMethod.Serial,
            ControllerType.PowerMACS
        ));
        _data.Add(controller2.Id, controller2);

        var controller3 = Controller.CreateNew();
        controller3.Upsert(new UpsertControllerDto(
            Guid.Empty,
            "Torque Analyzer - Test Bay",
            "TA-003",
            "USB1",
            ToolConnectionMethod.USB,
            ControllerType.TorqueAnalyzer
        ));
        _data.Add(controller3.Id, controller3);
    }
}
