using cloud_atlas.Entities.Models;

public class UpdatePhotoDto
{
    public Guid PhotoLinkId { get; set; }
    public PhotoData PhotoData { get; set; }
}