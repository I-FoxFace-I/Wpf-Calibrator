using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WpfEngine.Tests.Helpers;

/// <summary>
/// Custom xUnit attribute for running tests on STA thread (required for WPF UI components)
/// Usage: [STAFact] instead of [Fact]
/// </summary>
[XunitTestCaseDiscoverer("WpfEngine.Tests.Helpers.STAFactDiscoverer", "WpfEngine.Tests")]
public class STAFactAttribute : FactAttribute
{
}

/// <summary>
/// Custom xUnit test case discoverer for STA tests
/// </summary>
public class STAFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public STAFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        yield return new STATestCase(
            _diagnosticMessageSink,
            discoveryOptions.MethodDisplayOrDefault(),
            discoveryOptions.MethodDisplayOptionsOrDefault(),
            testMethod);
    }
}

/// <summary>
/// Custom test case that runs on STA thread with proper WPF dispatcher
/// </summary>
public class STATestCase : XunitTestCase
{
    [Obsolete("Called by the de-serializer", error: true)]
    public STATestCase() { }

    public STATestCase(
        IMessageSink diagnosticMessageSink,
        TestMethodDisplay defaultMethodDisplay,
        TestMethodDisplayOptions defaultMethodDisplayOptions,
        ITestMethod testMethod,
        object[]? testMethodArguments = null)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, 
               testMethod, testMethodArguments)
    {
    }

    public override async Task<RunSummary> RunAsync(
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var tcs = new TaskCompletionSource<RunSummary>();
        var thread = new Thread(() =>
        {
            try
            {
                // Set up WPF synchronization context for proper dispatcher behavior
                var context = new System.Windows.Threading.DispatcherSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
                
                // Run the test
                var result = base.RunAsync(
                    diagnosticMessageSink,
                    messageBus,
                    constructorArguments,
                    aggregator,
                    cancellationTokenSource).GetAwaiter().GetResult();
                    
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        // CRITICAL: Set STA apartment state before starting
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        
        // Wait for test completion
        await Task.Run(() => thread.Join());

        return await tcs.Task;
    }
}
