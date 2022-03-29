namespace Opdex.Auth.Api.Models;

public class StatusResponseModel
{
    public StatusResponseModel(string commit)
    {
        Commit = commit;
    }
    
    public string Commit { get; }
}