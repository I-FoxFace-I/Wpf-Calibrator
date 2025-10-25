using System;
using System.Threading.Tasks;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

public record DemoCustomerDetailParams : ViewModelOptions
{
    public int CustomerId { get; init; }
}
