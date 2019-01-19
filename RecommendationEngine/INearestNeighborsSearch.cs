using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface INearestNeighborsSearch
    {
        UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserID);

        IComparer<UsersSimilarity> DetermineComparerFromDistanceType();

        double GetCosineDistance(BookScore[] user1rates, BookScore[] user2rates);

        List<UsersSimilarity> GetNearestNeighbors(int userId);

        List<UsersSimilarity> GetNearestNeighbors(int userId, List<int> usersToCompare);

        double GetPearsonCorrelationSimilarity(BookScore[] user1rates, BookScore[] user2rates);

        List<UsersSimilarity> GetNearestNeighborsFromDb(int userId, int settingsVersion);
    }
}