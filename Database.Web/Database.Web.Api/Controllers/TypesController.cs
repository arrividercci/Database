using Database.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Database.Web.Api.Controllers
{
    [Route("api/types")]
    [ApiController]
    public class TypesController() : ControllerBase
    {
        [HttpGet]
        public ActionResult GetAll()
        {
            var names = Enum.GetNames(typeof(PropertyType));
            return Ok(names);
        }
    }
}
