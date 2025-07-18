public class CreatePresignedURLDto
{
    public Guid AtlasId { get; set; }
    public Guid MarkerId { get; set; }
    public required string Filename { get; set; }
}