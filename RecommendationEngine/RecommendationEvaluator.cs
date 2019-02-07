using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    public class RecommendationEvaluator : IRecommendationEvaluator
    {
        private readonly INearestNeighborsSearch _nearestNeighbors;
        private readonly IBookRecommender _recommender;
        private readonly IUsersSelector _selector;
        private readonly CollaborativeFilteringHelpers _helpers;

        public RecommendationEvaluator(IBookRecommender recommender, INearestNeighborsSearch nearestNeighbors,
            CollaborativeFilteringHelpers helpers, IUsersSelector selector)
        {
            _recommender = recommender;
            _nearestNeighbors = nearestNeighbors;
            _helpers = helpers;
            _selector = selector;
        }

        public double EvaluateScoreForUSer(int userId, ISettings settings)
        {
            _helpers.SaveSettings(settings.Id);
            var users = _selector.GetUsersWhoRatedAtLeastNBooks(settings.MinNumberOfBooksEachUserRated);

            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId, users);
            _helpers.PersistSimilarUsersInDb(similarUsers, settings.Id);

            var scores = _recommender.PredictScoreForAllUsersBooks(similarUsers, userId);
            _helpers.PersistTestResult(scores, settings.Id);

            return EvaluatePredictionsUsingMAE(scores);
        }

        public double EvaluatePredictionsUsingMAE(List<BookScore> scores)
        {
            var actualRates = scores.Select(s => (int) s.Rate).ToArray();
            var predictedRates = scores.Select(s => s.PredictedRate).ToArray();

            return ComputeMeanAbsoluteError(predictedRates, actualRates);
        }

        public double EvaluatePredictionsUsingMAE(BookScore[] scores)
        {
            var actualRates = scores.Select(s => (int) s.Rate).ToArray();
            var predictedRates = scores.Select(s => s.PredictedRate).ToArray();

            return ComputeMeanAbsoluteError(predictedRates, actualRates);
        }

        public double EvaluatePredictionsUsingRSME(BookScore[] scores)
        {
            var actualRates = scores.Select(s => (int) s.Rate).ToArray();
            var predictedRates = scores.Select(s => s.PredictedRate).ToArray();

            return ComputeMeanAbsoluteError(predictedRates, actualRates);
        }

        public double ComputeMeanAbsoluteError(double[] predictedRates, int[] actualRates)
        {
            var n = predictedRates.Length;
            ValidateInput(actualRates.Length, n);

            var numerator = 0.0;

            for (var i = 0; i < predictedRates.Length; i++)
            {
                var temp = predictedRates[i] - actualRates[i];
                numerator += Math.Abs(temp);
            }

            var result = numerator / n;
            return Math.Round(result, 2);
        }

        public double ComputeRootMeanSquareError(double[] predictedRates, int[] actualRates)
        {
            var n = predictedRates.Length;
            ValidateInput(actualRates.Length, n);
            var numerator = 0.0;

            for (var i = 0; i < predictedRates.Length; i++)
            {
                var temp = (predictedRates[i] - actualRates[i]) * (predictedRates[i] - actualRates[i]);
                numerator += temp;
            }

            var squareError = numerator / n;
            return Math.Round(Math.Sqrt(squareError), 2);
        }

        private static void ValidateInput(int predictedRatesLenght, int actualRatesLenght)
        {
            if (predictedRatesLenght == 0 || actualRatesLenght == 0)
                throw new InvalidOperationException("Rates lenght must be bigger than zero");
            if (predictedRatesLenght != actualRatesLenght)
                throw new InvalidOperationException("Actual and predicted rates must have same lenght");
        }
    }
}