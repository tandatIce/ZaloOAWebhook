namespace ZaloOAWebhook.IRepository
{
    public interface ICustomerRepoitory
    {
        public Task updateUserIdByPhoneNumber(string phoneNumber,string databaseName, string UserId);
    }
}
