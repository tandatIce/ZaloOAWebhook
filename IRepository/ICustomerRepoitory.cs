using ZaloOAWebhook.Models;

namespace ZaloOAWebhook.IRepository
{
    public interface ICustomerRepoitory
    {
        public Task updateUserIdByPhoneNumber(string phoneNumber,string databaseName, string UserId);
        public Task<Point?> GetPointCurrentByUserId(string userId, string databaseName);
    }
}
