using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Features.TransducersOverview.Queries;

public class GetTransducerQuery
{
    private readonly ITransducerRepository _transducerRepository;

    public GetTransducerQuery(ITransducerRepository transducerRepository)
    {
        _transducerRepository = transducerRepository;
    }
    
    public async Task<Transducer?> Execute(Guid id)
    {
        var transducer = await _transducerRepository.TryGetWithNoTracking(id);

        return transducer;
    }
}
