using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Commands;

public class UpsertMeasuringInstrumentCommand
{
    private readonly IMeasuringInstrumentRepository _measuringInstrumentRepository;

    public UpsertMeasuringInstrumentCommand(IMeasuringInstrumentRepository measuringInstrumentRepository)
    {
        _measuringInstrumentRepository = measuringInstrumentRepository;
    }

    public async Task Execute(UpsertMeasuringInstrumentDto upsertDto)
    {
        var measuringInstrument =
            await _measuringInstrumentRepository.TryGet(upsertDto.Id)
            ?? MeasuringInstrument.CreateNew();

        measuringInstrument.Upsert(upsertDto);

        await _measuringInstrumentRepository.Upsert(measuringInstrument);
    }
}
