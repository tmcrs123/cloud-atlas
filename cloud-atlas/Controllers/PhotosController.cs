using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PhotosController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    private readonly CosmosDbContext cosmosDbContext;
    public PhotosController(CosmosDbContext cosmosDbContext, SqlDbContext sqlDbContext)
    {
        this.cosmosDbContext = cosmosDbContext;
        this.sqlDbContext = sqlDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> SavePhotos([FromBody] SavePhotosDto dto)
    {
        var photoLinkId = await sqlDbContext.PhotoLinks.FirstOrDefaultAsync(pl => pl.MarkerId == dto.MarkerId);

        if (photoLinkId is null) return NotFound();

        //does this marker already have photos?
        var existingMarker = await cosmosDbContext.MarkerPhotos.FirstOrDefaultAsync(mp => mp.MarkerId == dto.MarkerId);

        if (existingMarker is null)
        {
            MarkerPhotos markerPhotos = new MarkerPhotos()
            {
                AtlasId = dto.AtlasId,
                MarkerId = dto.MarkerId,
                Photos = dto.PhotosData,
                PhotoLinkId = photoLinkId.PhotoLinkId
            };

            cosmosDbContext.MarkerPhotos.AddRange(markerPhotos);
        }
        else
        {
            existingMarker.Photos.AddRange(dto.PhotosData);
        }

        await cosmosDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePhoto([FromBody] UpdatePhotoDto dto)
    {
        var photoLink = await cosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.PhotoLinkId == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        var photo = photoLink.Photos.FirstOrDefault(p => p.Id == dto.PhotoData.Id);

        if (photo == null)
            return NotFound();

        photo.Legend = dto.PhotoData.Legend;

        await cosmosDbContext.SaveChangesAsync();

        return Ok(photo);
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoDto dto)
    {
        var photoLink = await cosmosDbContext.MarkerPhotos
            .FirstOrDefaultAsync(pl => pl.PhotoLinkId == dto.PhotoLinkId);

        if (photoLink == null)
            return NotFound();

        photoLink.Photos = photoLink.Photos.Where(p => p.Id != dto.PhotoId).ToList();

        await cosmosDbContext.SaveChangesAsync();

        return Ok();
    }
}