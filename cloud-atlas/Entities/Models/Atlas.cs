namespace cloud_atlas.Entities.Models
{
    public class Atlas
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public List<Marker> Markers { get; set; }
        public List<AtlasUser> AtlasUsers { get; set; }
    }
}
