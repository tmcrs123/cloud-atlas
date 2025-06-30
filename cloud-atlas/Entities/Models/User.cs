namespace cloud_atlas.Entities.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public List<AtlasUser> AtlasUsers { get; set; }
    }
}
