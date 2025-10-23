using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using IEquipmentRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentRepository;

namespace Calibrator.WpfApplication.Features.EquipmentsOverview.Queries;

public class GetEquipmentQuery
{
    private readonly IEquipmentRepository _equipmentRepository;

    public GetEquipmentQuery(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }
    
    public async Task<Equipment?> Execute(Guid id)
    {
        var equipment = await _equipmentRepository.TryGetWithNoTracking(id);

        return equipment;
    }
}
