using Amazon.S3;
using Amazon.S3.Model;
using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PhotosController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    private readonly CosmosDbContext cosmosDbContext;
    private readonly IAmazonS3 s3Client;

    public PhotosController(CosmosDbContext cosmosDbContext, SqlDbContext sqlDbContext, IAmazonS3 s3Client)
    {
        this.cosmosDbContext = cosmosDbContext;
        this.sqlDbContext = sqlDbContext;
        this.s3Client = s3Client;
    }

    [HttpGet]
    public async Task<IActionResult> GetPhotosForMarker([FromQuery] Guid markerId)
    {
        var photos = await cosmosDbContext.MarkerPhotos.AsNoTracking().Where(mp => mp.MarkerId == markerId).Select(mp => mp.Photos).ToListAsync();
        return Ok(photos);
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

    [HttpPost("delete-photo-from-bucket")]
    public async Task<IActionResult> DeletePhotoFromBucket([FromBody] string key)
    {
        try
        {
            var bucketName = this.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["AWS:BucketName"];

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await s3Client.DeleteObjectAsync(deleteRequest);

            return Ok($"Deleted '{key}' from bucket '{bucketName}'.");
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e);
            throw;
        }
    }
}