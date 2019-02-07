using System.Collections.Generic;
using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    public interface IUserBasedCollaborativeFiltering
    {
        Book[] RecommendBooksForUser(int userId);
        List<UsersSimilarity> InvokeNearestNeighbors(int userId, int numberOfBooks);
    }
}