using System.Threading.Tasks;

namespace Firepuma.MicroServices.Auth
{
    public interface IMicroServiceTokenProvider
    {
        Task<TokenResponse> GetToken();
    }
}
