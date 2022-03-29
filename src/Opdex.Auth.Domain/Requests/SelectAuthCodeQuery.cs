using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class SelectAuthCodeQuery : IRequest<AuthCode?>
{
    public SelectAuthCodeQuery(Guid value)
    {
        Value = value;
    }
    
    public Guid Value { get; }
}