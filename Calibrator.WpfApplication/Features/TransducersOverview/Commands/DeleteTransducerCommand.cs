using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

namespace Calibrator.WpfApplication.Features.TransducersOverview.Commands;

public class DeleteTransducerCommand
{
    private readonly ITransducerRepository _transducerRepository;

    public DeleteTransducerCommand(ITransducerRepository transducerRepository)
    {
        _transducerRepository = transducerRepository;
    }

    public async Task Execute(Guid id)
    {
        await _transducerRepository.Delete(id);
    }
}
