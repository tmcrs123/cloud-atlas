using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetAtlasForUser()
    {
        var atlases = await sqlDbContext.AtlasUsers
        .Include(au => au.Atlas)
        .Where(au => au.UserId == new Guid(HttpContext.Items["sub"] as string))
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
            AtlasUsers = new List<AtlasUser> { new AtlasUser() { UserId = new Guid(HttpContext.Items["sub"] as string), IsOwner = true } }
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
        .FirstOrDefaultAsync(au => au.AtlasId == dto.AtlasId && au.UserId == new Guid(HttpContext.Items["sub"] as string));

        if (atlasUser is null || atlasUser.IsOwner == false) return Unauthorized();

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
        var IsOwner = await IsUserOwnerOfAtlas(updateAtlas.Id);

        if (!IsOwner) return Unauthorized();

        var atlas = await sqlDbContext.Atlases.FirstOrDefaultAsync(a => a.Id == updateAtlas.Id);

        if (atlas is null)
        {
            return NotFound();
        }

        atlas.Title = updateAtlas.Title;

        await sqlDbContext.SaveChangesAsync();

        return Ok(atlas);
    }

    [HttpPost("share")]
    public async Task<IActionResult> AddUserToAtlas([FromBody] AddUserToAtlasDto dto)
    {
        var IsOwner = await IsUserOwnerOfAtlas(dto.AtlasId);

        if (!IsOwner) return Unauthorized();

        var atlasUser = await sqlDbContext
        .AtlasUsers
        .Include(au => au.Atlas)
        .FirstOrDefaultAsync(au => au.AtlasId == dto.AtlasId && au.UserId == new Guid(HttpContext.Items["sub"] as string));

        if (atlasUser is null) return NotFound();

        if (atlasUser.IsOwner == false) return Unauthorized();

        sqlDbContext.AtlasUsers.Add(new AtlasUser() { AtlasId = dto.AtlasId, UserId = dto.UserId, IsOwner = false });
        await sqlDbContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<bool> IsUserOwnerOfAtlas(Guid atlasId)
    {
        var atlasUser = await sqlDbContext
        .AtlasUsers
        .Include(au => au.Atlas)
        .FirstOrDefaultAsync(au => au.AtlasId == atlasId && au.UserId == new Guid(HttpContext.Items["sub"] as string));

        if (atlasUser.IsOwner == false) return false;
        return true;
    }
}