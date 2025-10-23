using System;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Queries;

public class GetMeasuringInstrumentQuery
{
    private readonly IMeasuringInstrumentRepository _measuringInstrumentRepository;

    public GetMeasuringInstrumentQuery(IMeasuringInstrumentRepository measuringInstrumentRepository)
    {
        _measuringInstrumentRepository = measuringInstrumentRepository;
    }
    
    public async Task<MeasuringInstrument?> Execute(Guid id)
    {
        var measuringInstrument = await _measuringInstrumentRepository.TryGetWithNoTracking(id);

        return measuringInstrument;
    }
}
