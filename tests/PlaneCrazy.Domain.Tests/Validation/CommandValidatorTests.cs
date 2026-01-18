using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Validation;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Validation;

public class CommandValidatorTests
{
    [Fact]
    public void ValidateAddComment_ValidCommand_ReturnsSuccess()
    {
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "A12345",
            Text = "Great aircraft!"
        };

        var result = CommandValidator.ValidateAddComment(command);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateAddComment_InvalidEntityType_ReturnsFailure()
    {
        var command = new AddCommentCommand
        {
            EntityType = "Plane",
            EntityId = "A12345",
            Text = "Great aircraft!"
        };

        var result = CommandValidator.ValidateAddComment(command);
        
        Assert.False(result.IsValid);
        Assert.Contains("Entity type must be one of", result.ErrorMessage);
    }

    [Fact]
    public void ValidateAddComment_InvalidEntityId_ReturnsFailure()
    {
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "12345", // Too short
            Text = "Great aircraft!"
        };

        var result = CommandValidator.ValidateAddComment(command);
        
        Assert.False(result.IsValid);
        Assert.Contains("ICAO24", result.ErrorMessage);
    }

    [Fact]
    public void ValidateAddComment_EmptyText_ReturnsFailure()
    {
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "A12345",
            Text = ""
        };

        var result = CommandValidator.ValidateAddComment(command);
        
        Assert.False(result.IsValid);
        Assert.Contains("cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFavouriteAircraft_ValidCommand_ReturnsSuccess()
    {
        var command = new FavouriteAircraftCommand
        {
            Icao24 = "A12345",
            Registration = "N12345",
            TypeCode = "B738"
        };

        var result = CommandValidator.ValidateFavouriteAircraft(command);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFavouriteAircraft_InvalidIcao24_ReturnsFailure()
    {
        var command = new FavouriteAircraftCommand
        {
            Icao24 = "XYZ123", // Invalid hex
        };

        var result = CommandValidator.ValidateFavouriteAircraft(command);
        
        Assert.False(result.IsValid);
        Assert.Contains("hexadecimal", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFavouriteAircraftType_ValidCommand_ReturnsSuccess()
    {
        var command = new FavouriteAircraftTypeCommand
        {
            TypeCode = "B738",
            TypeName = "Boeing 737-800"
        };

        var result = CommandValidator.ValidateFavouriteAircraftType(command);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFavouriteAircraftType_InvalidTypeCode_ReturnsFailure()
    {
        var command = new FavouriteAircraftTypeCommand
        {
            TypeCode = "A" // Too short
        };

        var result = CommandValidator.ValidateFavouriteAircraftType(command);
        
        Assert.False(result.IsValid);
        Assert.Contains("at least 2 characters", result.ErrorMessage);
    }

    [Fact]
    public void ValidateFavouriteAirport_ValidCommand_ReturnsSuccess()
    {
        var command = new FavouriteAirportCommand
        {
            IcaoCode = "KJFK",
            Name = "John F. Kennedy International Airport"
        };

        var result = CommandValidator.ValidateFavouriteAirport(command);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateFavouriteAirport_InvalidIcaoCode_ReturnsFailure()
    {
        var command = new FavouriteAirportCommand
        {
            IcaoCode = "JFK" // Too short
        };

        var result = CommandValidator.ValidateFavouriteAirport(command);
        
        Assert.False(result.IsValid);
        Assert.Contains("4 characters", result.ErrorMessage);
    }

    [Fact]
    public void ValidateEditComment_ValidCommand_ReturnsSuccess()
    {
        var command = new EditCommentCommand
        {
            CommentId = Guid.NewGuid(),
            EntityType = "Aircraft",
            EntityId = "A12345",
            NewText = "Updated comment"
        };

        var result = CommandValidator.ValidateEditComment(command);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateDeleteComment_ValidCommand_ReturnsSuccess()
    {
        var command = new DeleteCommentCommand
        {
            CommentId = Guid.NewGuid(),
            EntityType = "Aircraft",
            EntityId = "A12345",
            Reason = "Spam"
        };

        var result = CommandValidator.ValidateDeleteComment(command);
        
        Assert.True(result.IsValid);
    }
}
