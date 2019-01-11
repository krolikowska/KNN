using DataAccess;
using System.Collections.Generic;

namespace RecommendationEngine
{
    public interface INearestNeighborsSearch
    {
        UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserID);

        IComparer<UsersSimilarity> DetermineComparerFromDistanceType();

        double GetCosineDistance(BookScore[] user1rates, BookScore[] user2rates);

        List<UsersSimilarity> GetNearestNeighbors(int userId);

        List<UsersSimilarity> GetNearestNeighbors(int userId, List<User> usersToCompare);

        double GetPearsonCorrelationSimilarity(BookScore[] user1rates, BookScore[] user2rates);
    }
}