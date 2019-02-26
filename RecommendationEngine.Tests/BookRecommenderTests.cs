using System.Collections.Generic;
using System.Linq;
using DataAccess;
using NSubstitute;
using Shouldly;
using Xunit;

namespace RecommendationEngine.Tests
{
    public class BookRecommenderTests
    {
        private readonly TestSettings _settings;
        private readonly TestHelpers _helpers;
        private readonly IBookRecommender _sut;
        private readonly IDataManager _context;

        public BookRecommenderTests()
        {
            _helpers = new TestHelpers();
            _settings = new TestSettings
            {
                SimilarityDistance = DistanceSimilarityEnum.PearsonSimilarity,
                NumOfNeighbors = 2,
                NumOfBooksToRecommend = 1
            };
            _context = Substitute.For<IDataManager>();
            _sut = new BookRecommender(_settings, _context);
        }

        [Theory]
        [InlineData(0, new string[] { })]
        [InlineData(1, new string[] { "13" })]
        [InlineData(10, new string[] { "13", "12", "22" })]
        public void GetRecommendedBooks_ShouldRecommendNbooks(int numOfBooksToRecommend, string[] expected)
        {
            const int userId = 1;
            const int comparedUserId = 2;
            const int secondComparedUserId = 3;
            var userBooks = _helpers.CreateBookScores(userId, new[] { "1", "2", "3" }, new short[] { 1, 2, 3 });
            var comparedUserBooks = _helpers.CreateBookScores(comparedUserId, new[] { "1", "12", "13" }, new short[] { 1, 3, 5 });
            var secondComparedUserBooks = _helpers.CreateBookScores(secondComparedUserId, new[] { "1", "22", "13" }, new short[] { 1, 3, 2 });
            _settings.NumOfBooksToRecommend = numOfBooksToRecommend;

            var similarUsers = _helpers.CreateUserSimilarities(_settings,
                                            userId,
                                            new List<int> { comparedUserId, secondComparedUserId },
                                            userBooks,
                                            new List<BookScore[]> { comparedUserBooks, secondComparedUserBooks },
                                           new List<double?> { 1, 0.8 });

            _context.GetAverageRateForUser(userId).Returns(5);

            var books = _sut.GetRecommendedBooksWithScores(similarUsers, userId);
            books.Select(x => x.BookId).ShouldBe(expected);
        }

        [Fact]
        public void PredictScoreForAllUsersBooks_ShouldEvaluateForBooksUserRead()
        {
            const int userId = 1;
            const int comparedUserId = 2;
            const int secondComparedUserId = 3;
            var userBooks = _helpers.CreateBookScores(userId, new[] { "1", "2", "3" }, new short[] { 1, 2, 3 }, new[] { 0.0, 0, 0 });
            var comparedUserBooks = _helpers.CreateBookScores(comparedUserId, new[] { "1", "12", "13" }, new short[] { 1, 3, 5 }, new[] { 0.0, 0, 0 });
            var secondComparedUserBooks = _helpers.CreateBookScores(secondComparedUserId, new[] { "1", "22", "13" }, new short[] { 1, 3, 2 }, new[] { 0.0, 0, 0 });

            var similarUsers = _helpers.CreateUserSimilarities(_settings,
                                                               userId,
                                                               new List<int> { comparedUserId, secondComparedUserId },
                                                               userBooks,
                                                               new List<BookScore[]> { comparedUserBooks, secondComparedUserBooks },
                                                               new List<double?> { 1, 0.8 });

            _context.GetAverageRateForUser(userId).Returns(5);
            _context.GetBooksRatesByUserId(userId).Returns(userBooks);

            var result = _sut.PredictScoreForAllUsersBooks(similarUsers, userId);
            result.Select(x => x.BookId).ShouldBe(new[] { "1" });
            result.Select(x => x.PredictedRate).ShouldBe(new[] { 3.0 });
        }
    }
}
