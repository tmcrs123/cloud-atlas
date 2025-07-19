using Amazon.DynamoDBv2.DataModel;

namespace cloud_atlas.Entities.Models
{
    public class PhotoData
    {
        [DynamoDBProperty("legend")]
        public string Legend { get; set; }

        [DynamoDBProperty("id")]
        public string Id { get; set; }
    }
}
