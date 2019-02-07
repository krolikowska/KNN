using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecommendationEngine;

namespace RecommendationApi.Models
{
    public class NeighborsModel
    {
        public int UserId { get; set; }
        public int NeighborId { get; set; }
        public double? ComputedSimilarity { get; set; }

        public DistanceSimilarityEnum SimilarityType { get; set; }
        public int SettingsId { get; set; }
    }
}