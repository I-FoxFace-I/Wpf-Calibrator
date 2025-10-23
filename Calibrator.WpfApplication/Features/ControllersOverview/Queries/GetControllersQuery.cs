using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using IControllerRepository = Calibrator.WpfApplication.Infrastructure.Persistence.Repositories.IControllerRepository;

namespace Calibrator.WpfApplication.Features.ControllersOverview.Queries;

public class GetControllersQuery
{
    private readonly IControllerRepository _controllerRepository;

    public GetControllersQuery(IControllerRepository controllerRepository)
    {
        _controllerRepository = controllerRepository;
    }
    
    public async Task<List<Controller>> Execute()
    {
        var controllers = await _controllerRepository.GetAllWithNoTracking();

        return controllers;
    }
}
