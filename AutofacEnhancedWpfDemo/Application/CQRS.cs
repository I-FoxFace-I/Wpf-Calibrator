using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.Application;

/// <summary>
/// Marker interface for commands (write operations)
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for queries (read operations)
/// </summary>
/// <typeparam name="TResult">Query result type</typeparam>
public interface IQuery<TResult>
{
}

/// <summary>
/// Handler for commands
/// </summary>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command);
}

/// <summary>
/// Handler for queries
/// </summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}

/// <summary>
/// Result wrapper for operations
/// </summary>
public class Result
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static Result Ok() => new() { Success = true };
    public static Result Fail(string error) => new() { Success = false, ErrorMessage = error };
}

/// <summary>
/// Result wrapper with data
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; init; }
    
    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public new static Result<T> Fail(string error) => new() { Success = false, ErrorMessage = error };
}
