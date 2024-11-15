using Dapper;
using ZaloOAWebhook.Context;
using ZaloOAWebhook.IRepository;
using ZaloOAWebhook.Models;

namespace ZaloOAWebhook.Repository
{
    public class CustomerRepository : ICustomerRepoitory
    {
        private readonly StoreDBContext _context;

        public CustomerRepository(StoreDBContext context)
        {
            _context = context;
        }

        public async Task<Point?> GetPointCurrentByUserId(string userId, string databaseName)
        {
            using (var dBConnectionCustomer = _context.CreateConnection(databaseName))
            {
                string sql = "SELECT TOP 1 p.*\r\n" +
                             "FROM [dbo].[POINT] p\r\n" +
                             "JOIN [dbo].[cd_cstmr] c ON p.CD_CSTMR = c.CD_CSTMR\r\n" +
                             $"WHERE c.USER_ID = '{userId}'\r\n" +
                             "ORDER BY p.UPDATE_DATE DESC;";
                var pointRow = await dBConnectionCustomer.QueryAsync<Point>(sql);

                if (pointRow.Count() == 0)
                {
                    return null;
                }

                return pointRow.ToList()[0];
               
            }
        }

        public async Task updateUserIdByPhoneNumber(string phoneNumber, string databaseName, string userId)
        {
            using (var dBConnectionCustomer = _context.CreateConnection(databaseName))
            {
                string sql = $"UPDATE CD_CSTMR SET USER_ZALO_ID = '{userId}' WHERE MOBILE= '{phoneNumber}'";
                await dBConnectionCustomer.ExecuteAsync(sql);
            }
        }
    }
}
