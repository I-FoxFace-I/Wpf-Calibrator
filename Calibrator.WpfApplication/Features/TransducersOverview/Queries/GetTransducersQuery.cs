using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Features.TransducersOverview.Queries;

public class GetTransducersQuery
{
    private readonly ITransducerRepository _transducerRepository;

    public GetTransducersQuery(ITransducerRepository transducerRepository)
    {
        _transducerRepository = transducerRepository;
    }
    
    public async Task<List<Transducer>> Execute()
    {
        var transducers = await _transducerRepository.GetAllWithNoTracking();

        return transducers;
    }
}
