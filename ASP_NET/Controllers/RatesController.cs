using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        [HttpGet]
        public object Get()
        {
            return new { result = "GET request was received" };
        }

        [HttpPost]
        public object Post([FromBody] RequestData data)
        {
            return new { result = $"POST request was received with value={data.Value}" };
        }

        [HttpPut]
        public object Put([FromBody] RequestData data)
        {
            return new { result = $"PUT request was received with value={data.Value}" };
        }

        [HttpPatch]
        public object Patch([FromBody] RequestData data)
        {
            return new { result = $"PUT request was received with value={data.Value}" };
        }

        public object Default()
        {
            String str;
            using (var stream = new StreamReader(HttpContext.Request.Body))
            {
                str = stream.ReadToEndAsync().Result;
            }
            
            return new
            {
                result = $"{HttpContext.Request.Method} request was received with value={str}" 
            };
        }
    }
    public class RequestData
    {
        public string Value { get; set; } = null!;
    }
    /* Д.З. Реалізувати обмін данними з API методами PATCH та UNLINK */
}