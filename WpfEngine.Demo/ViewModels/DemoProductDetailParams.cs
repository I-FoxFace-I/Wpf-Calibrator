using System;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

public record DemoProductDetailParams : ViewModelOptions
{
    public int ProductId { get; init; }
}
