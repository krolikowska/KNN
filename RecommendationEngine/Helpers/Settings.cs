using System;
using System.Configuration;
using DataAccess;

namespace RecommendationEngine
{
    public class Settings : ISettings
    {
        public int BookPopularityAmongUsers => int.Parse(ConfigurationManager.AppSettings["BookPopularityAmongUsers"]);

        public int Id { get; set; } = int.Parse(ConfigurationManager.AppSettings["Id"]);

        public int MinNumberOfBooksEachUserRated { get; set; } =
            int.Parse(ConfigurationManager.AppSettings["MinNumberOfBooksEachUserRated"]);

        public int NumOfBooksToRecommend { get; set; } =
            int.Parse(ConfigurationManager.AppSettings["NumOfBooksToRecommend"]);

        public int NumOfNeighbors { get; set; } = int.Parse(ConfigurationManager.AppSettings["NumOfNeighbors"]);

        public DistanceSimilarityEnum SimilarityDistance { get; set; } =
            (DistanceSimilarityEnum) Enum.Parse(typeof(DistanceSimilarityEnum),
                                                ConfigurationManager.AppSettings["SimilarityDistance"]);

        public Parameter CreateParameterSetFromSettings()
        {
            return new Parameter
            {
                BookPopularity = BookPopularityAmongUsers,
                Kneigbors = NumOfNeighbors,
                NumberOfBooksEachUserRated = MinNumberOfBooksEachUserRated,
                DistanceType = (int) SimilarityDistance,
                Id = Id
            };
        }
    }
}