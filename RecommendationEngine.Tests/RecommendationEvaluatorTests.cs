using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
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
        private readonly CollaborativeFilteringHelpers _cfHelpers;
        private readonly TestHelpers _testHelpers;
        private readonly TestSettings _settings;

        public RecommendationEvaluatorTests()
        {
            _nearestNeighbors = Substitute.For<INearestNeighborsSearch>();
            _recommender = Substitute.For<IBookRecommender>();
            _selector = Substitute.For<IUsersSelector>();
           // _cfHelpers = Substitute.For<CollaborativeFilteringHelpers>();
            _settings = new TestSettings
            {
                Id = 1
            };

            _sut = new RecommendationEvaluator(_recommender, _nearestNeighbors,_cfHelpers, _selector);
        }

        private readonly RecommendationEvaluator _sut;

        [Theory]
        [InlineData(new double[] { 1, 2, 3 }, new short[] { 1, 2, 3 }, 0.0)]
        [InlineData(new double[] { 1, 2, 3 }, new short[] { 4, 5, 6 }, 3.0)]
        [InlineData(new double[] { 4, 5, 6 }, new short[] { 1, 2, 3 }, 3.0)]
        public void EvaluateScoreForUSer_ValidInput_ShouldComputeError(double[] predicted, short[] actual,
            double error)
        {
            int userId = 1;
            var scores = _testHelpers.CreateBookScore(userId, new[] {"1", "2", "3"}, actual, predicted).ToList();
            _recommender.PredictScoreForAllUsersBooks(new List<UsersSimilarity>(),userId).Returns(scores);

            _sut.EvaluateScoreForUSer(1, _settings).ShouldBe(error);

        //    _cfHelpers.Received().PersistSimilarUsersInDb(neighbors,settingsId);
        //    _cfHelpers.Received().PersistTestResult(books,settingsId);
        }

        [Fact]
        public void EvaluateScoreForUserWithErrors_()
        {

        }

        //[Theory]
        //[InlineData(new double[] {1, 2, 3}, new[] {1, 2, 3}, 0.0)]
        //[InlineData(new double[] {1, 2, 3}, new[] {4, 5, 6}, 3.0)]
        //[InlineData(new double[] {4, 5, 6}, new[] {1, 2, 3}, 3.0)]
        //public void ComputeMeanAbsoluteError_ValidInput_ShouldComputeError(double[] predicted, int[] actual,
        //    double error)
        //{
        //    var resut = _sut.ComputeMeanAbsoluteError(predicted, actual);
        //    resut.ShouldBe(error, 0.01);
        //}

        //[Theory]
        //[InlineData(new double[] {1, 2, 3}, new[] {1, 2, 3}, 0.0)]
        //[InlineData(new double[] {1, 2, 3}, new[] {4, 5, 6}, 3.0)]
        //[InlineData(new double[] {4, 5, 6}, new[] {1, 2, 3}, 3.0)]
        //public void ComputeRootMeanSquareError_ValidInput_ShouldComputeError(double[] predicted, int[] actual,
        //    double error)
        //{
        //    var resut = _sut.ComputeRootMeanSquareError(predicted, actual);
        //    resut.ShouldBe(error, 0.01);
        //}

        //[Fact]
        //public void ComputeMeanAbsoluteError_ArraysHaveDifferentLenghts_ShouldThrowException()
        //{
        //    var predicted = new double[2];
        //    var actual = new int[3];

        //    Should.Throw<InvalidOperationException>(() =>
        //                                                _sut
        //                                                    .ComputeMeanAbsoluteError(predicted, actual));
        //}

        //[Fact]
        //public void ComputeMeanAbsoluteError_EmptyArray_ShouldThrowException()
        //{
        //    var predicted = new double[] {1, 2, 3, 4};
        //    var actual = new int[0];

        //    Should.Throw<InvalidOperationException>(() =>
        //                                                _sut
        //                                                    .ComputeMeanAbsoluteError(predicted, actual));
        //}

        //[Fact]
        //public void ComputeRootMeanSquareError_ArraysHaveDifferentLenghts_ShouldThrowException()
        //{
        //    var predicted = new double[2];
        //    var actual = new int[3];

        //    Should.Throw<InvalidOperationException>(() =>
        //                                                _sut
        //                                                    .ComputeRootMeanSquareError(predicted, actual));
        //}

        //[Fact]
        //public void ComputeRootMeanSquareError_EmptyArray_ShouldThrowException()
        //{
        //    var predicted = new double[] {1, 2, 3, 4};
        //    var actual = new int[0];

        //    Should.Throw<InvalidOperationException>(() =>
        //                                                _sut
        //                                                    .ComputeMeanAbsoluteError(predicted, actual));
        //}
    }
}