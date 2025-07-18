using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AtlasController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    private readonly IAmazonS3 S3Client;
    private readonly IDynamoDBContext DynamoDBContext;
    public AtlasController(SqlDbContext sqlDbContext, IAmazonS3 s3Client, IDynamoDBContext dynamoDBContext)
    {
        this.sqlDbContext = sqlDbContext;
        S3Client = s3Client;
        DynamoDBContext = dynamoDBContext;
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

        var response =  await DynamoDBContext.QueryAsync<MarkerPhotos>(dto.AtlasId.ToString()).GetRemainingAsync();

        if (response is null)
        {
            return NotFound();
        }

        foreach (var item in response)
        {
            await DynamoDBContext.DeleteAsync(item);
        }

        var bucketName = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["AWS:BucketName"];

        var listRequest = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = $"{dto.AtlasId}"
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
    public async Task<IActionResult> UpdateAtlas([FromBody] UpdateAtlasDto updateAtlas)
    {
        var IsOwner = await sqlDbContext.UserOwnsMap(updateAtlas.Id);

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
        var IsOwner = await sqlDbContext.UserOwnsMap(dto.AtlasId);

        if (!IsOwner) return Unauthorized();

        sqlDbContext.AtlasUsers.Add(new AtlasUser() { AtlasId = dto.AtlasId, UserId = dto.UserId, IsOwner = false });
        await sqlDbContext.SaveChangesAsync();

        return Ok();
    }
}