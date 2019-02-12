using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;

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
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId, users, settings);
            var scores = _recommender.PredictScoreForAllUsersBooks(similarUsers, userId);
            _helpers.PersistSimilarUsersInDb(similarUsers, settings.Id);
            _helpers.PersistTestResult(scores, settings.Id);

            return EvaluatePredictionsUsingRSME(scores);
        }

        public (double mae, double rsme) EvaluateScoreForUserWithErrors(int userId, ISettings settings,
            List<int> users)
        {
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId, users, settings);
            var scores = _recommender.PredictScoreForAllUsersBooks(similarUsers, userId);
            if (scores.Count == 0) return (10, 10);

            var mae = EvaluatePredictionsUsingMAE(scores);
            var rsme = EvaluatePredictionsUsingRSME(scores);

            return (mae, rsme);
        }

        private double EvaluatePredictionsUsingMAE(List<BookScore> scores)
        {
            var actualRates = scores.Select(s => (int) s.Rate).ToArray();
            var predictedRates = scores.Select(s => s.PredictedRate).ToArray();

            return ComputeMeanAbsoluteError(predictedRates, actualRates);
        }

        private double EvaluatePredictionsUsingRSME(List<BookScore> scores)
        {
            var actualRates = scores.Select(s => (int) s.Rate).ToArray();
            var predictedRates = scores.Select(s => s.PredictedRate).ToArray();

            return ComputeRootMeanSquareError(predictedRates, actualRates);
        }

        private double ComputeMeanAbsoluteError(double[] predictedRates, int[] actualRates)
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
            return Math.Round(result, 4);
        }

        private double ComputeRootMeanSquareError(double[] predictedRates, int[] actualRates)
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
            return Math.Round(Math.Sqrt(squareError), 4);
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