using System;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Services;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

public record DemoProductDetailParams
{
    public int ProductId { get; init; }
}
