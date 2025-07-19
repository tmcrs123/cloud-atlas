using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MarkerController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    private readonly IAmazonS3 S3Client;
    private readonly IDynamoDBContext DynamoDBContext;
    public MarkerController(SqlDbContext sqlDbContext, IDynamoDBContext dynamoDBContext, IAmazonS3 s3Client)
    {
        this.sqlDbContext = sqlDbContext;
        DynamoDBContext = dynamoDBContext;
        S3Client = s3Client;
    }

    [HttpGet]
    public async Task<IActionResult> GetMarkersForAtlas([FromQuery] Guid atlasId)
    {
        var IsOwner = await sqlDbContext.UserOwnsMap(atlasId);

        if (!IsOwner) return Unauthorized();

        var markers = await sqlDbContext.Markers
        .Where(m => m.AtlasId == atlasId)
        .ToListAsync();

        return Ok(markers);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMarkers([FromBody] List<CreateMarkerDto> markers)
    {
        // are all markers for the same atlas?
        if (markers == null || markers.Count == 0)
        {
            return BadRequest("No markers provided.");
        }

        var firstAtlasId = markers[0].AtlasId;
        if (markers.Any(m => m.AtlasId != firstAtlasId))
        {
            return BadRequest("All markers must have the same AtlasId.");
        }

        var entities = markers.Select(m => new Marker
        {
            Title = m.Title,
            AtlasId = m.AtlasId,
            Latitude = m.Latitude,
            Longitude = m.Longitude,
            Journal = string.Empty
        }).ToList();

        sqlDbContext.Markers.AddRange(entities);

        await sqlDbContext.SaveChangesAsync();

        return Ok(entities.Select(e => new { Id = e.Id }));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteMarker([FromBody] DeleteMarkerDto dto)
    {
        var marker = await sqlDbContext.Markers.FirstOrDefaultAsync(m => m.Id == dto.MarkerId);

        if (marker is null)
        {
            return Ok();
        }

        sqlDbContext.Markers.Remove(marker);

        await sqlDbContext.SaveChangesAsync();

        var response = await DynamoDBContext.LoadAsync<MarkerPhotos>(
           dto.AtlasId.ToString(),
           dto.MarkerId.ToString()
       );

        if (response is null)
        {
            return Ok();
        }

        await DynamoDBContext.DeleteAsync(response);

        var bucketName = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["AWS:BucketName"];

        var listRequest = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = $"{dto.AtlasId}/{dto.MarkerId}/"
        };

        var listResponse = await S3Client.ListObjectsV2Async(listRequest);

        if (listResponse.S3Objects.Any())
        {
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = listResponse.S3Objects
                    .Select(o => new KeyVersion { Key = o.Key })
                    .ToList()
            };

            await S3Client.DeleteObjectsAsync(deleteObjectsRequest);
        }

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMarker([FromBody] UpdateMarkerDto updateMarker)
    {
        var marker = await sqlDbContext.Markers.FirstOrDefaultAsync(a => a.Id == updateMarker.MarkerId);

        if (marker is null)
        {
            return NotFound();
        }

        marker.Title = updateMarker.Title ?? marker.Title;
        marker.Latitude = updateMarker.Latitude ?? marker.Latitude;
        marker.Longitude = updateMarker.Longitude ?? marker.Longitude;
        marker.Journal = updateMarker.Journal ?? marker.Journal;

        await sqlDbContext.SaveChangesAsync();

        return Ok(marker);
    }
}