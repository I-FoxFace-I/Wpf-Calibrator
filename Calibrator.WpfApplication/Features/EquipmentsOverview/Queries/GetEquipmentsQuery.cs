using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using IEquipmentRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentRepository;

namespace Calibrator.WpfApplication.Features.EquipmentsOverview.Queries;

public class GetEquipmentsQuery
{
    private readonly IEquipmentRepository _equipmentRepository;

    public GetEquipmentsQuery(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }
    
    public async Task<List<Equipment>> Execute()
    {
        var equipments = await _equipmentRepository.GetAllWithNoTracking();

        return equipments;
    }
}
