using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers;

[ApiController]
[Route("api/students")]
public class StudentController(IDbService db) : ControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var result = await db.RemoveStudentByIdAsync(id);
        if (result) return NoContent();
        return NotFound($"Student with id:{id} does not exist");
    }
}