using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public class MockMeasuringInstrumentRepository : MockRepositoryBase<MeasuringInstrument>, IMeasuringInstrumentRepository
{
    public MockMeasuringInstrumentRepository()
    {
        // Seed with some test data
        var instrument1 = MeasuringInstrument.CreateNew();
        instrument1.Upsert(new UpsertMeasuringInstrumentDto(
            System.Guid.Empty,
            "STa 6000 - Lab 1",
            "STA-001",
            ToolConnectionMethod.Ethernet,
            MeasuringInstrumentType.STa6000
        ));
        _data.Add(instrument1.Id, instrument1);

        var instrument2 = MeasuringInstrument.CreateNew();
        instrument2.Upsert(new UpsertMeasuringInstrumentDto(
            System.Guid.Empty,
            "STpad - Mobile Unit",
            "STPAD-001",
            ToolConnectionMethod.Bluetooth,
            MeasuringInstrumentType.STpad
        ));
        _data.Add(instrument2.Id, instrument2);

        var instrument3 = MeasuringInstrument.CreateNew();
        instrument3.Upsert(new UpsertMeasuringInstrumentDto(
            System.Guid.Empty,
            "Custom Analyzer",
            "CA-001",
            ToolConnectionMethod.Serial,
            MeasuringInstrumentType.Other
        ));
        _data.Add(instrument3.Id, instrument3);
    }
}
