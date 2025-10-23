using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using System.Threading.Tasks;
using IControllerRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IControllerRepository;

namespace Calibrator.WpfApplication.Features.ControllersOverview.Commands;

public class UpsertControllerCommand
{
    private readonly IControllerRepository _controllerRepository;

    public UpsertControllerCommand(IControllerRepository controllerRepository)
    {
        _controllerRepository = controllerRepository;
    }

    public async Task Execute(UpsertControllerDto upsertCommand)
    {
        var controller = 
            await _controllerRepository.TryGet(upsertCommand.Id) 
            ?? Controller.CreateNew();
        
        controller.Upsert(upsertCommand);

        await _controllerRepository.Upsert(controller);
    }
}
