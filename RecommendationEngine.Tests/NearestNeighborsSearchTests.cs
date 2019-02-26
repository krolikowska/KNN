using System.Collections.Generic;
using System.Linq;
using DataAccess;
using NSubstitute;
using Xunit;
using Shouldly;

namespace RecommendationEngine.Tests
{
    public class NearestNeighborsSearchTests
    {
        private readonly NearestNeighborsSearch _sut;
        private readonly IUsersSelector _usersSelector;
        private readonly TestSettings _settings;
        private readonly TestHelpers _helpers;

        public NearestNeighborsSearchTests()
        {
            _usersSelector = Substitute.For<IUsersSelector>();
            _helpers = new TestHelpers();
            _settings = new TestSettings
            {
                SimilarityDistance = DistanceSimilarityEnum.PearsonSimilarity,
                NumOfNeighbors = 2
            };
            _sut = new NearestNeighborsSearch(_usersSelector);
        }

        [Fact]
        public void GetNearestNeighbors_WhenUsersDontShareBooks_ShouldReturnNull()
        {
            var actual = _sut.GetNearestNeighbors(1, new List<int>() {2}, _settings);

            actual.ShouldBeNull();
        }

        [Fact]
        public void GetNearestNeighbors_WhenSimilarityIsNotNull_ShouldNotReturnNull()
        {
            var similarity = new UsersSimilarity
            {
                ComparedUserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {1, 2, 3}),
                UserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {1, 2, 3}),
            };

            _usersSelector.SelectMutualAndUniqueBooksForUsers(1, 2).Returns(similarity);

            var actual = _sut.GetNearestNeighbors(1, new List<int>() {2}, _settings);

            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(DistanceSimilarityEnum.PearsonSimilarity)]
        [InlineData(DistanceSimilarityEnum.CosineSimilarity)]
        public void GetNearestNeighbors_WhenBooksAreTheSAme_ShouldReturnMaxSimilarity(DistanceSimilarityEnum distance)
        {
            _settings.SimilarityDistance = distance;
            var similarity = new UsersSimilarity
            {
                ComparedUserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {1, 2, 3}),
                UserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {1, 2, 3}),
            };

            _usersSelector.SelectMutualAndUniqueBooksForUsers(1, 2).Returns(similarity);

            var actual = _sut.GetNearestNeighbors(1, new List<int>() {2}, _settings).FirstOrDefault();

            actual.ShouldNotBeNull();
            actual.Similarity.Value.ShouldBe(1, 2);
        }

        [Theory]
        [InlineData(DistanceSimilarityEnum.PearsonSimilarity, -1)]
        [InlineData(DistanceSimilarityEnum.CosineSimilarity, 0)]
        public void GetNearestNeighbors_WhenBookRatesAreDifferent_ShouldReturnMinSimilarity(
            DistanceSimilarityEnum distance, double expected)
        {
            _settings.SimilarityDistance = distance;
            var similarity = new UsersSimilarity
            {
                ComparedUserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {1, 1, 1}),
                UserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {10, 10, 10}),
            };

            _usersSelector.SelectMutualAndUniqueBooksForUsers(1, 2).Returns(similarity);

            var actual = _sut.GetNearestNeighbors(1, new List<int>() {2}, _settings).FirstOrDefault();

            actual.ShouldNotBeNull();
            actual.Similarity.Value.ShouldBe(expected, 2);
        }

        [Theory]
        [InlineData(DistanceSimilarityEnum.PearsonSimilarity)]
        [InlineData(DistanceSimilarityEnum.CosineSimilarity)]
        public void GetNearestNeighbors_ShouldReturnTwoMostSimilarUsers(DistanceSimilarityEnum distance)
        {
            _settings.SimilarityDistance = distance;
            var similarity_1 = new UsersSimilarity
            {
                ComparedUserRatesForMutualBooks = _helpers.CreateBookScores(2, new[] {"1", "2", "3"}, new short[] {1, 2, 2}),
                UserRatesForMutualBooks = _helpers.CreateBookScores(1, new[] {"1", "2", "3"}, new short[] {1, 2, 2}),
                ComparedUserId = 2
            };

            var similarity_2 = new UsersSimilarity
            {
                ComparedUserRatesForMutualBooks = _helpers.CreateBookScores(3, new[] {"1", "2", "3"}, new short[] {1, 2, 2}),
                UserRatesForMutualBooks = _helpers.CreateBookScores(1, new[] {"1", "2", "3"}, new short[] {1, 4, 2}),
                ComparedUserId = 3
            };

            var similarity_3 = new UsersSimilarity
            {
                ComparedUserRatesForMutualBooks = _helpers.CreateBookScores(4, new[] {"1", "2", "3"}, new short[] {1, 2, 2}),
                UserRatesForMutualBooks = _helpers.CreateBookScores(1, new[] {"1", "2", "3"}, new short[] {10, 5, 3}),
                ComparedUserId = 4
            };

            _usersSelector.SelectMutualAndUniqueBooksForUsers(1, 2).Returns(similarity_1);
            _usersSelector.SelectMutualAndUniqueBooksForUsers(1, 3).Returns(similarity_2);
            _usersSelector.SelectMutualAndUniqueBooksForUsers(1, 4).Returns(similarity_3);

            var actual = _sut.GetNearestNeighbors(1, new List<int>() {2, 3, 4}, _settings);

            actual.ShouldNotBeNull();
            actual.Select(x => x.ComparedUserId).ShouldBe(new[] {2, 3});
            actual.Count.ShouldBe(2);
        }
    }
}