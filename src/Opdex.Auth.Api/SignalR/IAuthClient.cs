using System;
using System.Threading.Tasks;

namespace Opdex.Auth.Api.SignalR;

public interface IAuthClient
{
    Task OnAuthenticated(string code);
}