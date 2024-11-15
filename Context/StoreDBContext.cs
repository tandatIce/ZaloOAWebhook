using Microsoft.Data.SqlClient;

namespace ZaloOAWebhook.Context;

public partial class StoreDBContext 
{

    private readonly IConfiguration _configuration;
    public StoreDBContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public SqlConnection CreateConnection(string DatabaseName)
    {
        string? serverID = _configuration["DatabaseSettings:Server"];
        string? userId = _configuration["DatabaseSettings:UserId"];
        string? password = _configuration["DatabaseSettings:Password"];
        string connectString = $"Server={serverID};Database={DatabaseName};User Id={userId};Password={password};TrustServerCertificate=True;";

        return new SqlConnection(connectString);
    }
}
