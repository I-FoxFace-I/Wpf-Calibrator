using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using IControllerRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IControllerRepository;

namespace Calibrator.WpfApplication.Features.ControllersOverview.Queries;

public class GetControllerQuery
{
    private readonly IControllerRepository _controllerRepository;

    public GetControllerQuery(IControllerRepository controllerRepository)
    {
        _controllerRepository = controllerRepository;
    }
    
    public async Task<Controller?> Execute(Guid id)
    {
        var controller = await _controllerRepository.TryGetWithNoTracking(id);

        return controller;
    }
}
