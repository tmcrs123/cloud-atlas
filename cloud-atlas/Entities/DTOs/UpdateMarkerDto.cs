public class UpdateMarkerDto
{
    public Guid MarkerId { get; set; }
    public string? Title { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Journal { get; set; }
}