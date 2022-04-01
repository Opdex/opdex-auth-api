using System;

namespace Opdex.Auth.Api.SignalR;

public class AuthSessionConnectionException : Exception
{
    public AuthSessionConnectionException() : base("Auth session could not be linked to connection")
    {
        
    }
}