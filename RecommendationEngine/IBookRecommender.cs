using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IBookRecommender
    {
        BookScore[] GetRecommendedBooksWithScores(List<UsersSimilarity> similarUsers, int userId);
        Book[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId);
        List<BookScore> PredictScoreForAllUsersBooks(List<UsersSimilarity> usersSimilarity, int userId);
    }
}