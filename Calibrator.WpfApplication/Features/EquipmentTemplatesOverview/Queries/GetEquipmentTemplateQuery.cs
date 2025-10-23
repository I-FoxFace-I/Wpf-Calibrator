using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using IEquipmentTemplateRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentTemplateRepository;

namespace Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Queries;

public class GetEquipmentTemplateQuery
{
    private readonly IEquipmentTemplateRepository _equipmentTemplateRepository;

    public GetEquipmentTemplateQuery(IEquipmentTemplateRepository equipmentTemplateRepository)
    {
        _equipmentTemplateRepository = equipmentTemplateRepository;
    }
    
    public async Task<EquipmentTemplate?> Execute(Guid id)
    {
        var equipmentTemplate = await _equipmentTemplateRepository.TryGetWithNoTracking(id);

        return equipmentTemplate;
    }
}
