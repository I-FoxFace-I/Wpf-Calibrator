using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public class MockTransducerRepository : MockRepositoryBase<Transducer>, ITransducerRepository
{
    public MockTransducerRepository()
    {
        // Seed with some test data
        var transducer1 = Transducer.CreateNew();
        transducer1.Upsert(new UpsertTransducerDto(
            System.Guid.Empty,
            "Brake Transducer B1",
            "BT-001",
            50m,
            500m,
            TransducersType.Brake,
            UnitType.Nm,
            UnitType.Nm
        ));
        _data.Add(transducer1.Id, transducer1);

        var transducer2 = Transducer.CreateNew();
        transducer2.Upsert(new UpsertTransducerDto(
            System.Guid.Empty,
            "Rotary Transducer R1",
            "RT-001",
            10m,
            1000m,
            TransducersType.Rotary,
            UnitType.lbft,
            UnitType.lbft
        ));
        _data.Add(transducer2.Id, transducer2);

        var transducer3 = Transducer.CreateNew();
        transducer3.Upsert(new UpsertTransducerDto(
            System.Guid.Empty,
            "Static Transducer S1",
            "ST-001",
            20m,
            200m,
            TransducersType.Static,
            UnitType.lbin,
            UnitType.lbin
        ));
        _data.Add(transducer3.Id, transducer3);

        var transducer4 = Transducer.CreateNew();
        transducer4.Upsert(new UpsertTransducerDto(
            System.Guid.Empty,
            "Heavy Duty Brake",
            "HDB-001",
            100m,
            2000m,
            TransducersType.Brake,
            UnitType.Nm,
            UnitType.Nm
        ));
        _data.Add(transducer4.Id, transducer4);
    }
}
