using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;

public class PhotosController : BaseController
{
    private readonly CosmosDbContext CosmosDbContext;
    public PhotosController(CosmosDbContext cosmosDbContext)
    {
        CosmosDbContext = cosmosDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> SavePhoto([FromBody] SavePhotosDto dto)
    {
        MarkerPhotos markerPhotos = new MarkerPhotos()
        {
            AtlasId = dto.AtlasId,
            MarkerId = dto.MarkerId,
            Photos = dto.PhotosData
        };

        CosmosDbContext.MarkerPhotos.AddRange(markerPhotos);

        await CosmosDbContext.SaveChangesAsync();

        return Ok(markerPhotos);
    }
}