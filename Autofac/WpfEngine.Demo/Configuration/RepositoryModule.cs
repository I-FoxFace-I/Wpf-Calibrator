using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Sessions;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Repositories;
using WpfEngine.Services.Autofac;

namespace WpfEngine.Demo.Configuration;



/// <summary>
/// Autofac module pro Repository + Unit of Work pattern
/// Alternativa k CQRS patternu v původním Demo projektu
/// </summary>
public class RepositoryModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ========== DATABASE CONTEXT ==========

        // ========== REPOSITORY PATTERN ==========
        // Register generic repository - per database session scope
        var databaseTag = ScopeTag.Database().ToAutofacTag();
        var workflowTag = ScopeTag.Workflow("order-workflow").ToAutofacTag();

        builder.RegisterGeneric(typeof(Repository<>))
            .As(typeof(IRepository<>))
            .InstancePerMatchingLifetimeScope(databaseTag);

        builder.RegisterType<OrderRepository>()
            .As<IOrderRepository>()
            .InstancePerMatchingLifetimeScope(databaseTag);

        // ========== UNIT OF WORK ==========
        // Unit of Work is also per database session scope
        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerMatchingLifetimeScope(databaseTag);
    }
}
