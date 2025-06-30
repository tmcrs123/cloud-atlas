using cloud_atlas.Entities.Models;

public class SavePhotosDto
{
    public Guid AtlasId { get; set; }
    public Guid MarkerId { get; set; }
    public List<PhotoData> PhotosData { get; set; }
}