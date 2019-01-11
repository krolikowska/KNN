using DataAccess;

namespace RecommendationEngine.Properties
{
    public interface ISettings
    {
        int BookPopularityAmongUsers { get; }
        int Id { get; }
        int MinNumberOfBooksEachUserRated { get; }
        int NumOfBooksToRecommend { get; }
        int NumOfNeighbors { get; }
        DistanceSimilarityEnum SimilarityDistance { get; }

        Parameter CreateParameterSetFromSettings();
    }
}