using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using IEquipmentTemplateRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentTemplateRepository;

namespace Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Commands;

public class UpsertEquipmentTemplateCommand
{
    private readonly IEquipmentTemplateRepository _equipmentTemplateRepository;

    public UpsertEquipmentTemplateCommand(IEquipmentTemplateRepository equipmentTemplateRepository)
    {
        _equipmentTemplateRepository = equipmentTemplateRepository;
    }

    public async Task Execute(UpsertEquipmentTemplateDto upsertCommand)
    {
        var equipmentTemplate = 
            await _equipmentTemplateRepository.TryGet(upsertCommand.Id) 
            ?? EquipmentTemplate.CreateNew();
        
        equipmentTemplate.Upsert(upsertCommand);

        await _equipmentTemplateRepository.Upsert(equipmentTemplate);
    }
}
