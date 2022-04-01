using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data;
using Opdex.Auth.Infrastructure.Data.Entities;
using Opdex.Auth.Infrastructure.Data.Handlers;
using Xunit;

namespace Opdex.Auth.Infrastructure.Tests.Data.Handlers;

public class SelectAuthCodeByValueQueryHandlerTests
{
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly SelectAuthCodeByValueQueryHandler _handler;

    public SelectAuthCodeByValueQueryHandlerTests()
    {
        _dbContextMock = new Mock<IDbContext>();
        _handler = new SelectAuthCodeByValueQueryHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Query_Limit1()
    {
        // Arrange
        var query = new SelectAuthCodeByValueQuery(Guid.NewGuid());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteFindAsync<It.IsAnyType>(
            It.Is<DatabaseQuery>(q => q.Sql.EndsWith("LIMIT 1;"))), Times.Once);
    }

    [Fact]
    public async Task Handle_Query_Filter()
    {
        // Arrange
        var query = new SelectAuthCodeByValueQuery(Guid.NewGuid());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(callTo => callTo.ExecuteFindAsync<It.IsAnyType>(
            It.Is<DatabaseQuery>(q => q.Sql.Contains("AccessCode = @AccessCode"))), Times.Once);
    }

    [Fact]
    public async Task Handle_NoResult_ReturnNull()
    {
        // Arrange
        var query = new SelectAuthCodeByValueQuery(Guid.NewGuid());
        _dbContextMock.Setup(callTo => callTo.ExecuteFindAsync<AuthCodeEntity?>(It.IsAny<DatabaseQuery>()))
            .ReturnsAsync((AuthCodeEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(null);
    }

    [Fact]
    public async Task Handle_Result_ReturnMapped()
    {
        // Arrange
        var code = Guid.NewGuid();
        var authCodeEntity = new AuthCodeEntity(new AuthCode(code, "connectionId", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(1)));
        _dbContextMock.Setup(callTo => callTo.ExecuteFindAsync<AuthCodeEntity>(It.IsAny<DatabaseQuery>()))
            .ReturnsAsync(authCodeEntity);

        // Act
        var result = await _handler.Handle(new SelectAuthCodeByValueQuery(code), CancellationToken.None);

        // Assert
        result!.Value.Should().Be(authCodeEntity.AccessCode);
        result!.Signer.Should().Be(authCodeEntity.Signer);
        result!.Stamp.Should().Be(authCodeEntity.Stamp);
        result!.Expiry.Should().Be(authCodeEntity.Expiry);
    }
}