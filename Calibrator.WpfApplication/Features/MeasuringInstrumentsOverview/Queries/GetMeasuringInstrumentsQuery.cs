using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Queries;

public class GetMeasuringInstrumentsQuery
{
    private readonly IMeasuringInstrumentRepository _measuringInstrumentRepository;

    public GetMeasuringInstrumentsQuery(IMeasuringInstrumentRepository measuringInstrumentRepository)
    {
        _measuringInstrumentRepository = measuringInstrumentRepository;
    }

    public async Task<List<MeasuringInstrument>> Execute()
    {
        var measuringInstruments = await _measuringInstrumentRepository.GetAllWithNoTracking();

        return measuringInstruments;
    }
}
