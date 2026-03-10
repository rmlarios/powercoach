using CoachPlatform.Application.Features.Athletes.Queries.GetAthletesByCoach;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace CoachPlatform.UnitTests.Application.Queries;

public class GetAthletesByCoachQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetAthletesByCoachQueryHandler _handler;
    private readonly Guid _validCoachId = Guid.NewGuid();

    public GetAthletesByCoachQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAthletesByCoachQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithAthletes_ShouldReturnPagedResult()
    {
        // Arrange
        var athletes = new List<Athlete>
        {
            CreateAthleteWithCountry("John", "Doe", "john@example.com", "USA"),
            CreateAthleteWithCountry("Jane", "Smith", "jane@example.com", "Canada")
        };

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(athletes);

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var activeAthlete = CreateAthleteWithCountry("John", "Doe", "john@example.com", "USA");
        var inactiveAthlete = CreateAthleteWithCountry("Jane", "Smith", "jane@example.com", "Canada");
        inactiveAthlete.Deactivate();

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete> { activeAthlete, inactiveAthlete });

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            Status = "Active",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_WithEmailFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var athletes = new List<Athlete>
        {
            CreateAthleteWithCountry("John", "Doe", "john@example.com", "USA"),
            CreateAthleteWithCountry("Jane", "Smith", "jane@test.com", "Canada")
        };

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(athletes);

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            Email = "john",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Email.Should().Contain("john");
    }

    [Fact]
    public async Task Handle_WithCountryFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var athletes = new List<Athlete>
        {
            CreateAthleteWithCountry("John", "Doe", "john@example.com", "USA"),
            CreateAthleteWithCountry("Jane", "Smith", "jane@example.com", "Canada")
        };

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(athletes);

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            Country = "USA",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Country.Should().Be("USA");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldSearchByNameOrEmail()
    {
        // Arrange
        var athletes = new List<Athlete>
        {
            CreateAthleteWithCountry("John", "Doe", "john@example.com", "USA"),
            CreateAthleteWithCountry("Jane", "Smith", "jane@example.com", "Canada")
        };

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(athletes);

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            SearchTerm = "Smith",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var athletes = new List<Athlete>();
        for (int i = 1; i <= 15; i++)
        {
            athletes.Add(CreateAthleteWithCountry($"John{i}", $"Doe{i}", $"john{i}@example.com", "USA"));
        }

        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(athletes);

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            PageNumber = 2,
            PageSize = 5
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WithNoAthletes_ShouldReturnEmptyResult()
    {
        // Arrange
        _contextMock.Setup(x => x.Athletes)
            .ReturnsDbSet(new List<Athlete>());

        var query = new GetAthletesByCoachQuery
        {
            CoachId = _validCoachId,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private Athlete CreateAthleteWithCountry(string firstName, string lastName, string email, string country)
    {
        return Athlete.Create(
            _validCoachId,
            firstName,
            lastName,
            email,
            country: country);
    }
}
