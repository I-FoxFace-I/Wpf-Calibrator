using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Features.TransducersOverview.Commands;

public class UpsertTransducerCommand
{
    private readonly ITransducerRepository _transducerRepository;

    public UpsertTransducerCommand(ITransducerRepository transducerRepository)
    {
        _transducerRepository = transducerRepository;
    }

    public async Task Execute(UpsertTransducerDto upsertCommand)
    {
        var transducer = 
            await _transducerRepository.TryGet(upsertCommand.Id) 
            ?? Transducer.CreateNew();
        
        transducer.Upsert(upsertCommand);

        await _transducerRepository.Upsert(transducer);
    }
}
