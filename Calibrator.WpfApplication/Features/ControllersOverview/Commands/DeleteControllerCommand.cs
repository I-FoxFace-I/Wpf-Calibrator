using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using IControllerRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IControllerRepository;

namespace Calibrator.WpfApplication.Features.ControllersOverview.Commands;

public class DeleteControllerCommand
{
    private readonly IControllerRepository _controllerRepository;

    public DeleteControllerCommand(IControllerRepository controllerRepository)
    {
        _controllerRepository = controllerRepository;
    }

    public async Task Execute(Guid id)
    {
        await _controllerRepository.Delete(id);
    }
}
