using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using IEquipmentTemplateRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentTemplateRepository;

namespace Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Queries;

public class GetEquipmentTemplatesQuery
{
    private readonly IEquipmentTemplateRepository _equipmentTemplateRepository;

    public GetEquipmentTemplatesQuery(IEquipmentTemplateRepository equipmentTemplateRepository)
    {
        _equipmentTemplateRepository = equipmentTemplateRepository;
    }
    
    public async Task<List<EquipmentTemplate>> Execute()
    {
        var equipmentTemplates = await _equipmentTemplateRepository.GetAllWithNoTracking();

        return equipmentTemplates;
    }
}
