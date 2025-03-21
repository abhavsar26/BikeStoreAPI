using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BikeStoreApp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get","post", Route = null)] HttpRequest req,
            [Sql("dbo.staffs", "SqlConnection")] IAsyncCollector<staffs> staff,
            ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            staffs staff1 = JsonConvert.DeserializeObject<staffs>(requestBody);

            await staff.AddAsync(staff1);
            await staff.FlushAsync();


            return new OkObjectResult("Staff Added");
        }
    }
}
