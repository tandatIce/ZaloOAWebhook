using Microsoft.Data.SqlClient;

namespace ZaloOAWebhook.Context;

public partial class ZaloOatestContext 
{

    private readonly IConfiguration _configuration;
    public ZaloOatestContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SqlConnection CreateConnection()
    {
        string? connectString = _configuration["Logging:ConnectionStrings:DefaultConnection"];

        return new SqlConnection(connectString);
    }


}
