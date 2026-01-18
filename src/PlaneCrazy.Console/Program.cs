using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure;
using PlaneCrazy.Infrastructure.DependencyInjection;
using PlaneCrazy.Infrastructure.Projections;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Console;

class Program
{
    private static IHost _host = null!;
    private static IServiceProvider _serviceProvider = null!;
    private static IEventStore _eventStore = null!;
    private static IEventDispatcher _dispatcher = null!;
    private static AircraftRepository _aircraftRepo = null!;
    private static FavouriteRepository _favouriteRepo = null!;
    private static CommentRepository _commentRepo = null!;
    private static IAircraftDataService _aircraftService = null!;
    private static FavouriteProjection _favouriteProjection = null!;
    private static CommentProjection _commentProjection = null!;
    private static IAircraftQueryService _aircraftQueryService = null!;
    private static ICommentQueryService _commentQueryService = null!;
    private static IFavouriteQueryService _favouriteQueryService = null!;

    static async Task Main(string[] args)
    {
        // Build the host with background services
        var builder = Host.CreateApplicationBuilder(args);
        
        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        
        // Add all infrastructure services (repositories, event store, projections, HTTP services, background services)
        builder.Services.AddPlaneCrazyInfrastructure();
        
        _host = builder.Build();
        _serviceProvider = _host.Services;
        
        // Initialize app and projections
        InitializeApp();
        await RebuildProjectionsAsync();

        // Start background services (non-blocking)
        _ = _host.RunAsync();

        System.Console.Clear();
        System.Console.WriteLine("╔═══════════════════════════════════════╗");
        System.Console.WriteLine("║         PlaneCrazy v1.0               ║");
        System.Console.WriteLine("║   ADS-B Aircraft Tracking System      ║");
        System.Console.WriteLine("╚═══════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("Background polling is active!");
        System.Console.WriteLine();

        bool running = true;
        while (running)
        {
            DisplayMainMenu();
            var choice = System.Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await FetchAndDisplayAircraftAsync();
                    break;
                case "2":
                    await ManageFavouritesAsync();
                    break;
                case "3":
                    await ManageCommentsAsync();
                    break;
                case "4":
                    await ViewEventsAsync();
                    break;
                case "5":
                    running = false;
                    break;
                default:
                    System.Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }

            if (running)
            {
                System.Console.WriteLine("\nPress any key to continue...");
                System.Console.ReadKey();
            }
        }

        System.Console.WriteLine("\nStopping background services...");
        await _host.StopAsync();
        System.Console.WriteLine("Thank you for using PlaneCrazy!");
    }

    private static void InitializeApp()
    {
        System.Console.WriteLine($"Data directory: {PlaneCrazyPaths.BasePath}");
        System.Console.WriteLine();

        // Resolve singleton services from DI container
        _eventStore = _serviceProvider.GetRequiredService<IEventStore>();
        _dispatcher = _serviceProvider.GetRequiredService<IEventDispatcher>();
        _aircraftRepo = _serviceProvider.GetRequiredService<AircraftRepository>();
        _favouriteRepo = _serviceProvider.GetRequiredService<FavouriteRepository>();
        _commentRepo = _serviceProvider.GetRequiredService<CommentRepository>();
        _aircraftService = _serviceProvider.GetRequiredService<IAircraftDataService>();
        _favouriteProjection = _serviceProvider.GetRequiredService<FavouriteProjection>();
        _commentProjection = _serviceProvider.GetRequiredService<CommentProjection>();
        _aircraftQueryService = _serviceProvider.GetRequiredService<IAircraftQueryService>();
        _commentQueryService = _serviceProvider.GetRequiredService<ICommentQueryService>();
        _favouriteQueryService = _serviceProvider.GetRequiredService<IFavouriteQueryService>();
        
        // Show projection statistics
        var stats = _dispatcher.GetProjectionStatistics();
        System.Console.WriteLine($"Registered projections: {stats.TotalProjections}");
        foreach (var name in stats.ProjectionNames)
        {
            System.Console.WriteLine($"  - {name}");
        }
        System.Console.WriteLine();
    }

    private static async Task RebuildProjectionsAsync()
    {
        System.Console.WriteLine("Rebuilding projections from event store...");
        await _favouriteProjection.RebuildAsync();
        await _commentProjection.RebuildAsync();
        System.Console.WriteLine("Projections rebuilt successfully.");
        System.Console.WriteLine();
    }

    private static void DisplayMainMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("╔═══════════════════════════════════════╗");
        System.Console.WriteLine("║            Main Menu                  ║");
        System.Console.WriteLine("╚═══════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("1. Fetch and View Aircraft");
        System.Console.WriteLine("2. Manage Favourites");
        System.Console.WriteLine("3. Manage Comments");
        System.Console.WriteLine("4. View Event History");
        System.Console.WriteLine("5. Exit");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");
    }

    private static async Task FetchAndDisplayAircraftAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("╔═══════════════════════════════════════╗");
        System.Console.WriteLine("║        Fetching Aircraft Data         ║");
        System.Console.WriteLine("╚═══════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("Fetching aircraft from adsb.fi...");

        var aircraft = await _aircraftService.FetchAircraftAsync();
        var aircraftList = aircraft.ToList();

        if (!aircraftList.Any())
        {
            System.Console.WriteLine("No aircraft data available.");
            return;
        }

        // Store aircraft in repository
        foreach (var plane in aircraftList.Take(50)) // Limit to first 50
        {
            await _aircraftRepo.SaveAsync(plane);
        }

        System.Console.WriteLine($"\nFound {aircraftList.Count} aircraft. Displaying first 20:");
        System.Console.WriteLine();
        System.Console.WriteLine("┌────────────┬──────────────┬──────────┬─────────────┬──────────────┐");
        System.Console.WriteLine("│ ICAO24     │ Registration │ Type     │ Callsign    │ Altitude (ft)│");
        System.Console.WriteLine("├────────────┼──────────────┼──────────┼─────────────┼──────────────┤");

        foreach (var plane in aircraftList.Take(20))
        {
            System.Console.WriteLine($"│ {plane.Icao24,-10} │ {plane.Registration ?? "N/A",-12} │ {plane.TypeCode ?? "N/A",-8} │ {plane.Callsign ?? "N/A",-11} │ {(plane.Altitude?.ToString("F0") ?? "N/A"),-12} │");
        }

        System.Console.WriteLine("└────────────┴──────────────┴──────────┴─────────────┴──────────────┘");

        System.Console.WriteLine("\nOptions:");
        System.Console.WriteLine("F <ICAO24> - Favourite an aircraft");
        System.Console.WriteLine("C <ICAO24> - Add comment to an aircraft");
        System.Console.WriteLine("Enter - Return to main menu");
        System.Console.Write("\nYour choice: ");

        var input = System.Console.ReadLine()?.Trim() ?? "";
        if (input.StartsWith("F ", StringComparison.OrdinalIgnoreCase))
        {
            var icao = input.Substring(2).Trim().ToUpper();
            var plane = aircraftList.FirstOrDefault(a => a.Icao24 == icao);
            if (plane != null)
            {
                await FavouriteAircraftAsync(plane);
            }
            else
            {
                System.Console.WriteLine($"Aircraft {icao} not found in current list.");
            }
        }
        else if (input.StartsWith("C ", StringComparison.OrdinalIgnoreCase))
        {
            var icao = input.Substring(2).Trim().ToUpper();
            await AddCommentAsync("Aircraft", icao);
        }
    }

    private static async Task ManageFavouritesAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("╔═══════════════════════════════════════╗");
        System.Console.WriteLine("║       Manage Favourites               ║");
        System.Console.WriteLine("╚═══════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("1. View Favourite Aircraft");
        System.Console.WriteLine("2. View Favourite Types");
        System.Console.WriteLine("3. View Favourite Airports");
        System.Console.WriteLine("4. Favourite a Type");
        System.Console.WriteLine("5. Favourite an Airport");
        System.Console.WriteLine("6. Unfavourite");
        System.Console.WriteLine("7. Back to Main Menu");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");

        var choice = System.Console.ReadLine();

        switch (choice)
        {
            case "1":
                await ViewFavouritesByTypeAsync("Aircraft");
                break;
            case "2":
                await ViewFavouritesByTypeAsync("Type");
                break;
            case "3":
                await ViewFavouritesByTypeAsync("Airport");
                break;
            case "4":
                await FavouriteTypeAsync();
                break;
            case "5":
                await FavouriteAirportAsync();
                break;
            case "6":
                await UnfavouriteAsync();
                break;
        }
    }

    private static async Task ViewFavouritesByTypeAsync(string entityType)
    {
        System.Console.Clear();
        System.Console.WriteLine($"╔═══════════════════════════════════════╗");
        System.Console.WriteLine($"║     Favourite {entityType,-19}    ║");
        System.Console.WriteLine($"╚═══════════════════════════════════════╝");
        System.Console.WriteLine();

        var favourites = await _favouriteQueryService.GetFavouritesByTypeAsync(entityType);
        var favList = favourites.ToList();

        if (!favList.Any())
        {
            System.Console.WriteLine($"No favourite {entityType.ToLower()}s found.");
            return;
        }

        System.Console.WriteLine($"Found {favList.Count} favourite {entityType.ToLower()}(s):");
        System.Console.WriteLine();

        foreach (var fav in favList)
        {
            System.Console.WriteLine($"• {fav.EntityId}");
            System.Console.WriteLine($"  Added: {fav.FavouritedAt:yyyy-MM-dd HH:mm:ss}");
            foreach (var meta in fav.Metadata.Where(m => !string.IsNullOrEmpty(m.Value)))
            {
                System.Console.WriteLine($"  {meta.Key}: {meta.Value}");
            }
            
            // Show comments
            if (fav.CommentCount > 0)
            {
                var comments = await _commentQueryService.GetActiveCommentsForEntityAsync(entityType, fav.EntityId);
                var commentList = comments.ToList();
                System.Console.WriteLine($"  Comments ({commentList.Count}):");
                foreach (var comment in commentList.Take(3))
                {
                    var preview = comment.Text.Length > 50 
                        ? comment.Text.Substring(0, 50) + "..." 
                        : comment.Text;
                    System.Console.WriteLine($"    - {preview}");
                }
            }
            System.Console.WriteLine();
        }
    }

    private static async Task FavouriteAircraftAsync(Domain.Entities.Aircraft aircraft)
    {
        var @event = new AircraftFavourited
        {
            Icao24 = aircraft.Icao24,
            Registration = aircraft.Registration,
            TypeCode = aircraft.TypeCode
        };

        var result = await _dispatcher.DispatchAsync(@event);
        
        if (result.Success)
        {
            System.Console.WriteLine($"\n✓ Aircraft {aircraft.Icao24} added to favourites!");
            System.Console.WriteLine($"  Updated {result.ProjectionsUpdated} projections in {result.TotalTimeMs}ms");
        }
        else
        {
            System.Console.WriteLine($"\n✗ Failed to favourite aircraft: {result.Error}");
        }
    }

    private static async Task FavouriteTypeAsync()
    {
        System.Console.Write("\nEnter aircraft type code (e.g., B738, A320): ");
        var typeCode = System.Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrEmpty(typeCode))
        {
            System.Console.WriteLine("Invalid type code.");
            return;
        }

        System.Console.Write("Enter type name (optional): ");
        var typeName = System.Console.ReadLine()?.Trim();

        var @event = new TypeFavourited
        {
            TypeCode = typeCode,
            TypeName = typeName
        };

        var result = await _dispatcher.DispatchAsync(@event);
        
        if (result.Success)
        {
            System.Console.WriteLine($"\n✓ Type {typeCode} added to favourites!");
            System.Console.WriteLine($"  Updated {result.ProjectionsUpdated} projections in {result.TotalTimeMs}ms");
        }
        else
        {
            System.Console.WriteLine($"\n✗ Failed to favourite type: {result.Error}");
        }
    }

    private static async Task FavouriteAirportAsync()
    {
        System.Console.Write("\nEnter airport ICAO code (e.g., EGLL, KJFK): ");
        var icaoCode = System.Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrEmpty(icaoCode))
        {
            System.Console.WriteLine("Invalid ICAO code.");
            return;
        }

        System.Console.Write("Enter airport name (optional): ");
        var name = System.Console.ReadLine()?.Trim();

        var @event = new AirportFavourited
        {
            IcaoCode = icaoCode,
            Name = name
        };

        var result = await _dispatcher.DispatchAsync(@event);
        
        if (result.Success)
        {
            System.Console.WriteLine($"\n✓ Airport {icaoCode} added to favourites!");
            System.Console.WriteLine($"  Updated {result.ProjectionsUpdated} projections in {result.TotalTimeMs}ms");
        }
        else
        {
            System.Console.WriteLine($"\n✗ Failed to favourite airport: {result.Error}");
        }
    }

    private static async Task UnfavouriteAsync()
    {
        System.Console.WriteLine("\nUnfavourite:");
        System.Console.WriteLine("1. Aircraft");
        System.Console.WriteLine("2. Type");
        System.Console.WriteLine("3. Airport");
        System.Console.Write("Choice: ");

        var choice = System.Console.ReadLine();
        
        System.Console.Write("\nEnter ID to unfavourite: ");
        var id = System.Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrEmpty(id))
        {
            System.Console.WriteLine("Invalid ID.");
            return;
        }

        DomainEvent? @event = choice switch
        {
            "1" => new AircraftUnfavourited { Icao24 = id },
            "2" => new TypeUnfavourited { TypeCode = id },
            "3" => new AirportUnfavourited { IcaoCode = id },
            _ => null
        };

        if (@event != null)
        {
            var result = await _dispatcher.DispatchAsync(@event);
            
            if (result.Success)
            {
                System.Console.WriteLine($"\n✓ Successfully unfavourited {id}!");
                System.Console.WriteLine($"  Updated {result.ProjectionsUpdated} projections in {result.TotalTimeMs}ms");
            }
            else
            {
                System.Console.WriteLine($"\n✗ Failed to unfavourite: {result.Error}");
            }
        }
    }

    private static async Task ManageCommentsAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("╔═══════════════════════════════════════╗");
        System.Console.WriteLine("║        Manage Comments                ║");
        System.Console.WriteLine("╚═══════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("1. View Comments for an Entity");
        System.Console.WriteLine("2. Add Comment");
        System.Console.WriteLine("3. Back to Main Menu");
        System.Console.WriteLine();
        System.Console.Write("Enter your choice: ");

        var choice = System.Console.ReadLine();

        switch (choice)
        {
            case "1":
                await ViewCommentsAsync();
                break;
            case "2":
                System.Console.Write("\nEntity Type (Aircraft/Type/Airport): ");
                var entityType = System.Console.ReadLine()?.Trim();
                System.Console.Write("Entity ID: ");
                var entityId = System.Console.ReadLine()?.Trim().ToUpper();
                if (!string.IsNullOrEmpty(entityType) && !string.IsNullOrEmpty(entityId))
                {
                    await AddCommentAsync(entityType, entityId);
                }
                break;
        }
    }

    private static async Task ViewCommentsAsync()
    {
        System.Console.Write("\nEntity Type (Aircraft/Type/Airport): ");
        var entityType = System.Console.ReadLine()?.Trim();
        System.Console.Write("Entity ID: ");
        var entityId = System.Console.ReadLine()?.Trim().ToUpper();

        if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId))
        {
            System.Console.WriteLine("Invalid input.");
            return;
        }

        // Use GetActiveCommentsForEntityAsync to exclude deleted comments
        var comments = await _commentQueryService.GetActiveCommentsForEntityAsync(entityType, entityId);
        var commentList = comments.ToList();

        System.Console.WriteLine($"\nComments for {entityType} {entityId}:");
        System.Console.WriteLine();

        if (!commentList.Any())
        {
            System.Console.WriteLine("No comments found.");
            return;
        }

        foreach (var comment in commentList)
        {
            var editedMarker = comment.UpdatedAt.HasValue ? " (edited)" : "";
            System.Console.WriteLine($"┌─ {comment.CreatedAt:yyyy-MM-dd HH:mm:ss}{editedMarker} ─────");
            if (!string.IsNullOrEmpty(comment.CreatedBy))
            {
                System.Console.WriteLine($"│ By: {comment.CreatedBy}");
            }
            System.Console.WriteLine($"│ {comment.Text}");
            if (comment.UpdatedAt.HasValue)
            {
                System.Console.WriteLine($"│ Last edited: {comment.UpdatedAt:yyyy-MM-dd HH:mm:ss} by {comment.UpdatedBy}");
            }
            System.Console.WriteLine($"└────────────────────────────");
            System.Console.WriteLine();
        }
    }

    private static async Task AddCommentAsync(string entityType, string entityId)
    {
        System.Console.Write("\nEnter your comment: ");
        var commentText = System.Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(commentText))
        {
            System.Console.WriteLine("Comment cannot be empty.");
            return;
        }

        var @event = new CommentAdded
        {
            EntityType = entityType,
            EntityId = entityId,
            Text = commentText,
            User = "DefaultUser" // TODO: Get from actual user context
        };

        var result = await _dispatcher.DispatchAsync(@event);
        
        if (result.Success)
        {
            System.Console.WriteLine("\n✓ Comment added successfully!");
            System.Console.WriteLine($"  Updated {result.ProjectionsUpdated} projections in {result.TotalTimeMs}ms");
        }
        else
        {
            System.Console.WriteLine($"\n✗ Failed to add comment: {result.Error}");
        }
    }

    private static async Task ViewEventsAsync()
    {
        System.Console.Clear();
        System.Console.WriteLine("╔═══════════════════════════════════════╗");
        System.Console.WriteLine("║         Event History                 ║");
        System.Console.WriteLine("╚═══════════════════════════════════════╝");
        System.Console.WriteLine();

        var events = await _eventStore.GetAllAsync();
        var eventList = events.ToList();

        if (!eventList.Any())
        {
            System.Console.WriteLine("No events found in the event store.");
            return;
        }

        System.Console.WriteLine($"Total events: {eventList.Count}");
        System.Console.WriteLine();

        foreach (var @event in eventList.TakeLast(20))
        {
            System.Console.WriteLine($"[{@event.OccurredAt:yyyy-MM-dd HH:mm:ss}] {@event.EventType}");
            System.Console.WriteLine($"  ID: {@event.Id}");
            
            switch (@event)
            {
                case AircraftFavourited af:
                    System.Console.WriteLine($"  Aircraft: {af.Icao24} ({af.Registration})");
                    break;
                case TypeFavourited tf:
                    System.Console.WriteLine($"  Type: {tf.TypeCode}");
                    break;
                case AirportFavourited apf:
                    System.Console.WriteLine($"  Airport: {apf.IcaoCode}");
                    break;
                case CommentAdded ca:
                    System.Console.WriteLine($"  Entity: {ca.EntityType}/{ca.EntityId}");
                    var commentPreview = ca.Text.Length > 50 
                        ? ca.Text.Substring(0, 50) + "..." 
                        : ca.Text;
                    System.Console.WriteLine($"  Comment: {commentPreview}");
                    break;
            }
            
            System.Console.WriteLine();
        }
    }
}
