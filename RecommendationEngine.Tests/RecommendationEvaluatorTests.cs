using System;
using Shouldly;
using Xunit;
using NSubstitute;

namespace RecommendationEngine.Tests
{
    public class RecommendationEvaluatorTests
    {
        private readonly INearestNeighborsSearch _nearestNeighbors;
        private readonly IBookRecommender _recommender;
        private readonly IUsersSelector _selector;
        private readonly CollaborativeFilteringHelpers _helpers;

        public RecommendationEvaluatorTests()
        {
            _nearestNeighbors = Substitute.For<INearestNeighborsSearch>();
            _recommender = Substitute.For<IBookRecommender>();
            _selector = Substitute.For<IUsersSelector>();
            _helpers = Substitute.For<CollaborativeFilteringHelpers>();

            _sut = new RecommendationEvaluator(_recommender, _nearestNeighbors,_helpers, _selector);
        }

        private readonly RecommendationEvaluator _sut;

        [Theory]
        [InlineData(new double[] {1, 2, 3}, new[] {1, 2, 3}, 0.0)]
        [InlineData(new double[] {1, 2, 3}, new[] {4, 5, 6}, 3.0)]
        [InlineData(new double[] {4, 5, 6}, new[] {1, 2, 3}, 3.0)]
        public void ComputeMeanAbsoluteError_ValidInput_ShouldComputeError(double[] predicted, int[] actual,
            double error)
        {
            var resut = _sut.ComputeMeanAbsoluteError(predicted, actual);
            resut.ShouldBe(error, 0.01);
        }

        [Theory]
        [InlineData(new double[] {1, 2, 3}, new[] {1, 2, 3}, 0.0)]
        [InlineData(new double[] {1, 2, 3}, new[] {4, 5, 6}, 3.0)]
        [InlineData(new double[] {4, 5, 6}, new[] {1, 2, 3}, 3.0)]
        public void ComputeRootMeanSquareError_ValidInput_ShouldComputeError(double[] predicted, int[] actual,
            double error)
        {
            var resut = _sut.ComputeRootMeanSquareError(predicted, actual);
            resut.ShouldBe(error, 0.01);
        }

        [Fact]
        public void ComputeMeanAbsoluteError_ArraysHaveDifferentLenghts_ShouldThrowException()
        {
            var predicted = new double[2];
            var actual = new int[3];

            Should.Throw<InvalidOperationException>(() =>
                                                        _sut
                                                            .ComputeMeanAbsoluteError(predicted, actual));
        }

        [Fact]
        public void ComputeMeanAbsoluteError_EmptyArray_ShouldThrowException()
        {
            var predicted = new double[] {1, 2, 3, 4};
            var actual = new int[0];

            Should.Throw<InvalidOperationException>(() =>
                                                        _sut
                                                            .ComputeMeanAbsoluteError(predicted, actual));
        }

        [Fact]
        public void ComputeRootMeanSquareError_ArraysHaveDifferentLenghts_ShouldThrowException()
        {
            var predicted = new double[2];
            var actual = new int[3];

            Should.Throw<InvalidOperationException>(() =>
                                                        _sut
                                                            .ComputeRootMeanSquareError(predicted, actual));
        }

        [Fact]
        public void ComputeRootMeanSquareError_EmptyArray_ShouldThrowException()
        {
            var predicted = new double[] {1, 2, 3, 4};
            var actual = new int[0];

            Should.Throw<InvalidOperationException>(() =>
                                                        _sut
                                                            .ComputeMeanAbsoluteError(predicted, actual));
        }
    }
}