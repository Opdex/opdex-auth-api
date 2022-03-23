using System.Threading;
using System.Threading.Tasks;
using Moq;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data;
using Opdex.Auth.Infrastructure.Data.Handlers;
using Xunit;

namespace Opdex.Auth.Infrastructure.Tests.Data.Handlers;

public class IsAdminQueryHandlerTests
{
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly IsAdminQueryHandler _handler;

    public IsAdminQueryHandlerTests()
    {
        _dbContextMock = new Mock<IDbContext>();
        _handler = new IsAdminQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Query_IsAdmin_CorrectTable()
    {
        // Arrange
        var query = new IsAdminQuery("PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteScalarAsync<bool>(
            It.Is<DatabaseQuery>(q => q.Sql.Contains("SELECT 1 FROM admin"))), Times.Once);
    }

    [Fact]
    public async Task Query_IsAdmin_Limit1()
    {
        // Arrange
        var query = new IsAdminQuery("PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteScalarAsync<bool>(
            It.Is<DatabaseQuery>(q => q.Sql.Contains("LIMIT 1"))), Times.Once);
    }
}