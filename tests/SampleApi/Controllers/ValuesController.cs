using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public async Task<ValuesList> Get()
        {
            var model = new ValuesList
            {
                Items = new[]
                {
                    new Value { Id = 1, Name = "value 1", CreatedAtUtc = DateTime.UtcNow }
                }
            };

            return await Task.FromResult(model);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Value), 200)]
        public IActionResult Get(int id)
        {
            return Ok(new Value { Id = 1, Name = "value 1", CreatedAtUtc = DateTime.UtcNow });
        }
    }
}
