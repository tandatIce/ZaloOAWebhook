using Dapper;
using ZaloOAWebhook.Context;
using ZaloOAWebhook.IRepository;

namespace ZaloOAWebhook.Repository
{
    public class CustomerRepository : ICustomerRepoitory
    {
        private readonly StoreDBContext _context;

        public CustomerRepository(StoreDBContext context)
        {
            _context = context;
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
