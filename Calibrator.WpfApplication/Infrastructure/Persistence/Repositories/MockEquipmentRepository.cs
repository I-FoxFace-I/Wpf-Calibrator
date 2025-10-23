using System;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public class MockEquipmentRepository : MockRepositoryBase<Equipment>, IEquipmentRepository
{
    public MockEquipmentRepository()
    {
        // Seed with some test data
        var templateId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var templateId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var equipment1 = Equipment.CreateNew(templateId1);
        equipment1.Upsert(new UpsertEquipmentDto(
            Guid.Empty,
            templateId1,
            "EQ-001",
            "Torque Wrench 1",
            UnitType.Nm,
            10m,
            100m,
            0m,
            360m
        ));
        _data.Add(equipment1.Id, equipment1);

        var equipment2 = Equipment.CreateNew(templateId1);
        equipment2.Upsert(new UpsertEquipmentDto(
            Guid.Empty,
            templateId1,
            "EQ-002",
            "Torque Wrench 2",
            UnitType.Nm,
            20m,
            200m,
            0m,
            360m
        ));
        _data.Add(equipment2.Id, equipment2);

        var equipment3 = Equipment.CreateNew(templateId2);
        equipment3.Upsert(new UpsertEquipmentDto(
            Guid.Empty,
            templateId2,
            "EQ-003",
            "Electric Tool 1",
            UnitType.lbft,
            15m,
            150m,
            0m,
            720m
        ));
        _data.Add(equipment3.Id, equipment3);
    }
}
