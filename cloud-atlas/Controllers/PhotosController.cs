using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PhotosController : BaseController
{
    private readonly CosmosDbContext CosmosDbContext;
    public PhotosController(CosmosDbContext cosmosDbContext)
    {
        CosmosDbContext = cosmosDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> SavePhotos([FromBody] SavePhotosDto dto)
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

    [HttpPut]
    public async Task<IActionResult> UpdatePhoto([FromBody] UpdatePhotoDto dto)
    {
        var photoLink = await CosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.Id == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        var photo = photoLink.Photos.FirstOrDefault(p => p.Id == dto.PhotoData.Id);

        if (photo == null)
            return NotFound();

        photo.Legend = dto.PhotoData.Legend;

        await CosmosDbContext.SaveChangesAsync();

        return Ok(photo);
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoDto dto)
    {
        var photoLink = await CosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.Id == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        photoLink.Photos = photoLink.Photos.Where(p => p.Id != dto.PhotoId).ToList();

        await CosmosDbContext.SaveChangesAsync();

        return Ok();
    }
}