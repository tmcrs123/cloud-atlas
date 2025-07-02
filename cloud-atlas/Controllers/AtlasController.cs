using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AtlasController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    private readonly CosmosDbContext cosmosDbContext;
    public AtlasController(SqlDbContext sqlDbContext, CosmosDbContext cosmosDbContext)
    {
        this.sqlDbContext = sqlDbContext;
        this.cosmosDbContext = cosmosDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAtlasForUser([FromQuery] string userId)
    {
        var atlases = await sqlDbContext.AtlasUsers
        .Include(au => au.Atlas)
        .Where(au => au.UserId == new Guid(userId))
        .Select(au => new { id = au.Atlas.Id, title = au.Atlas.Title })
        .ToListAsync();

        return Ok(atlases);

    }

    [HttpPost]
    public async Task<IActionResult> CreateAtlas([FromBody] CreateAtlasDto atlas)
    {
        var entity = new Atlas
        {
            Title = atlas.Title,
            Markers = new List<Marker>(),
            AtlasUsers = new List<AtlasUser> { new AtlasUser() { UserId = atlas.UserId, IsOwner = true } }
        };

        sqlDbContext.Add(entity);
        await sqlDbContext.SaveChangesAsync();
        return Ok(new { Id = entity.Id });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAtlas([FromBody] DeleteAtlasDto dto)
    {
        var atlasUser = await sqlDbContext
        .AtlasUsers
        .Include(au => au.Atlas)
        .FirstOrDefaultAsync(au => au.AtlasId == dto.AtlasId && au.UserId == dto.UserId);

        if (atlasUser is null) return NotFound();

        if (atlasUser.IsOwner == false) return Unauthorized();

        sqlDbContext.Atlases.Remove(atlasUser.Atlas);

        await sqlDbContext.SaveChangesAsync();

        var markerPhotosToRemove = await cosmosDbContext.MarkerPhotos
            .Where(mp => mp.AtlasId == dto.AtlasId).ToListAsync();

        cosmosDbContext.MarkerPhotos.RemoveRange(markerPhotosToRemove);

        await cosmosDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAtlas([FromBody] UpdateAtlasDto updateAtlas)
    {
        var atlas = await sqlDbContext.Atlases.FirstOrDefaultAsync(a => a.Id == updateAtlas.Id);

        if (atlas is null)
        {
            return NotFound();
        }

        atlas.Title = updateAtlas.Title;

        await sqlDbContext.SaveChangesAsync();

        return Ok(atlas);
    }

    [HttpPost("add-user")]
    public async Task<IActionResult> AddUserToAtlas([FromBody] AddUserToAtlasDto dto)
    {
        sqlDbContext.AtlasUsers.Add(new AtlasUser() { AtlasId = dto.AtlasId, UserId = dto.UserId, IsOwner = false });
        await sqlDbContext.SaveChangesAsync();

        return Ok();
    }
}