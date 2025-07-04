using cloud_atlas;
using cloud_atlas.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MarkerController : BaseController
{
    private readonly SqlDbContext sqlDbContext;
    private readonly CosmosDbContext cosmosDbContext;
    public MarkerController(SqlDbContext sqlDbContext, CosmosDbContext cosmosDbContext)
    {
        this.sqlDbContext = sqlDbContext;
        this.cosmosDbContext = cosmosDbContext;
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

    [HttpGet("photo-link")]
    public async Task<IActionResult> GetPhotoLinkForMarker([FromQuery] Guid markerId)
    {
        var link = await sqlDbContext.PhotoLinks
        .Where(m => m.MarkerId == markerId)
        .Select(pl => pl.PhotoLinkId)
        .FirstOrDefaultAsync();

        return Ok(link);
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
            Latitude = m.Coordinates.Latitude,
            Longitude = m.Coordinates.Longitude,
            MarkerPhotosLink = new MarkerPhotosLink()
        }).ToList();

        sqlDbContext.Markers.AddRange(entities);

        await sqlDbContext.SaveChangesAsync();

        return Ok(entities.Select(e => new { Id = e.Id, PhotosLink = e.MarkerPhotosLink.PhotoLinkId }));
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

        var markerToRemove = await cosmosDbContext.MarkerPhotos.FirstOrDefaultAsync(mp => mp.MarkerId == dto.MarkerId);

        if (markerToRemove is null) return Ok();

        cosmosDbContext.MarkerPhotos.Remove(markerToRemove);

        await cosmosDbContext.SaveChangesAsync();

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

        await sqlDbContext.SaveChangesAsync();

        return Ok(marker);
    }
}