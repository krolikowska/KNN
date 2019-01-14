using System;
using Shouldly;
using Xunit;

namespace RecommendationEngine.Tests
{
    public class RecommendationEvaluatorTests
    {
        public RecommendationEvaluatorTests()
        {
            _recommendationEvaluator = new RecommendationEvaluator();
        }

        private readonly RecommendationEvaluator _recommendationEvaluator;

        [Theory]
        [InlineData(new double[] {1, 2, 3}, new[] {1, 2, 3}, 0.0)]
        [InlineData(new double[] {1, 2, 3}, new[] {4, 5, 6}, 3.0)]
        [InlineData(new double[] {4, 5, 6}, new[] {1, 2, 3}, 3.0)]
        public void ComputeMeanAbsoluteError_ValidInput_ShouldComputeError(double[] predicted, int[] actual,
            double error)
        {
            var resut = _recommendationEvaluator.ComputeMeanAbsoluteError(predicted, actual);
            resut.ShouldBe(error, 0.02);
        }

        [Theory]
        [InlineData(new double[] {1, 2, 3}, new[] {1, 2, 3}, 0.0)]
        [InlineData(new double[] {1, 2, 3}, new[] {4, 5, 6}, 3.0)]
        [InlineData(new double[] {4, 5, 6}, new[] {1, 2, 3}, 3.0)]
        public void ComputeRootMeanSquareError_ValidInput_ShouldComputeError(double[] predicted, int[] actual,
            double error)
        {
            var resut = _recommendationEvaluator.ComputeRootMeanSquareError(predicted, actual);
            resut.ShouldBe(error, 0.02);
        }

        [Fact]
        public void ComputeMeanAbsoluteError_ArraysHaveDifferentLenghts_ShouldThrowException()
        {
            var predicted = new double[2];
            var actual = new int[3];

            Should.Throw<InvalidOperationException>(() =>
                                                        _recommendationEvaluator
                                                            .ComputeMeanAbsoluteError(predicted, actual));
        }

        [Fact]
        public void ComputeMeanAbsoluteError_EmptyArray_ShouldThrowException()
        {
            var predicted = new double[] {1, 2, 3, 4};
            var actual = new int[0];

            Should.Throw<InvalidOperationException>(() =>
                                                        _recommendationEvaluator
                                                            .ComputeMeanAbsoluteError(predicted, actual));
        }

        [Fact]
        public void ComputeRootMeanSquareError_ArraysHaveDifferentLenghts_ShouldThrowException()
        {
            var predicted = new double[2];
            var actual = new int[3];

            Should.Throw<InvalidOperationException>(() =>
                                                        _recommendationEvaluator
                                                            .ComputeRootMeanSquareError(predicted, actual));
        }

        [Fact]
        public void ComputeRootMeanSquareError_EmptyArray_ShouldThrowException()
        {
            var predicted = new double[] {1, 2, 3, 4};
            var actual = new int[0];

            Should.Throw<InvalidOperationException>(() =>
                                                        _recommendationEvaluator
                                                            .ComputeMeanAbsoluteError(predicted, actual));
        }
    }
}