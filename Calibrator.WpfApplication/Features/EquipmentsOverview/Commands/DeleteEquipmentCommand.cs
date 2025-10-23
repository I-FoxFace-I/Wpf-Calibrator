using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using IEquipmentRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IEquipmentRepository;

namespace Calibrator.WpfApplication.Features.EquipmentsOverview.Commands;

public class DeleteEquipmentCommand
{
    private readonly IEquipmentRepository _equipmentRepository;

    public DeleteEquipmentCommand(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public async Task Execute(Guid id)
    {
        await _equipmentRepository.Delete(id);
    }
}
