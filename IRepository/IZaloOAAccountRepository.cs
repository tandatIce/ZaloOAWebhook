using System.Runtime.CompilerServices;
using ZaloOAWebhook.Models;

namespace ZaloOAWebhook.IRepository
{
    public interface IZaloOAAccountRepository
    {
        public Task<IEnumerable<ZaloOaAccount>> GetAccountByAppId(string appId);
        public Task UpdateRefeshAndAccessTokenByAppId(string appId,string accessToken,string refreshToken);
    }
}
