using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data;
using Opdex.Auth.Infrastructure.Data.Handlers;
using Xunit;

namespace Opdex.Auth.Infrastructure.Tests.Data.Handlers;

public class PersistAuthSuccessCommandHandlerTests
{
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly PersistAuthSuccessCommandHandler _handler;

    public PersistAuthSuccessCommandHandlerTests()
    {
        _dbContextMock = new Mock<IDbContext>();
        _handler = new PersistAuthSuccessCommandHandler(_dbContextMock.Object, new NullLogger<PersistAuthSuccessCommandHandler>());
    }

    [Fact]
    public async Task Insert_AuthSuccess_CorrectTable()
    {
        // Arrange
        var authSuccess = new AuthSuccess("CONNECTION_ID", "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ");
        var command = new PersistAuthSuccessCommand(authSuccess);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteCommandAsync(
            It.Is<DatabaseQuery>(q => q.Sql.StartsWith("INSERT INTO auth_success"))), Times.Once);
    }

    [Fact]
    public async Task Handle_Success_ReturnTrue()
    {
        // Arrange
        var authSuccess = new AuthSuccess("CONNECTION_ID", "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ");
        var command = new PersistAuthSuccessCommand(authSuccess);

        _dbContextMock.Setup(db => db.ExecuteCommandAsync(It.IsAny<DatabaseQuery>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Fail_ReturnFalse()
    {
        // Arrange
        var authSuccess = new AuthSuccess("CONNECTION_ID", "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ");
        var command = new PersistAuthSuccessCommand(authSuccess);

        _dbContextMock.Setup(db => db.ExecuteCommandAsync(It.IsAny<DatabaseQuery>())).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}