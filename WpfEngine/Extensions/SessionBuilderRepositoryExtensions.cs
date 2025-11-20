using WpfEngine.Services.Sessions;

namespace WpfEngine.Extensions;

/// <summary>
/// Extension methods for ISessionBuilder to add repository support
/// </summary>
public static class SessionBuilderRepositoryExtensions
{
    /// <summary>
    /// Register a generic repository for an entity type
    /// Requires that IRepository&lt;T&gt; is registered in central modules
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="builder">Session builder</param>
    /// <returns>Session builder with repository declared</returns>
    /// <remarks>
    /// This is a declaration, not a registration. The actual IRepository&lt;T&gt;
    /// should be registered centrally with InstancePerMatchingLifetimeScope.
    /// </remarks>
    public static ISessionBuilder WithRepository<TEntity>(this ISessionBuilder builder)
        where TEntity : class
    {
        // This is just a marker - actual registration should be in central modules
        // We could add runtime validation here if needed
        return builder;
    }
}

