using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecommendationEngine.Tests
{
    public class TestHelpers
    {
        public BookScore[] CreateBookScores(int userId, string[] isbn, short[] rates, double[] predictedRates = null)
        {
            var results = new BookScore[isbn.Length];
            for (int i = 0; i < isbn.Length; i++)
            {
                var predictedRate = predictedRates == null ? RandomNumber(1) : predictedRates[i];
                results[i] = new BookScore
                {
                    UserId = userId,
                    BookId = isbn[i],
                    Rate = rates[i],
                    PredictedRate = predictedRate
                };
            }

            return results;
        }

        public List<UsersSimilarity> CreateUserSimilarities(ISettings settings,
            int userId,
            List<int> comparedUserId,
            BookScore[] userBooks,
            List<BookScore[]> comparedUserBooks,
            List<double?> similarity = null)
        {
            var userSimilarities = new List<UsersSimilarity>();
            var comparer = new BookScoreEqualityComparer();
            for (int i = 0; i < comparedUserId.Count; i++)
            {
                var comparedUserRatesForMutualBooks = comparedUserBooks[i].Intersect(userBooks, comparer).ToArray();
                var userRatesForMutualBooks = userBooks.Intersect(comparedUserBooks[i], comparer).ToArray();
                var booksUniqueForComparedUser = comparedUserBooks[i].Except(userBooks, comparer).ToArray();
                var temp = new UsersSimilarity
                {
                    AverageScoreForComparedUser = RandomNumber(3),
                    ComparedUserId = comparedUserId[i],
                    UserId = userId,
                    Similarity = similarity?[i],
                    SimilarityType = (int) settings.SimilarityDistance,
                    ComparedUserRatesForMutualBooks = comparedUserRatesForMutualBooks,
                    BooksUniqueForComparedUser = booksUniqueForComparedUser,
                    UserRatesForMutualBooks = userRatesForMutualBooks
                };
                userSimilarities.Add(temp);
            }

            return userSimilarities;
        }

        public double RandomNumber(int seed)
        {
            var random = new Random(seed);
            return random.Next(1, 10);
        }
    }
}