namespace PlaneCrazy.Models;

public class AirportFavourite : Favourite
{
    public string AirportCode { get; set; } = string.Empty;
    public string? AirportName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}
