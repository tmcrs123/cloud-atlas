public class CreateMarkerDto
{
    public required string Title { get; set; }
    public Coordinates Coordinates { get; set; }
    public required Guid AtlasId { get; set; }
}