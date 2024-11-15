using Microsoft.AspNetCore.Mvc;
using System.Data;
using ZaloOAWebhook.Context;
using ZaloOAWebhook.Models;
using Dapper;

namespace ZaloOAWebhook.Controllers
{
    [Route("")]
    [ApiController]
    public class TestConnectDataBaseController: ControllerBase
    {
        private readonly StoreDBContext _storeContext;
        private readonly ILogger<TestConnectDataBaseController> _logger;

        public TestConnectDataBaseController(StoreDBContext storeContext, ILogger<TestConnectDataBaseController> logger)
        {
            _storeContext = storeContext;
            _logger = logger;
        }

        [HttpGet("{DatabaseName}")]
        public async Task<IActionResult> GetCustomers([FromRoute] string DatabaseName)
        {
           
            using (IDbConnection dbConnection = _storeContext.CreateConnection(DatabaseName))
            {
                _logger.LogInformation("hello");
                dbConnection.Open();               
                var result = await dbConnection.QueryAsync<Customer>("select top (100) * from CD_CSTMR where CD_CSTMR= '100010149564'");
                
                return Ok(result);
            }

            
        }
    }
}
