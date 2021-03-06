using System;
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

public class PersistAuthCodeCommandHandlerTests
{
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly PersistAuthCodeCommandHandler _handler;

    public PersistAuthCodeCommandHandlerTests()
    {
        _dbContextMock = new Mock<IDbContext>();
        _handler = new PersistAuthCodeCommandHandler(_dbContextMock.Object, new NullLogger<PersistAuthCodeCommandHandler>());
    }

    [Fact]
    public async Task Insert_AuthCode_CorrectTable()
    {
        // Arrange
        var authCode = new AuthCode("PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", Guid.NewGuid());
        var command = new PersistAuthCodeCommand(authCode);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteCommandAsync(
            It.Is<DatabaseQuery>(q => q.Sql.StartsWith("INSERT INTO auth_access_code"))), Times.Once);
    }

    [Fact]
    public async Task Handle_Code_ReturnTrue()
    {
        // Arrange
        var authCode = new AuthCode("PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", Guid.NewGuid());
        var command = new PersistAuthCodeCommand(authCode);

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
        var authCode = new AuthCode("PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", Guid.NewGuid());
        var command = new PersistAuthCodeCommand(authCode);

        _dbContextMock.Setup(db => db.ExecuteCommandAsync(It.IsAny<DatabaseQuery>())).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}