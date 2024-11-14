using Database.Common.Data;
using Microsoft.AspNetCore.Mvc;

namespace Database.Web.Api.Controllers
{
    [Route("api/databases")]
    [ApiController]
    public class DatabasesController(DatabaseContext databaseContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var databases = await databaseContext.LoadDatabaseAsync();
            return Ok(databases);
        }

        [HttpGet("{name}")]
        public async Task<ActionResult> Get(string name)
        {
            var database = await databaseContext.GetDatabaseAsync(name);
            if (database == null)
            {
                return NotFound($"Database {name} not found.");
            }

            return Ok(database);
        }

        [HttpPost]
        public async Task<ActionResult> CreateDatabase([FromBody] string name)
        {
            await databaseContext.CreateDatabaseAsync(name);
            return Created();
        }

        [HttpDelete("{name}")]
        public async Task<ActionResult> RemoveDatabase(string name)
        {
            await databaseContext.RemoveDatabaseAsync(name);
            return NoContent();
        }
    }
}
