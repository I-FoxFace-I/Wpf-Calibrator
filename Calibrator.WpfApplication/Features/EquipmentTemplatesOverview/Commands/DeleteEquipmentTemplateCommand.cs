using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

namespace Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Commands;

public class DeleteEquipmentTemplateCommand
{
    private readonly IEquipmentTemplateRepository _equipmentTemplateRepository;

    public DeleteEquipmentTemplateCommand(IEquipmentTemplateRepository equipmentTemplateRepository)
    {
        _equipmentTemplateRepository = equipmentTemplateRepository;
    }

    public async Task Execute(Guid id)
    {
        await _equipmentTemplateRepository.Delete(id);
    }
}
