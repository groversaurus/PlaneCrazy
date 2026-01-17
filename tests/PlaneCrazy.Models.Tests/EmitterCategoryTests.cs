namespace PlaneCrazy.Models.Tests;

public class EmitterCategoryTests
{
    [Fact]
    public void EmitterCategory_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)EmitterCategory.None);
        Assert.Equal(1, (int)EmitterCategory.Light);
        Assert.Equal(2, (int)EmitterCategory.Small);
        Assert.Equal(3, (int)EmitterCategory.Large);
        Assert.Equal(5, (int)EmitterCategory.Heavy);
        Assert.Equal(7, (int)EmitterCategory.Rotorcraft);
        Assert.Equal(12, (int)EmitterCategory.UAV);
    }

    [Fact]
    public void EmitterCategory_CanBeAssignedToAircraft()
    {
        // Arrange & Act
        var aircraft = new Aircraft
        {
            Hex = "test123",
            Category = EmitterCategory.Heavy
        };

        // Assert
        Assert.Equal(EmitterCategory.Heavy, aircraft.Category);
    }
}
