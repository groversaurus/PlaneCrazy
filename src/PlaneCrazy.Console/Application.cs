using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Services;
using Spectre.Console;

namespace PlaneCrazy.Console;

/// <summary>
/// Main console application class
/// </summary>
public class Application
{
    private readonly IAircraftDataService _aircraftDataService;

    public Application(IAircraftDataService aircraftDataService)
    {
        _aircraftDataService = aircraftDataService;
    }

    public async Task RunAsync()
    {
        AnsiConsole.Write(
            new FigletText("PlaneCrazy")
                .Centered()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[blue]ADS-B Aircraft Data Tracker[/]");
        AnsiConsole.WriteLine();

        // Add sample data
        await AddSampleDataAsync();

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[blue]What would you like to do?[/]")
                    .AddChoices(new[] {
                        "View All Aircraft",
                        "Search Aircraft",
                        "Add Sample Aircraft",
                        "Exit"
                    }));

            switch (choice)
            {
                case "View All Aircraft":
                    await ViewAllAircraftAsync();
                    break;
                case "Search Aircraft":
                    await SearchAircraftAsync();
                    break;
                case "Add Sample Aircraft":
                    await AddSampleAircraftAsync();
                    break;
                case "Exit":
                    AnsiConsole.MarkupLine("[green]Goodbye![/]");
                    return;
            }

            AnsiConsole.WriteLine();
        }
    }

    private async Task AddSampleDataAsync()
    {
        var sampleAircraft = new[]
        {
            new AircraftData
            {
                Icao24 = "A12345",
                Callsign = "AAL123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Altitude = 35000,
                GroundSpeed = 450,
                Heading = 90,
                VerticalRate = 0
            },
            new AircraftData
            {
                Icao24 = "B67890",
                Callsign = "UAL456",
                Latitude = 34.0522,
                Longitude = -118.2437,
                Altitude = 28000,
                GroundSpeed = 420,
                Heading = 270,
                VerticalRate = -500
            }
        };

        foreach (var aircraft in sampleAircraft)
        {
            await _aircraftDataService.AddOrUpdateAircraftAsync(aircraft);
        }
    }

    private async Task ViewAllAircraftAsync()
    {
        var aircraft = await _aircraftDataService.GetAllAircraftAsync();
        var aircraftList = aircraft.ToList();

        if (!aircraftList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No aircraft currently tracked.[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("ICAO24");
        table.AddColumn("Callsign");
        table.AddColumn("Latitude");
        table.AddColumn("Longitude");
        table.AddColumn("Altitude (ft)");
        table.AddColumn("Speed (kts)");
        table.AddColumn("Heading");
        table.AddColumn("Last Seen");

        foreach (var ac in aircraftList)
        {
            table.AddRow(
                ac.Icao24,
                ac.Callsign ?? "N/A",
                ac.Latitude?.ToString("F4") ?? "N/A",
                ac.Longitude?.ToString("F4") ?? "N/A",
                ac.Altitude?.ToString() ?? "N/A",
                ac.GroundSpeed?.ToString("F1") ?? "N/A",
                ac.Heading?.ToString("F0") ?? "N/A",
                ac.LastSeen.ToString("HH:mm:ss")
            );
        }

        AnsiConsole.Write(table);
    }

    private async Task SearchAircraftAsync()
    {
        var icao = AnsiConsole.Ask<string>("Enter ICAO24 address:");
        var aircraft = await _aircraftDataService.GetAircraftByIcaoAsync(icao);

        if (aircraft == null)
        {
            AnsiConsole.MarkupLine($"[red]Aircraft with ICAO {icao} not found.[/]");
            return;
        }

        var panel = new Panel(
            $"[blue]Callsign:[/] {aircraft.Callsign ?? "N/A"}\n" +
            $"[blue]Position:[/] {aircraft.Latitude?.ToString("F4") ?? "N/A"}, {aircraft.Longitude?.ToString("F4") ?? "N/A"}\n" +
            $"[blue]Altitude:[/] {aircraft.Altitude?.ToString() ?? "N/A"} ft\n" +
            $"[blue]Ground Speed:[/] {aircraft.GroundSpeed?.ToString("F1") ?? "N/A"} kts\n" +
            $"[blue]Heading:[/] {aircraft.Heading?.ToString("F0") ?? "N/A"}°\n" +
            $"[blue]Vertical Rate:[/] {aircraft.VerticalRate?.ToString() ?? "N/A"} ft/min\n" +
            $"[blue]Last Seen:[/] {aircraft.LastSeen:HH:mm:ss}"
        );
        panel.Header = new PanelHeader($"Aircraft {aircraft.Icao24}");
        panel.Border = BoxBorder.Rounded;

        AnsiConsole.Write(panel);
    }

    private async Task AddSampleAircraftAsync()
    {
        var icao = AnsiConsole.Ask<string>("Enter ICAO24 address:");
        var callsign = AnsiConsole.Ask<string>("Enter callsign:");

        var aircraft = new AircraftData
        {
            Icao24 = icao,
            Callsign = callsign,
            Latitude = Random.Shared.NextDouble() * 180 - 90,
            Longitude = Random.Shared.NextDouble() * 360 - 180,
            Altitude = Random.Shared.Next(10000, 40000),
            GroundSpeed = Random.Shared.Next(300, 500),
            Heading = Random.Shared.Next(0, 360),
            VerticalRate = Random.Shared.Next(-1000, 1000)
        };

        await _aircraftDataService.AddOrUpdateAircraftAsync(aircraft);
        AnsiConsole.MarkupLine($"[green]Aircraft {icao} added successfully![/]");
    }
}
