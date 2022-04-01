using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class SelectAuthCodeByStampQuery : IRequest<AuthCode?>
{
    public SelectAuthCodeByStampQuery(Guid stamp)
    {
        Stamp = stamp;
    }
    
    public Guid Stamp { get; }
}