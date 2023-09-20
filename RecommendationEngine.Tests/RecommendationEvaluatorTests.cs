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
        private readonly ICollaborativeFilteringHelpers _cfHelpers;
        private readonly TestHelpers _testHelpers;
        private readonly TestSettings _settings;

        public RecommendationEvaluatorTests()
        {
            _nearestNeighbors = Substitute.For<INearestNeighborsSearch>();
            _recommender = Substitute.For<IBookRecommender>();
            _selector = Substitute.For<IUsersSelector>();
            _testHelpers = new TestHelpers();
            _cfHelpers = Substitute.For<ICollaborativeFilteringHelpers>();
            _settings = new TestSettings
            {
                Id = 1
            };

            _sut = new RecommendationEvaluator(_recommender, _nearestNeighbors, _cfHelpers, _selector);
        }
//test comment
        private readonly RecommendationEvaluator _sut;

        [Theory]
        [InlineData(new double[] { 1, 2, 3 }, new short[] { 1, 2, 3 }, 0.0)]
        [InlineData(new double[] { 1, 2, 3 }, new short[] { 4, 5, 6 }, 3.0)]
        [InlineData(new double[] { 4, 5, 6 }, new short[] { 1, 2, 3 }, 3.0)]
        public void EvaluateScoreForUSer_ShouldComputeError(double[] predicted, short[] actual,
            double error)
        {
            const int userId = 1;
            var scores = _testHelpers.CreateBookScores(userId, new[] { "1", "2", "3" }, actual, predicted).ToList();
            var users = new List<int> { };
            var similarUsers = new List<UsersSimilarity>();
//second test commit
            _selector.SelectUsersIdsToCompareWith(userId).Returns(users);
            _nearestNeighbors.GetNearestNeighbors(userId, users, _settings).Returns(similarUsers);
            _recommender.PredictScoreForAllUsersBooks(similarUsers, userId).Returns(scores);

            _sut.EvaluateScoreForUSer(userId, _settings).ShouldBe(error);
            _cfHelpers.Received().PersistSimilarUsersInDb(similarUsers, _settings.Id);
            _cfHelpers.Received().PersistTestResult(scores, _settings.Id);
            _cfHelpers.Received().SaveSettings(_settings.Id);
        }

        [Theory]
        [InlineData(new double[] { 1, 2, 3 }, new short[] { 1, 2, 3 }, 0.0, 0.0)]
        [InlineData(new double[] { 1, 2, 3 }, new short[] { 4, 5, 6 }, 3.0, 3.0)]
        [InlineData(new double[] { 4, 5, 6 }, new short[] { 1, 2, 3 }, 3.0, 3.0)]
        public void EvaluateScoreForUserWithErrors_ShouldComputeError(double[] predicted, short[] actual,
            double errorRMSE, double errorMAE)
        {
            const int userId = 1;
            var scores = _testHelpers.CreateBookScores(userId, new[] { "1", "2", "3" }, actual, predicted).ToList();
            var users = new List<int> { };
            var similarUsers = new List<UsersSimilarity>();

            _selector.SelectUsersIdsToCompareWith(_settings.NumOfNeighbors).Returns(users);
            _nearestNeighbors.GetNearestNeighbors(userId, users, _settings).Returns(similarUsers);
            _recommender.PredictScoreForAllUsersBooks(similarUsers, userId).Returns(scores);

            var result = _sut.EvaluateScoreForUserWithErrors(userId, _settings, users);
            result.mae.ShouldBe(errorMAE, 0.01);
            result.rsme.ShouldBe(errorRMSE, 0.01);
        }
    }
}
