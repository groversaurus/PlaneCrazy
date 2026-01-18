namespace PlaneCrazy.Infrastructure;

/// <summary>
/// Static class that resolves and ensures the existence of PlaneCrazy folder structure
/// in the user's Documents directory.
/// </summary>
public static class PlaneCrazyPaths
{
    /// <summary>
    /// Gets the base PlaneCrazy folder path in the user's Documents directory.
    /// </summary>
    public static string BasePath { get; }

    /// <summary>
    /// Gets the Config subfolder path.
    /// </summary>
    public static string ConfigPath { get; }

    /// <summary>
    /// Gets the Data subfolder path.
    /// </summary>
    public static string DataPath { get; }

    /// <summary>
    /// Gets the Events subfolder path.
    /// </summary>
    public static string EventsPath { get; }

    /// <summary>
    /// Static constructor that initializes all paths and ensures folders exist.
    /// </summary>
    static PlaneCrazyPaths()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        
        // If MyDocuments is not available (e.g., on Linux), fall back to user's home directory
        if (string.IsNullOrEmpty(documentsPath))
        {
            documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        
        BasePath = Path.Combine(documentsPath, "PlaneCrazy");
        
        ConfigPath = Path.Combine(BasePath, "Config");
        DataPath = Path.Combine(BasePath, "Data");
        EventsPath = Path.Combine(BasePath, "Events");

        // Ensure all directories exist
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(ConfigPath);
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(EventsPath);
    }
}
