namespace cloud_atlas.Entities.Models
{
    public class AtlasUsers
    {
        public Guid AtlasId { get; set; }
        public Guid UserId { get; set; }
        public bool IsOwner { get; set; }
    }
}
