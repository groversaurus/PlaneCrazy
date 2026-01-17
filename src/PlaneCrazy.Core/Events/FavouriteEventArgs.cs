namespace PlaneCrazy.Core.Events;

public class FavouriteEventArgs : EventArgs
{
    public string FavouriteId { get; set; } = string.Empty;
    public string FavouriteName { get; set; } = string.Empty;
}
