using DataAccess;
using System.Collections.Generic;

namespace RecommendationEngine
{
    public interface IBookRecommender
    {
        BookScore[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId);

        SortedSet<BookScore> PredictScore(List<UsersSimilarity> similarUsers, int userId, string[] booksIds);
    }
}