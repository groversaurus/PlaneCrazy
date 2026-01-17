using PlaneCrazy.Paths;

namespace PlaneCrazy.Tests;

public class PlaneCrazyPathsTests
{
    [Fact]
    public void BaseDirectory_ShouldReturnPathInDocuments()
    {
        // Arrange
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Act
        var basePath = PlaneCrazyPaths.BaseDirectory;

        // Assert
        Assert.NotNull(basePath);
        Assert.NotEmpty(basePath);
        Assert.Contains("PlaneCrazy", basePath);
        Assert.StartsWith(documentsPath, basePath);
    }

    [Fact]
    public void BaseDirectory_ShouldEnsureDirectoryExists()
    {
        // Act
        var basePath = PlaneCrazyPaths.BaseDirectory;

        // Assert
        Assert.True(Directory.Exists(basePath), $"Directory should exist: {basePath}");
    }

    [Fact]
    public void ConfigDirectory_ShouldReturnPathUnderBase()
    {
        // Act
        var configPath = PlaneCrazyPaths.ConfigDirectory;
        var basePath = PlaneCrazyPaths.BaseDirectory;

        // Assert
        Assert.NotNull(configPath);
        Assert.NotEmpty(configPath);
        Assert.StartsWith(basePath, configPath);
        Assert.EndsWith("Config", configPath);
    }

    [Fact]
    public void ConfigDirectory_ShouldEnsureDirectoryExists()
    {
        // Act
        var configPath = PlaneCrazyPaths.ConfigDirectory;

        // Assert
        Assert.True(Directory.Exists(configPath), $"Directory should exist: {configPath}");
    }

    [Fact]
    public void DataDirectory_ShouldReturnPathUnderBase()
    {
        // Act
        var dataPath = PlaneCrazyPaths.DataDirectory;
        var basePath = PlaneCrazyPaths.BaseDirectory;

        // Assert
        Assert.NotNull(dataPath);
        Assert.NotEmpty(dataPath);
        Assert.StartsWith(basePath, dataPath);
        Assert.EndsWith("Data", dataPath);
    }

    [Fact]
    public void DataDirectory_ShouldEnsureDirectoryExists()
    {
        // Act
        var dataPath = PlaneCrazyPaths.DataDirectory;

        // Assert
        Assert.True(Directory.Exists(dataPath), $"Directory should exist: {dataPath}");
    }

    [Fact]
    public void EventsDirectory_ShouldReturnPathUnderBase()
    {
        // Act
        var eventsPath = PlaneCrazyPaths.EventsDirectory;
        var basePath = PlaneCrazyPaths.BaseDirectory;

        // Assert
        Assert.NotNull(eventsPath);
        Assert.NotEmpty(eventsPath);
        Assert.StartsWith(basePath, eventsPath);
        Assert.EndsWith("Events", eventsPath);
    }

    [Fact]
    public void EventsDirectory_ShouldEnsureDirectoryExists()
    {
        // Act
        var eventsPath = PlaneCrazyPaths.EventsDirectory;

        // Assert
        Assert.True(Directory.Exists(eventsPath), $"Directory should exist: {eventsPath}");
    }

    [Fact]
    public void AllDirectories_ShouldBeCreatedTogether()
    {
        // Act - Access all directories
        var basePath = PlaneCrazyPaths.BaseDirectory;
        var configPath = PlaneCrazyPaths.ConfigDirectory;
        var dataPath = PlaneCrazyPaths.DataDirectory;
        var eventsPath = PlaneCrazyPaths.EventsDirectory;

        // Assert - All should exist
        Assert.True(Directory.Exists(basePath));
        Assert.True(Directory.Exists(configPath));
        Assert.True(Directory.Exists(dataPath));
        Assert.True(Directory.Exists(eventsPath));
    }

    [Fact]
    public void Paths_ShouldBeConsistentAcrossMultipleCalls()
    {
        // Act - Call multiple times
        var basePath1 = PlaneCrazyPaths.BaseDirectory;
        var basePath2 = PlaneCrazyPaths.BaseDirectory;
        var configPath1 = PlaneCrazyPaths.ConfigDirectory;
        var configPath2 = PlaneCrazyPaths.ConfigDirectory;

        // Assert - Should return same paths
        Assert.Equal(basePath1, basePath2);
        Assert.Equal(configPath1, configPath2);
    }
}
