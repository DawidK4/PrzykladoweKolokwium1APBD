using Microsoft.AspNetCore.Mvc;
using Group = System.Text.RegularExpressions.Group;

namespace Kolokwium1.Controllers;

[ApiController]
[Route("api/groups")] 
public class GroupsController(IDbService db) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await db.GetGroupDetailsByIdAsync(id);
        if (result is null) return NotFound($"Group with id:{id} does not exits");
        return Ok(result);
    }
}