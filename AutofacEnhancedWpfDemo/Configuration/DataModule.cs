using Autofac;
using AutofacEnhancedWpfDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Configuration;

/// <summary>
/// Autofac module for data-related registrations
/// Uses IDbContextFactory pattern for better isolation and thread-safety
/// </summary>
public class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register IDbContextFactory manually for Autofac
        builder.Register<IDbContextFactory<AppDbContext>>(c =>
        {
            var loggerFactory = c.Resolve<ILoggerFactory>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=DemoDb.db")
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .Options;

            return new PooledDbContextFactory<AppDbContext>(options);
        })
        .As<IDbContextFactory<AppDbContext>>()
        .SingleInstance();
    }
}
