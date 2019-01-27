using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface INearestNeighborsSearch
    {
        List<UsersSimilarity> GetNearestNeighbors(int userId, List<int> usersToCompare);
    }
}