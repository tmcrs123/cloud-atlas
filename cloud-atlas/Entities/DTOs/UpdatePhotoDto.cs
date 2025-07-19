using cloud_atlas.Entities.Models;

public class UpdatePhotoDto
{
    public Guid AtlasId { get; set; }
    public Guid MarkerId { get; set; }
    public PhotoData PhotoData { get; set; }
}