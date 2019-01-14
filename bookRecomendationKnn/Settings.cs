using DataAccess;

namespace RecommendationEngine.Properties
{
    public sealed partial class Settings : ISettings
    {
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