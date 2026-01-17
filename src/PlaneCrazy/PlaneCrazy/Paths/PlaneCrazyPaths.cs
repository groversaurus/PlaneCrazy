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
    public static string ConfigDirectory
    {
        get
        {
            var path = Path.Combine(_basePath, "Config");
            EnsureDirectoryExists(path);
            return path;
        }
    }

    /// <summary>
    /// Gets the Data subdirectory path (Documents/PlaneCrazy/Data).
    /// Ensures the directory exists when accessed.
    /// </summary>
    public static string DataDirectory
    {
        get
        {
            var path = Path.Combine(_basePath, "Data");
            EnsureDirectoryExists(path);
            return path;
        }
    }

    /// <summary>
    /// Gets the Events subdirectory path (Documents/PlaneCrazy/Events).
    /// Ensures the directory exists when accessed.
    /// </summary>
    public static string EventsDirectory
    {
        get
        {
            var path = Path.Combine(_basePath, "Events");
            EnsureDirectoryExists(path);
            return path;
        }
    }

    /// <summary>
    /// Ensures the specified directory exists, creating it if necessary.
    /// </summary>
    /// <param name="path">The directory path to ensure exists.</param>
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
