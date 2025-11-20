using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Tests.Helpers;
using WpfEngine.ViewModels.Base;
using Xunit;

namespace WpfEngine.Tests.Core.ViewModels;

/// <summary>
/// Tests for ValidatableViewModel with async validation
/// </summary>
public class ValidatableViewModelTests
{
    private readonly Mock<ILogger> _loggerMock;
    
    public ValidatableViewModelTests()
    {
        _loggerMock = new Mock<ILogger>();
    }
    
    [Fact]
    public async Task ValidatePropertyAsync_WithRequiredAttribute_ValidatesCorrectly()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>());
        
        // Act - set empty required field
        vm.RequiredField = "";
        await vm.ForceValidatePropertyAsync(nameof(vm.RequiredField));
        
        // Assert
        vm.HasErrors.Should().BeTrue();
        var errors = vm.GetPropertyErrors(nameof(vm.RequiredField)).ToList();
        errors.Should().HaveCount(1);
        errors[0].Should().Contain("required");
        
        // Act - set valid value
        vm.RequiredField = "Valid Value";
        await vm.ForceValidatePropertyAsync(nameof(vm.RequiredField));
        
        // Assert
        vm.GetPropertyErrors(nameof(vm.RequiredField)).Should().BeEmpty();
    }
    
    [Fact]
    public async Task ValidatePropertyAsync_WithRangeAttribute_ValidatesCorrectly()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>());
        
        // Act - set out of range value
        vm.RangeValue = 150;
        await vm.ForceValidatePropertyAsync(nameof(vm.RangeValue));
        
        // Assert
        vm.HasErrors.Should().BeTrue();
        var errors = vm.GetPropertyErrors(nameof(vm.RangeValue)).ToList();
        errors.Should().HaveCountGreaterThan(0);
        
        // Act - set valid value
        vm.RangeValue = 50;
        await vm.ForceValidatePropertyAsync(nameof(vm.RangeValue));
        
        // Assert
        vm.GetPropertyErrors(nameof(vm.RangeValue)).Should().BeEmpty();
    }
    
    [Fact]
    public async Task ValidatePropertyAsync_WithCustomValidation_RunsAsyncValidation()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>());
        
        // Act - set value that will fail async validation
        vm.AsyncValidatedField = "invalid";
        await vm.ForceValidatePropertyAsync(nameof(vm.AsyncValidatedField));
        
        // Assert
        vm.HasErrors.Should().BeTrue();
        var errors = vm.GetPropertyErrors(nameof(vm.AsyncValidatedField)).ToList();
        errors.Should().Contain("Value 'invalid' is not allowed");
        
        // Act - set valid value
        vm.AsyncValidatedField = "valid";
        await vm.ForceValidatePropertyAsync(nameof(vm.AsyncValidatedField));
        
        // Assert
        vm.GetPropertyErrors(nameof(vm.AsyncValidatedField)).Should().BeEmpty();
    }
    
    [Fact]
    public async Task ValidateAsync_ValidatesAllProperties()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>())
        {
            RequiredField = "", // Invalid
            RangeValue = 150,   // Invalid
            AsyncValidatedField = "invalid" // Invalid
        };
        
        // Act
        var isValid = await vm.ValidateAsync();
        
        // Assert
        isValid.Should().BeFalse();
        vm.HasErrors.Should().BeTrue();
        vm.GetValidationErrors().Should().HaveCountLessThanOrEqualTo(3);
    }
    
    [Fact]
    public async Task ValidateAsync_WithObjectLevelValidation_ValidatesObject()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>())
        {
            RequiredField = "Valid",
            RangeValue = 50,
            AsyncValidatedField = "valid",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now // End before start - object level error
        };
        
        // Act
        var isValid = await vm.ValidateAsync();
        
        // Assert
        isValid.Should().BeFalse();
        vm.GetValidationErrors().Should().Contain("End date must be after start date");
    }
    
    [Fact]
    public async Task SetPropertyWithValidation_TriggersValidationAsync()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>());
        var propertyChangedEvents = new List<string>();
        vm.PropertyChanged += (s, e) => propertyChangedEvents.Add(e.PropertyName!);
        
        // Act
        vm.ValidatedOnChangeField = "";
        
        // Wait a bit for async validation
        await Task.Delay(100);
        
        // Assert
        propertyChangedEvents.Should().Contain(nameof(vm.ValidatedOnChangeField));
        vm.HasErrors.Should().BeFalse(); // Validation should have run
    }
    
    [Fact]
    public void ClearAllErrors_RemovesAllErrors()
    {
        // Arrange
        var vm = new TestValidatableViewModel(new MockLogger<TestValidatableViewModel>());
        vm.AddPropertyError(nameof(vm.RequiredField), "Error 1");
        vm.AddPropertyError(nameof(vm.RangeValue), "Error 2");
        vm.HasErrors.Should().BeTrue();
        
        // Act
        vm.TestClearAllErrors();
        
        // Assert
        vm.HasErrors.Should().BeFalse();
        vm.GetValidationErrors().Should().BeEmpty();
    }
    
    // ========== TEST VIEW MODEL ==========
    
    private class TestValidatableViewModel : ValidatableViewModel
    {
        private string _requiredField = "";
        private int _rangeValue;
        private string _asyncValidatedField = "";
        private string? _validatedOnChangeField;
        
        [Required(ErrorMessage = "This field is required")]
        public string RequiredField
        {
            get => _requiredField;
            set => SetProperty(ref _requiredField, value);
        }
        
        [Range(1, 100, ErrorMessage = "Value must be between 1 and 100")]
        public int RangeValue
        {
            get => _rangeValue;
            set => SetProperty(ref _rangeValue, value);
        }
        
        public string AsyncValidatedField
        {
            get => _asyncValidatedField;
            set => SetProperty(ref _asyncValidatedField, value);
        }
        
        public string? ValidatedOnChangeField
        {
            get => _validatedOnChangeField;
            set => SetPropertyWithValidation(ref _validatedOnChangeField, value, true);
        }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public TestValidatableViewModel(ILogger<TestValidatableViewModel> logger) : base(logger)
        {
        }
        
        protected override async Task<IEnumerable<string>> ValidatePropertyCustomAsync(string propertyName, object? value)
        {
            var errors = new List<string>();
            
            if (propertyName == nameof(AsyncValidatedField))
            {
                // Simulate async validation (e.g., checking against a service)
                await Task.Delay(10);
                
                if (value is string strValue && strValue == "invalid")
                {
                    errors.Add($"Value '{strValue}' is not allowed");
                }
            }
            
            return errors;
        }
        
        protected override Task<IEnumerable<string>> ValidateObjectCustomAsync()
        {
            var errors = new List<string>();
            
            if (EndDate < StartDate)
            {
                errors.Add("End date must be after start date");
            }
            
            return Task.FromResult<IEnumerable<string>>(errors);
        }
        
        // Expose protected method for testing
        public new void AddPropertyError(string propertyName, string error) => base.AddPropertyError(propertyName, error);
        public void TestClearAllErrors() => ClearAllErrors();
    }
}