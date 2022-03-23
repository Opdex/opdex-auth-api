using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data;
using Opdex.Auth.Infrastructure.Data.Entities;
using Opdex.Auth.Infrastructure.Data.Handlers;
using Xunit;

namespace Opdex.Auth.Infrastructure.Tests.Data.Handlers;

public class SelectAuthSuccessByConnectionIdQueryHandlerTests
{
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly SelectAuthSuccessByConnectionIdQueryHandler _handler;

    public SelectAuthSuccessByConnectionIdQueryHandlerTests()
    {
        _dbContextMock = new Mock<IDbContext>();
        _handler = new SelectAuthSuccessByConnectionIdQueryHandler(_dbContextMock.Object, new NullLogger<SelectAuthSuccessByConnectionIdQueryHandler>());
    }

    [Fact]
    public async Task Handle_Query_Limit1()
    {
        // Arrange
        var query = new SelectAuthSuccessByConnectionIdQuery("jwf892jfieurwnfqjkr3n2ogfn3wklfnekrlf");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteFindAsync<It.IsAnyType>(
            It.Is<DatabaseQuery>(q => q.Sql.EndsWith("LIMIT 1;"))), Times.Once);
    }

    [Fact]
    public async Task Handle_Query_FilterByConnectionId()
    {
        // Arrange
        var query = new SelectAuthSuccessByConnectionIdQuery("jwf892jfieurwnfqjkr3n2ogfn3wklfnekrlf");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteFindAsync<It.IsAnyType>(
            It.Is<DatabaseQuery>(q => q.Sql.Contains("ConnectionId = @ConnectionId"))), Times.Once);
    }

    [Fact]
    public async Task Handle_NoResult_ReturnNull()
    {
        // Arrange
        var query = new SelectAuthSuccessByConnectionIdQuery("jwf892jfieurwnfqjkr3n2ogfn3wklfnekrlf");
        _dbContextMock.Setup(callTo => callTo.ExecuteFindAsync<AuthSuccessEntity?>(It.IsAny<DatabaseQuery>()))
            .ReturnsAsync((AuthSuccessEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(null);
    }

    [Fact]
    public async Task Handle_Result_ReturnMapped()
    {
        // Arrange
        const string connectionId = "jwf892jfieurwnfqjkr3n2ogfn3wklfnekrlf";
        var authSuccessEntity = new AuthSuccessEntity(new AuthSuccess(connectionId, "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", DateTime.UtcNow.AddMinutes(1)));
        var query = new SelectAuthSuccessByConnectionIdQuery(connectionId);
        _dbContextMock.Setup(callTo => callTo.ExecuteFindAsync<AuthSuccessEntity>(It.IsAny<DatabaseQuery>()))
            .ReturnsAsync(authSuccessEntity);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result!.ConnectionId.Should().Be(authSuccessEntity.ConnectionId);
        result!.Signer.Should().Be(authSuccessEntity.Signer);
        result!.Expiry.Should().Be(authSuccessEntity.Expiry);
    }
}