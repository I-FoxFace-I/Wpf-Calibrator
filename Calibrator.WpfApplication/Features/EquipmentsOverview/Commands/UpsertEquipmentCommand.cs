using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using IEquipmentRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentRepository;

namespace Calibrator.WpfApplication.Features.EquipmentsOverview.Commands;

public class UpsertEquipmentCommand
{
    private readonly IEquipmentRepository _equipmentRepository;

    public UpsertEquipmentCommand(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public async Task Execute(UpsertEquipmentDto upsertCommand)
    {
        var equipment = 
            await _equipmentRepository.TryGet(upsertCommand.Id) 
            ?? Equipment.CreateNew(upsertCommand.EquipmentTemplateId);
        
        equipment.Upsert(upsertCommand);

        await _equipmentRepository.Upsert(equipment);
    }
}
