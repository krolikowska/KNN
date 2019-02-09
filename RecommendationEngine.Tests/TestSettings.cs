using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace RecommendationEngine.Tests
{
    public class TestSettings : ISettings
    {
        public int BookPopularityAmongUsers { get; set; }
        public int Id { get; set; }
        public int MinNumberOfBooksEachUserRated { get; set; }
        public int NumOfBooksToRecommend { get; set; }
        public int NumOfNeighbors { get; set; }
        public DistanceSimilarityEnum SimilarityDistance { get; set; }
        public Parameter CreateParameterSetFromSettings()
        {
            throw new NotImplementedException();
        }
    }
}
