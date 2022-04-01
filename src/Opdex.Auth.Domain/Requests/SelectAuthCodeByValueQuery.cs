using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class SelectAuthCodeByValueQuery : IRequest<AuthCode?>
{
    public SelectAuthCodeByValueQuery(Guid value)
    {
        Value = value;
    }
    
    public Guid Value { get; }
}