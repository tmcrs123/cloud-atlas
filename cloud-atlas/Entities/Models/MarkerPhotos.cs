namespace cloud_atlas.Entities.Models
{
    public class MarkerPhotos
    {
        public Guid Id { get; set; }
        public Guid MapId { get; set; }
        public Guid MarkerId { get; set; }
        public List<Photo> Photos { get; set; }
    }
}
