using Database.Common.Data;
using Database.Common.Models;
using Database.Web.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Database.Web.Api.Controllers
{
    [Route("api/tables")]
    [ApiController]
    public class TablesController(DatabaseContext databaseContext) : ControllerBase
    {
        [HttpGet("{databaseName}")]
        public async Task<ActionResult> GetAll(string databaseName)
        {
            var database = await databaseContext.GetDatabaseAsync(databaseName);
            if (database == null)
            {
                return NotFound($"Database {databaseName} not found.");
            }

            return Ok(database.Tables);
        }

        [HttpGet("{databaseName}/table/{tableName}")]
        public async Task<ActionResult> Get(string databaseName, string tableName)
        {
            var database = await databaseContext.GetDatabaseAsync(databaseName);
            if (database == null)
            {
                return NotFound($"Database {databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            return Ok(table);
        }

        [HttpPost("{databaseName}")]
        public async Task<ActionResult> CreateTable(string databaseName, [FromBody] TableModelForCreate table)
        {
            var database = await databaseContext.GetDatabaseAsync(databaseName);
            if (database == null)
            {
                return NotFound($"Database {databaseName} not found.");
            }
            if (table == null)
            {
                return BadRequest($"{nameof(table)} is null.");
            }

            await databaseContext.AddTableToDatabaseAsync(databaseName, table.Name, table.Properties);

            return Ok();
        }

        [HttpDelete("{databaseName}/table/{tableName}")]
        public async Task<ActionResult> RemoveTable(string databaseName, string tableName)
        {
            var database = await databaseContext.GetDatabaseAsync(databaseName);

            await databaseContext.RemoveTableAsync(databaseName, tableName);

            return NoContent();
        }

        [HttpPut("{databaseName}/table/{tableName}/row")]
        public async Task<ActionResult> InsertRow(string databaseName, string tableName, [FromBody] List<string> row)
        {
            await databaseContext.InsertRowToTableAsync(databaseName, tableName, row);
            return NoContent();
        }

        [HttpDelete("{databaseName}/table/{tableName}/row/{index}")]
        public async Task<ActionResult> RemoveRow(string databaseName, string tableName, int index)
        {
            await databaseContext.RemoveRowInTableAsync(databaseName, tableName, index);
            return NoContent();
        }

        [HttpPut("{databaseName}/table/{tableName}/property")]
        public async Task<ActionResult> AddPropertyToTable(string databaseName, string tableName, [FromBody] Property property)
        {
            await databaseContext.AddPropertyToTableAsync(databaseName, tableName, property);
            return Ok();
        }

        [HttpDelete("{databaseName}/table/{tableName}/property/{name}")]
        public async Task<ActionResult> AddPropertyToTable(string databaseName, string tableName, string name)
        {
            await databaseContext.RemovePropertyFromTableAsync(databaseName, tableName, name);
            return Ok();
        }

        [HttpGet("{databaseName}/table/{firstTableName}/join/{secondTableName}/on/{firstTablePropertyName}/and/{secondTablePropertyName}")]
        public async Task<ActionResult> JoinTables(string databaseName, string firstTableName, string secondTableName, string firstTablePropertyName, string secondTablePropertyName)
        {
            var table = await databaseContext.JoinTablesAsync(databaseName, firstTableName, secondTableName, firstTablePropertyName, secondTablePropertyName);
            return Ok(table);
        }
    }
}
