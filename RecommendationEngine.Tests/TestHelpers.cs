using DataAccess;
using System;

namespace RecommendationEngine.Tests
{
    public class TestHelpers
    {
        public BookScore[] CreateBookScore(int userId, string[] isbn, short[] rates, double[] predictedRates = null)
        {
            var results = new BookScore[isbn.Length];
            for (int i = 0; i < isbn.Length; i++)
            {
                results[i] = new BookScore
                {
                    UserId = userId,
                    BookId = isbn[i],
                    Rate = rates[i],
                    PredictedRate = predictedRates == null ? RandomNumber(1) : predictedRates[i] 
                };
            }

            return results;
        }

        public double RandomNumber(int seed)
        {
            var random = new Random(seed);
            return random.Next(1, 10);
        }
    }
}