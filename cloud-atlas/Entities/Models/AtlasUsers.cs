namespace cloud_atlas.Entities.Models
{
    public class AtlasUser
    {
        public Guid AtlasId { get; set; }
        public Atlas Atlas { get; set; }
        public Guid UserId { get; set; }
        public bool IsOwner { get; set; }
    }
}
