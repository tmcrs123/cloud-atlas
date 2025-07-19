using Amazon.DynamoDBv2.DataModel;

namespace cloud_atlas.Entities.Models
{
    [DynamoDBTable("photos")]
    public class MarkerPhotos
    {
        [DynamoDBHashKey("atlasId")]
        public string AtlasId { get; set; }

        [DynamoDBRangeKey("markerId")]
        public string MarkerId { get; set; }

        [DynamoDBProperty("photos")]
        public List<PhotoData> Photos { get; set; }
    }
}
