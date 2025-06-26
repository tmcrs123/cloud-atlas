namespace cloud_atlas.Entities.Models
{
    public class PhotoDetails
    {
        public Guid Id { get; set; }
        public required string URLs { get; set; }
        public required Marker Marker { get; set; }
    }
}
