using Microsoft.Extensions.DependencyInjection;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.CommandHandlers;
using PlaneCrazy.Infrastructure.EventStore;
using PlaneCrazy.Infrastructure.Http;
using PlaneCrazy.Infrastructure.Models;
using PlaneCrazy.Infrastructure.Projections;
using PlaneCrazy.Infrastructure.QueryServices;
using PlaneCrazy.Infrastructure.Repositories;
using PlaneCrazy.Infrastructure.Services;

namespace PlaneCrazy.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for configuring PlaneCrazy infrastructure services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all PlaneCrazy infrastructure services including repositories, event store, projections, and HTTP services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlaneCrazyInfrastructure(this IServiceCollection services)
    {
        // Add logging (if not already added by the host)
        services.AddLogging(builder => 
        {
            // Configuration can be overridden by the host application
        });

        // Register Event Store as Singleton (maintains event history state)
        services.AddSingleton<IEventStore, JsonFileEventStore>();

        // Register Repositories as Singleton (file-based repositories with thread-safe operations)
        services.AddSingleton<AircraftRepository>();
        services.AddSingleton<FavouriteRepository>();
        services.AddSingleton<CommentRepository>();
        services.AddSingleton<AirportRepository>();

        // Register Projections as Singleton (must be done before EventDispatcher)
        services.AddSingleton<IProjection, FavouriteProjection>();
        services.AddSingleton<IProjection, CommentProjection>();
        services.AddSingleton<IProjection, AircraftStateProjection>();
        
        // Register Projections with their concrete types for backward compatibility
        services.AddSingleton<FavouriteProjection>();
        services.AddSingleton<CommentProjection>();
        services.AddSingleton<AircraftStateProjection>();
        
        // Register EventDispatcher (will automatically get all IProjection implementations)
        services.AddSingleton<IEventDispatcher, Infrastructure.EventDispatcher.EventDispatcher>();

        // Register HTTP Client and related services
        services.AddHttpClient<IApiClient, ApiClient>();

        // Register Aircraft Data Service as Singleton
        services.AddSingleton<IAircraftDataService, AdsbFiAircraftService>();

        // Register Command Handlers as Transient (stateless handlers created per request)
        services.AddTransient<ICommandHandler<AddCommentCommand>, AddCommentCommandHandler>();
        services.AddTransient<ICommandHandler<EditCommentCommand>, EditCommentCommandHandler>();
        services.AddTransient<ICommandHandler<DeleteCommentCommand>, DeleteCommentCommandHandler>();
        services.AddTransient<ICommandHandler<FavouriteAircraftCommand>, FavouriteAircraftCommandHandler>();
        services.AddTransient<ICommandHandler<UnfavouriteAircraftCommand>, UnfavouriteAircraftCommandHandler>();
        services.AddTransient<ICommandHandler<FavouriteAircraftTypeCommand>, FavouriteAircraftTypeCommandHandler>();
        services.AddTransient<ICommandHandler<UnfavouriteAircraftTypeCommand>, UnfavouriteAircraftTypeCommandHandler>();
        services.AddTransient<ICommandHandler<FavouriteAirportCommand>, FavouriteAirportCommandHandler>();
        services.AddTransient<ICommandHandler<UnfavouriteAirportCommand>, UnfavouriteAirportCommandHandler>();

        // Register Query Services as Singleton (read-only services using projections)
        services.AddSingleton<IAircraftQueryService, AircraftQueryService>();
        services.AddSingleton<ICommentQueryService, CommentQueryService>();
        services.AddSingleton<IFavouriteQueryService, FavouriteQueryService>();

        // Register Poller Configuration as Singleton
        services.AddSingleton<PollerConfiguration>(new PollerConfiguration());

        // Register Background Services
        services.AddHostedService<BackgroundAdsBPoller>();

        return services;
    }
}
