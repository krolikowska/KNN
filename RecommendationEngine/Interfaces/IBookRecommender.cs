using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IBookRecommender
    {
        Book[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId);
        Book[] GetRecommendedBooksFromDatabase(int userId);
        List<BookScore> PredictScoreForAllUsersBooks(List<UsersSimilarity> usersSimilarity, int userId);
    }
}