using System.Data;
using ASP_NET.Data;
using ASP_NET.Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public RatesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public object Get()
        {
            return new { result = "GET request was received" };
        }

        [HttpPost]
        public object Post([FromBody] RequestData data)
        {
            String result = null!;
            if (data == null
                || data.ItemId == null
                || data.Value == null
                || data.UserId == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                result = $"Missing parameters: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
            }
            else
            {
                Guid itemId, userId;
                int value;
                try
                {
                    itemId = Guid.Parse(data.ItemId);
                    userId = Guid.Parse(data.UserId);
                    value = Convert.ToInt32(data.Value);
                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.ItemId == itemId && r.UserId == userId);
                    if (rate is null)
                    {
                        _dataContext.Rates.Add(new()
                        {
                            ItemId = itemId,
                            UserId = userId,
                            Rating = value
                        });
                        _dataContext.SaveChanges();

                        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
                        result = $"Data added";

                    }
                    else if (rate.Rating != value)
                    {
                        rate.Rating = value;
                        _dataContext.SaveChanges();
                        
                        HttpContext.Response.StatusCode = StatusCodes.Status202Accepted;
                        result = $"Data updated";
                    }
                    else
                    {
                        HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                        result = $"Data has already exist: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
                    }
                }
                catch
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    result = $"Parameters validation error: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
                }

            }

            return new { result };
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

        [HttpDelete]
        public object Delete([FromBody] RequestData data)
        {
             String result = null!;
            if (data == null
                || data.ItemId == null
                || data.Value == null
                || data.UserId == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                result = $"Missing parameters: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
            }
            else
            {
                Guid itemId, userId;
                int value;
                try
                {
                    itemId = Guid.Parse(data.ItemId);
                    userId = Guid.Parse(data.UserId);
                    value = Convert.ToInt32(data.Value);
                    Rate? rate = _dataContext.Rates.FirstOrDefault(r => r.ItemId == itemId && r.UserId == userId);
                    if (rate is null)
                    {
                        HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                        result = $"data doesn't exist in DB: value={data.Value} user={data.UserId} item={data.ItemId}";
                    }
                    else
                    {
                        _dataContext.Rates.Remove(rate);
                        _dataContext.SaveChanges();
                        result = $"Data removed";
                    }
                }
                catch
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    result = $"Parameters validation error: value={data?.Value} user={data?.UserId} item={data?.ItemId}";
                }

            }

            return new { result };
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
        public string ItemId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}

