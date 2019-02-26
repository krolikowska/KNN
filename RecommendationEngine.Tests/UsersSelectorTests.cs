using System.Collections.Generic;
using System.Linq;
using DataAccess;
using NSubstitute;
using Shouldly;
using Xunit;

namespace RecommendationEngine.Tests
{
    public class UsersSelectorTests
    {
        private readonly TestSettings _settings;
        private readonly TestHelpers _helpers;
        private readonly UsersSelector _sut;
        private readonly IDataManager _context;
        private readonly int _minNumberOfBooksUserRated;
        private readonly int _settingsVersion;

        public UsersSelectorTests()
        {
            _helpers = new TestHelpers();
            _settings = new TestSettings
            {
                SimilarityDistance = DistanceSimilarityEnum.PearsonSimilarity,
                NumOfNeighbors = 2,
                MinNumberOfBooksEachUserRated = 5
            };
            _context = Substitute.For<IDataManager>();
            _minNumberOfBooksUserRated = _settings.MinNumberOfBooksEachUserRated;
            _settingsVersion = _settings.Id;
            _sut = new UsersSelector(_context, _settings);
        }

        [Fact]
        public void SelectUsersIdsToCompareWith_ReturnsUsersWithoutActiveUserId()
        {
            const int userId = 1;
            var users = new List<int> { 1, 2, 3, 4, 5 };
            _context.GetUserIdsWithNorMoreRatedBooks(_minNumberOfBooksUserRated).Returns(users);

            var usersIdsToCompare = _sut.SelectUsersIdsToCompareWith(userId);
            usersIdsToCompare.ShouldSatisfyAllConditions(
                                                         () => usersIdsToCompare.Count.ShouldBe(4),
                                                         () => usersIdsToCompare.ShouldNotContain(1));
        }

        [Fact]
        public void SelectMutualAndUniqueBooksForUsers_WhenTheyAreNoSimilarBooksReturnsNull()
        {
            const int userId = 1;
            const int comparedUserId = 2;
            var books1 = _helpers.CreateBookScores(userId, new[] { "1", "2", "3" }, new short[] { 5, 6, 7 });
            var books2 = _helpers.CreateBookScores(comparedUserId, new[] { "4", "5", "6" }, new short[] { 5, 6, 7 });
            _context.GetBooksRatesByUserId(userId).Returns(books1);
            _context.GetBooksRatesByUserId(comparedUserId).Returns(books2);

            var result = _sut.SelectMutualAndUniqueBooksForUsers(userId, comparedUserId);
            result.ShouldBeNull();
        }

        [Fact]
        public void SelectMutualAndUniqueBooksForUsers_WhenTheyAreSimilarBooksReturnsCorrectValue()
        {
            const int userId = 1;
            const int comparedUserId = 2;
            var books1 = new[] { "1", "2", "3" };
            var books2 = new[] { "3", "5", "6" };
            var rates1 = new short[] { 5, 6, 7 };
            var rates2 = new short[] { 4, 7, 7 };
            var bookScores1 = _helpers.CreateBookScores(userId, books1, rates1);
            var bookScores2 = _helpers.CreateBookScores(comparedUserId, books2, rates2);
            _context.GetBooksRatesByUserId(userId).Returns(bookScores1);
            _context.GetBooksRatesByUserId(comparedUserId).Returns(bookScores2);
            _context.GetAverageRateForUser(comparedUserId).Returns(5);

            var result = _sut.SelectMutualAndUniqueBooksForUsers(userId, comparedUserId);

            result.ShouldSatisfyAllConditions(
                                              () => result.ShouldNotBeNull(),
                                              () => result.AverageScoreForComparedUser.ShouldBe(5),
                                              () => result.BooksUniqueForComparedUser.Select(x => x.BookId).ShouldBe(new[] { "5", "6" }),
                                              () => result.ComparedUserRatesForMutualBooks.Select(x => x.Rate).ShouldBe(new short[] { 4 }),
                                              () => result.ComparedUserId.ShouldBe(comparedUserId),
                                              () => result.UserRatesForMutualBooks.Select(x => x.Rate).ShouldBe(new short[] { 7 }),
                                              () => result.UserId.ShouldBe(userId),
                                              () => result.Similarity.ShouldBeNull());


        }
    }
}