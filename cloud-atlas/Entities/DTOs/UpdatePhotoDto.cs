using cloud_atlas.Entities.Models;

public class UpdatePhotoDto
{
    public Guid PhotoLinkId { get; set; }
    public PhotoData PhotoData { get; set; }
}

public class UpdatePhotoDto2
{
    public Guid AtlasId { get; set; }
    public Guid MarkerId { get; set; }
    public PhotoData PhotoData { get; set; }
}