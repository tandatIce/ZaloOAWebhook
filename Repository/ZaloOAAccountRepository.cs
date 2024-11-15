using Dapper;
using ZaloOAWebhook.Context;
using ZaloOAWebhook.IRepository;
using ZaloOAWebhook.Models;

namespace ZaloOAWebhook.Repository
{
    public class ZaloOAAccountRepository : IZaloOAAccountRepository
    {
        private readonly ZaloOatestContext _context;

        public ZaloOAAccountRepository(ZaloOatestContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ZaloOaAccount>> GetAccountByAppId(string appId)
        {
            using (var dbConnectionAccountZaloOA = _context.CreateConnection())
            {
                string sql = $"SELECT * FROM Zalo_OA_Accounts WHERE App_Id='{appId}'";

                var OAAccount = await dbConnectionAccountZaloOA.QueryAsync<ZaloOaAccount>(sql);   //find account OA by app ID

                return OAAccount;
            }
            
        }

        public async Task UpdateRefeshAndAccessTokenByAppId(string appId, string accessToken, string refreshToken)
        {
            using (var dbConnectionAccountZaloOA = _context.CreateConnection())
            {

                string sql = $"UPDATE Zalo_OA_Accounts SET RefreshToken='{refreshToken}', AccessToken='{accessToken}' WHERE App_Id='{appId}'";
                await dbConnectionAccountZaloOA.ExecuteAsync(sql);
            }
            
        }
    }
}
