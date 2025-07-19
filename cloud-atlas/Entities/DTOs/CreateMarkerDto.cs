public class CreateMarkerDto
{
    public required string Title { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required Guid AtlasId { get; set; }
}