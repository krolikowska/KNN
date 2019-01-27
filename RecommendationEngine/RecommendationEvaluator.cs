using System;
using System.Linq;
using DataAccess;

namespace RecommendationEngine
{
    public class RecommendationEvaluator
    {
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

            return numerator / n;
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
            return Math.Sqrt(squareError);
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