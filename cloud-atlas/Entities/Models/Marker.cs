namespace cloud_atlas.Entities.Models
{
    public class Marker
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required Atlas Atlas { get; set; }
        public MarkerPhotosLink PhotosLink { get; set; }
    }
}
