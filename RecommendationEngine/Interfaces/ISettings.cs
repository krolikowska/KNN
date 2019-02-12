using DataAccess;

namespace RecommendationEngine
{
    public interface ISettings
    {
        int Id { get; set; }
        int MinNumberOfBooksEachUserRated { get; set; }
        int NumOfBooksToRecommend { get; set; }
        int NumOfNeighbors { get; set; }

        int BookPopularityAmongUsers { get; }
        DistanceSimilarityEnum SimilarityDistance { get; set;}

        Parameter CreateParameterSetFromSettings();
    }
}