namespace cloud_atlas.Entities.Models
{
    // This model is only used for Cosmos
    public class MarkerPhotos
    {
        public Guid PhotoLinkId { get; set; }
        public Guid AtlasId { get; set; }
        public Guid MarkerId { get; set; }
        public List<PhotoData> Photos { get; set; }
    }
}
