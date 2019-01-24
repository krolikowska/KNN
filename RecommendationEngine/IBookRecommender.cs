using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IBookRecommender
    {
        BookScore[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId);

        BookScore[] PredictScoreForAllUsersBooks(List<UsersSimilarity> similarUsers, int userId);
    }
}