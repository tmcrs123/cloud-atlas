namespace cloud_atlas.Entities.Models
{
    public class MarkerPhotos
    {
        public Guid Id { get; set; }
        public Guid AtlasId { get; set; }
        public Guid MarkerId { get; set; }
        public List<PhotoData> Photos { get; set; }
    }
}
