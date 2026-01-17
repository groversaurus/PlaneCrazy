namespace PlaneCrazy.Paths;

/// <summary>
/// Provides static paths to PlaneCrazy application directories.
/// Resolves Documents/PlaneCrazy folder and subfolders (Config, Data, Events).
/// Ensures directories exist when accessed.
/// </summary>
public static class PlaneCrazyPaths
{
    private static readonly string _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private static readonly string _basePath = Path.Combine(_documentsPath, "PlaneCrazy");

    /// <summary>
    /// Gets the base PlaneCrazy directory path (Documents/PlaneCrazy).
    /// Ensures the directory exists when accessed.
    /// </summary>
    public static string BaseDirectory
    {
        get
        {
            EnsureDirectoryExists(_basePath);
            return _basePath;
        }
    }

    /// <summary>
    /// Gets the Config subdirectory path (Documents/PlaneCrazy/Config).
    /// Ensures the directory exists when accessed.
    /// </summary>
    public static string ConfigDirectory => GetSubdirectoryPath("Config");

    /// <summary>
    /// Gets the Data subdirectory path (Documents/PlaneCrazy/Data).
    /// Ensures the directory exists when accessed.
    /// </summary>
    public static string DataDirectory => GetSubdirectoryPath("Data");

    /// <summary>
    /// Gets the Events subdirectory path (Documents/PlaneCrazy/Events).
    /// Ensures the directory exists when accessed.
    /// </summary>
    public static string EventsDirectory => GetSubdirectoryPath("Events");

    /// <summary>
    /// Gets a subdirectory path under the base PlaneCrazy directory.
    /// Ensures both the base directory and subdirectory exist.
    /// </summary>
    /// <param name="subdirectoryName">The name of the subdirectory.</param>
    /// <returns>The full path to the subdirectory.</returns>
    private static string GetSubdirectoryPath(string subdirectoryName)
    {
        // Access BaseDirectory to ensure it exists
        var basePath = BaseDirectory;
        
        var path = Path.Combine(basePath, subdirectoryName);
        EnsureDirectoryExists(path);
        return path;
    }

    /// <summary>
    /// Ensures the specified directory exists, creating it if necessary.
    /// Directory.CreateDirectory is thread-safe and idempotent - it will create
    /// the directory if it doesn't exist, or do nothing if it already exists.
    /// </summary>
    /// <param name="path">The directory path to ensure exists.</param>
    private static void EnsureDirectoryExists(string path)
    {
        Directory.CreateDirectory(path);
    }
}
