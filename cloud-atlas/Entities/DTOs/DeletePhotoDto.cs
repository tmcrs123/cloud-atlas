using cloud_atlas.Entities.Models;

public class DeletePhotoDto
{
    public Guid AtlasId { get; set; }
    public Guid MarkerId { get; set; }
    public string PhotoId { get; set; }
}