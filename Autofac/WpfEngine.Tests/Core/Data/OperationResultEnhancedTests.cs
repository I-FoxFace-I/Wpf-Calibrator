using FluentAssertions;
using WpfEngine.Data.Evaluation;
using Xunit;

namespace WpfEngine.Tests.Core.Data;

/// <summary>
/// Tests for OperationResultEnhanced - pattern matching and LINQ-style operations
/// </summary>
public class OperationResultEnhancedTests
{
    // ========== FACTORY METHODS ==========

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Arrange & Act
        var result = OperationResultEnhanced<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.ErrorMessage.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessage_CreatesFailureResult()
    {
        // Arrange & Act
        var result = OperationResultEnhanced<int>.Failure("Something went wrong");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(0);
        result.ErrorMessage.Should().Be("Something went wrong");
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void Failure_WithException_CreatesFailureResult()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = OperationResultEnhanced<int>.Failure(exception);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Test exception");
        result.Exception.Should().Be(exception);
    }

    // ========== PATTERN MATCHING ==========

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessPath()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: (msg, ex) => $"Failure: {msg}");

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailurePath()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error occurred");

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: (msg, ex) => $"Failure: {msg}");

        // Assert
        output.Should().Be("Failure: Error occurred");
    }

    [Fact]
    public async Task MatchAsync_OnSuccess_ExecutesSuccessPath()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var output = await result.MatchAsync(
            onSuccess: async value =>
            {
                await Task.Delay(10);
                return $"Success: {value}";
            },
            onFailure: async (msg, ex) =>
            {
                await Task.Delay(10);
                return $"Failure: {msg}";
            });

        // Assert
        output.Should().Be("Success: 42");
    }

    [Fact]
    public void Match_VoidAction_ExecutesCorrectPath()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);
        var executed = "";

        // Act
        result.Match(
            onSuccess: value => executed = $"Success: {value}",
            onFailure: (msg, ex) => executed = $"Failure: {msg}");

        // Assert
        executed.Should().Be("Success: 42");
    }

    // ========== LINQ-STYLE OPERATIONS ==========

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var mapped = result.Map(value => value.ToString());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public void Map_OnFailure_PreservesFailure()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error");

        // Act
        var mapped = result.Map(value => value.ToString());

        // Assert
        mapped.IsSuccess.Should().BeFalse();
        mapped.ErrorMessage.Should().Be("Error");
    }

    [Fact]
    public async Task MapAsync_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var mapped = await result.MapAsync(async value =>
        {
            await Task.Delay(10);
            return value.ToString();
        });

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public void Bind_OnSuccess_ChainsOperation()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var bound = result.Bind(value =>
            value > 40
                ? OperationResultEnhanced<string>.Success("Large number")
                : OperationResultEnhanced<string>.Failure("Small number"));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("Large number");
    }

    [Fact]
    public void Bind_OnFailure_PreservesFailure()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Initial error");

        // Act
        var bound = result.Bind(value =>
            OperationResultEnhanced<string>.Success(value.ToString()));

        // Assert
        bound.IsSuccess.Should().BeFalse();
        bound.ErrorMessage.Should().Be("Initial error");
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ChainsAsyncOperation()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var bound = await result.BindAsync(async value =>
        {
            await Task.Delay(10);
            return OperationResultEnhanced<string>.Success(value.ToString());
        });

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("42");
    }

    // ========== SIDE EFFECTS ==========

    [Fact]
    public void OnSuccess_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);
        var executed = false;

        // Act
        var returnedResult = result.OnSuccess(value => executed = true);

        // Assert
        executed.Should().BeTrue();
        returnedResult.Should().Be(result); // Returns same instance
    }

    [Fact]
    public void OnSuccess_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error");
        var executed = false;

        // Act
        result.OnSuccess(value => executed = true);

        // Assert
        executed.Should().BeFalse();
    }

    [Fact]
    public void OnFailure_OnFailure_ExecutesAction()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error");
        var capturedMessage = "";

        // Act
        var returnedResult = result.OnFailure((msg, ex) => capturedMessage = msg!);

        // Assert
        capturedMessage.Should().Be("Error");
        returnedResult.Should().Be(result);
    }

    [Fact]
    public void OnFailure_OnSuccess_DoesNotExecuteAction()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);
        var executed = false;

        // Act
        result.OnFailure((msg, ex) => executed = true);

        // Assert
        executed.Should().BeFalse();
    }

    [Fact]
    public async Task OnSuccessAsync_ExecutesAsyncAction()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);
        var executed = false;

        // Act
        await result.OnSuccessAsync(async value =>
        {
            await Task.Delay(10);
            executed = true;
        });

        // Assert
        executed.Should().BeTrue();
    }

    // ========== COMBINATION ==========

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        // Arrange
        var result1 = OperationResultEnhanced<int>.Success(1);
        var result2 = OperationResultEnhanced<int>.Success(2);
        var result3 = OperationResultEnhanced<int>.Success(3);

        // Act
        var combined = OperationResultEnhanced<int>.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Combine_OneFailure_ReturnsFailure()
    {
        // Arrange
        var result1 = OperationResultEnhanced<int>.Success(1);
        var result2 = OperationResultEnhanced<int>.Failure("Error in operation 2");
        var result3 = OperationResultEnhanced<int>.Success(3);

        // Act
        var combined = OperationResultEnhanced<int>.Combine(result1, result2, result3);

        // Assert
        combined.IsSuccess.Should().BeFalse();
        combined.ErrorMessage.Should().Contain("Operation 2 failed");
    }

    [Fact]
    public void FirstSuccess_ReturnsFirstSuccessful()
    {
        // Arrange
        var result1 = OperationResultEnhanced<int>.Failure("Error 1");
        var result2 = OperationResultEnhanced<int>.Success(42);
        var result3 = OperationResultEnhanced<int>.Success(100);

        // Act
        var first = OperationResultEnhanced<int>.FirstSuccess(result1, result2, result3);

        // Assert
        first.IsSuccess.Should().BeTrue();
        first.Value.Should().Be(42);
    }

    [Fact]
    public void FirstSuccess_AllFailures_ReturnsLastFailure()
    {
        // Arrange
        var result1 = OperationResultEnhanced<int>.Failure("Error 1");
        var result2 = OperationResultEnhanced<int>.Failure("Error 2");
        var result3 = OperationResultEnhanced<int>.Failure("Error 3");

        // Act
        var first = OperationResultEnhanced<int>.FirstSuccess(result1, result2, result3);

        // Assert
        first.IsSuccess.Should().BeFalse();
        first.ErrorMessage.Should().Be("Error 3");
    }

    // ========== CHAINING OPERATIONS ==========

    [Fact]
    public void ChainedOperations_FluentStyle_WorksCorrectly()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(10);
        var sideEffectValue = 0;

        // Act
        var final = result
            .OnSuccess(value => sideEffectValue = value)
            .Map(value => value * 2)
            .Bind(value => value > 15
                ? OperationResultEnhanced<string>.Success($"Large: {value}")
                : OperationResultEnhanced<string>.Failure("Too small"))
            .OnSuccess(str => sideEffectValue = 999);

        // Assert
        final.IsSuccess.Should().BeTrue();
        final.Value.Should().Be("Large: 20");
        sideEffectValue.Should().Be(999); // Last OnSuccess executed
    }

    [Fact]
    public void ChainedOperations_WithFailure_StopsAtFailure()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(5);
        var executionCount = 0;

        // Act
        var final = result
            .OnSuccess(value => executionCount++)
            .Map(value => value * 2) // 10
            .Bind(value => value > 15
                ? OperationResultEnhanced<string>.Success($"Large: {value}")
                : OperationResultEnhanced<string>.Failure("Too small")) // Fails here
            .OnSuccess(str => executionCount++); // Should not execute

        // Assert
        final.IsSuccess.Should().BeFalse();
        final.ErrorMessage.Should().Be("Too small");
        executionCount.Should().Be(1); // Only first OnSuccess executed
    }

    // ========== CONVERSION ==========

    [Fact]
    public void ToOperationResult_Success_ConvertsCorrectly()
    {
        // Arrange
        var enhanced = OperationResultEnhanced<int>.Success(42);

        // Act
        var standard = enhanced.ToOperationResult();

        // Assert
        standard.IsSuccess.Should().BeTrue();
        standard.Value.Should().Be(42);
    }

    [Fact]
    public void FromOperationResult_Success_ConvertsCorrectly()
    {
        // Arrange
        var standard = OperationResult<int>.Success(42);

        // Act
        var enhanced = OperationResultEnhanced<int>.FromOperationResult(standard);

        // Assert
        enhanced.IsSuccess.Should().BeTrue();
        enhanced.Value.Should().Be(42);
    }

    // ========== IMPLICIT CONVERSIONS ==========

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        // Act
        OperationResultEnhanced<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_ToBool_ReturnsIsSuccess()
    {
        // Arrange
        var success = OperationResultEnhanced<int>.Success(42);
        var failure = OperationResultEnhanced<int>.Failure("Error");

        // Act & Assert
        if (success)
        {
            true.Should().BeTrue(); // Success path
        }
        else
        {
            false.Should().BeTrue(); // Should not reach here
        }

        if (failure)
        {
            false.Should().BeTrue(); // Should not reach here
        }
        else
        {
            true.Should().BeTrue(); // Failure path
        }
    }

    // ========== TRADITIONAL METHODS ==========

    [Fact]
    public void GetValueOrThrow_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var value = result.GetValueOrThrow();

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrThrow_OnFailure_ThrowsException()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error occurred");

        // Act
        Action act = () => result.GetValueOrThrow();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Error occurred");
    }

    [Fact]
    public void TryGetValue_OnSuccess_ReturnsTrueAndValue()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var success = result.TryGetValue(out var value);

        // Assert
        success.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetValue_OnFailure_ReturnsFalse()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error");

        // Act
        var success = result.TryGetValue(out var value);

        // Assert
        success.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Success(42);

        // Act
        var value = result.GetValueOrDefault(999);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefault()
    {
        // Arrange
        var result = OperationResultEnhanced<int>.Failure("Error");

        // Act
        var value = result.GetValueOrDefault(999);

        // Assert
        value.Should().Be(999);
    }

    // ========== NON-GENERIC TESTS ==========

    [Fact]
    public void NonGeneric_Success_CreatesSuccessfulResult()
    {
        // Act
        var result = OperationResultEnhanced.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void NonGeneric_Match_WorksCorrectly()
    {
        // Arrange
        var success = OperationResultEnhanced.Success();
        var failure = OperationResultEnhanced.Failure("Error");

        // Act
        var successOutput = success.Match(
            onSuccess: () => "Success",
            onFailure: (msg, ex) => $"Failure: {msg}");

        var failureOutput = failure.Match(
            onSuccess: () => "Success",
            onFailure: (msg, ex) => $"Failure: {msg}");

        // Assert
        successOutput.Should().Be("Success");
        failureOutput.Should().Be("Failure: Error");
    }

    [Fact]
    public void NonGeneric_Combine_WorksCorrectly()
    {
        // Arrange
        var result1 = OperationResultEnhanced.Success();
        var result2 = OperationResultEnhanced.Success();
        var result3 = OperationResultEnhanced.Failure("Error");

        // Act
        var allSuccess = OperationResultEnhanced.Combine(result1, result2);
        var hasFailure = OperationResultEnhanced.Combine(result1, result2, result3);

        // Assert
        allSuccess.IsSuccess.Should().BeTrue();
        hasFailure.IsSuccess.Should().BeFalse();
    }
}


