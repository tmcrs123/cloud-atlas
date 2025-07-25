﻿namespace cloud_atlas.Entities.Models
{
    public class Marker
    {
        public Guid Id { get; set; }
        public Guid AtlasId { get; set; }
        public string Journal { get; set; }
        public required string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Atlas Atlas { get; set; }
    }
}
