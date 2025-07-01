using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;

public class UserController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    public UserController(SqlDbContext sqlDbContext)
    {
        this.sqlDbContext = sqlDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto user)
    {
        var entity = new User() { Name = user.Name };
        sqlDbContext.Add(entity);
        await sqlDbContext.SaveChangesAsync();
        return Ok(new { Id = entity.Id });
    }
}