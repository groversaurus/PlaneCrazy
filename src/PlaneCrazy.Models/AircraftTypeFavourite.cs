namespace PlaneCrazy.Models;

public class AircraftTypeFavourite : Favourite
{
    public string AircraftType { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
}
