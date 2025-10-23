using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Products;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// Demonstrates Autofac scope hierarchy with shared state
/// Parent window opens multiple non-modal child windows
/// All children share the same service instances (scoped to parent)
/// </summary>
public partial class ScopeHierarchyDemoViewModel : BaseViewModel
{
    private readonly IWindowNavigator _navigator;
    private readonly IQueryHandler<GetAllProductsQuery, List<Product>> _getAllProductsHandler;

    [ObservableProperty]
    private ObservableCollection<string> _logMessages = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    public ScopeHierarchyDemoViewModel(
        IWindowNavigator navigator,
        IQueryHandler<GetAllProductsQuery, List<Product>> getAllProductsHandler,
        ILogger<ScopeHierarchyDemoViewModel> logger) : base(logger)
    {
        _navigator = navigator;
        _getAllProductsHandler = getAllProductsHandler;

        AddLog("🔗 Scope Hierarchy Demo initialized");
        AddLog("📍 This window creates a parent scope");
    }

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            AddLog("📦 Loading products...");
            var products = await _getAllProductsHandler.HandleAsync(new GetAllProductsQuery());

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            AddLog($"✅ Loaded {products.Count} products");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error loading products: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenChild1()
    {
        AddLog("🟥 Opening Child Window 1 (Red)");
        _navigator.ShowWindow<ChildDemoViewModel>(
            new ChildDemoOptions { ChildNumber = 1, Color = "#EF4444" }
        );
    }

    [RelayCommand]
    private void OpenChild2()
    {
        AddLog("🟦 Opening Child Window 2 (Blue)");
        _navigator.ShowWindow<ChildDemoViewModel>(
            new ChildDemoOptions { ChildNumber = 2, Color = "#3B82F6" }
        );
    }

    [RelayCommand]
    private void OpenChild3()
    {
        AddLog("🟩 Opening Child Window 3 (Green)");
        _navigator.ShowWindow<ChildDemoViewModel>(
            new ChildDemoOptions { ChildNumber = 3, Color = "#10B981" }
        );
    }

    [RelayCommand]
    private void ClearLog()
    {
        LogMessages.Clear();
        AddLog("🧹 Log cleared");
    }

    public void UpdateProduct(int productId)
    {
        var product = Products.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            AddLog($"🔄 Product {product.Name} updated from child window");
        }
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogMessages.Add($"[{timestamp}] {message}");
        Logger.LogInformation(message);
    }
}