using System;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public class MockEquipmentTemplateRepository : MockRepositoryBase<EquipmentTemplate>, IEquipmentTemplateRepository
{
    public MockEquipmentTemplateRepository()
    {
        // Seed with some test data
        var template1 = EquipmentTemplate.CreateNew();
        template1.Upsert(new UpsertEquipmentTemplateDto(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Ratchet Wrench Template",
            EquipmentType.RatchetWrench,
            UnitType.Nm,
            5m,
            250m
        ));
        _data.Add(template1.Id, template1);

        var template2 = EquipmentTemplate.CreateNew();
        template2.Upsert(new UpsertEquipmentTemplateDto(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Electric Tool Template",
            EquipmentType.ElectricTool,
            UnitType.lbft,
            10m,
            300m
        ));
        _data.Add(template2.Id, template2);

        var template3 = EquipmentTemplate.CreateNew();
        template3.Upsert(new UpsertEquipmentTemplateDto(
            Guid.Empty,
            "Pneumatic Tool Template",
            EquipmentType.PneumaticTool,
            UnitType.lbin,
            50m,
            500m
        ));
        _data.Add(template3.Id, template3);
    }
}
