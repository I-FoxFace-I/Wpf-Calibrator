using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

namespace Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Commands;

public class DeleteMeasuringInstrumentCommand
{
    private readonly IMeasuringInstrumentRepository _measuringInstrumentRepository;

    public DeleteMeasuringInstrumentCommand(IMeasuringInstrumentRepository measuringInstrumentRepository)
    {
        _measuringInstrumentRepository = measuringInstrumentRepository;
    }

    public async Task Execute(Guid id)
    {
        await _measuringInstrumentRepository.Delete(id);
    }
}
