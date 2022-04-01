using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data;
using Opdex.Auth.Infrastructure.Data.Handlers;
using Xunit;

namespace Opdex.Auth.Infrastructure.Tests.Data.Handlers;

public class DeleteAuthCodeCommandHandlerTests
{
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly FakeDeleteAuthCodeCommandHandler _handler;

    public DeleteAuthCodeCommandHandlerTests()
    {
        _dbContextMock = new Mock<IDbContext>();
        _handler = new FakeDeleteAuthCodeCommandHandler(_dbContextMock.Object, new NullLogger<FakeDeleteAuthCodeCommandHandler>());
    }

    [Fact]
    public async Task Handle_Command_Limit1()
    {
        // Arrange
        var authCode = new AuthCode(Guid.NewGuid(), "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", Guid.NewGuid(), DateTime.UtcNow);
        var query = new DeleteAuthCodeCommand(authCode);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteCommandAsync(
            It.Is<DatabaseQuery>(q => q.Sql.EndsWith("LIMIT 1;"))), Times.Once);
    }

    [Fact]
    public async Task Handle_Command_Filter()
    {
        // Arrange
        var authCode = new AuthCode(Guid.NewGuid(), "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", Guid.NewGuid(), DateTime.UtcNow);
        var query = new DeleteAuthCodeCommand(authCode);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteCommandAsync(
            It.Is<DatabaseQuery>(q => q.Sql.Contains("AccessCode = @AccessCode"))), Times.Once);
    }
}

public class FakeDeleteAuthCodeCommandHandler : DeleteAuthCodeCommandHandler
{
    public FakeDeleteAuthCodeCommandHandler(IDbContext dbContext, ILogger<FakeDeleteAuthCodeCommandHandler> logger)
        : base(dbContext, logger)
    {
    }

    public Task Handle(DeleteAuthCodeCommand command, CancellationToken cancellationToken)
    {
        return base.Handle(command, cancellationToken);
    }
}