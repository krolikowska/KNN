using System;
using System.Linq;
using System.Net;
using DataAccess;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RecommendationApi.Controllers;
using RecommendationEngine;
using RecommendationEngine.Tests;
using Shouldly;
using Xunit;

namespace BookRecommendationWebApi.Tests
{
    public class RecommendationControllerTests
    {
        private readonly IUserBasedCollaborativeFiltering _runner;
        private readonly ISettings _settings;
        private readonly IRecommendationEvaluator _evaluator;
        private readonly RecommendationsController _sut;

        public RecommendationControllerTests()
        {
            _runner = Substitute.For<IUserBasedCollaborativeFiltering>();
            _settings = new TestSettings();
            _evaluator = Substitute.For<IRecommendationEvaluator>();
            _sut = new RecommendationsController(_runner, _settings, _evaluator);
        }

        [Fact]
        public void EvaluateScore()
        {
            const int userId = 1;
            const double score = 3;
            _evaluator.EvaluateScoreForUSer(userId, _settings).Returns(score);

            var result = _sut.EvaluateScore(userId);
            
            result.ShouldBe(score);
        }

        [Fact]
        public void GetRecommendedBookIdsForUser_ReturnsNoContentWhenThereIsNoBooksToRecommend()
        {
            const int userId = 1;
            var books = new Book[0];

            _runner.RecommendBooksForUser(userId, _settings).Returns(books);

            var result = _sut.GetRecommendedBookIdsForUser(userId);

            result.Content.ShouldBeNull();
            result.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        [Fact]
        public void GetRecommendedBookIdsForUser_ReturnsOkStatus()
        {
            const int userId = 1;
            var books = new[]
            {
                new Book
                {
                    ISBN = "1",
                    BookTitle = "Title_1",
                    ImageURLL = "big_1",
                    ImageURLM = "medium_1",
                    ImageURLS = "small_1",
                    BookAuthor = "Author_1",
                    Publisher = "Editor",
                    YearOfPublication = 2012

                },
                new Book {ISBN = "2"}
            };

            _runner.RecommendBooksForUser(userId, _settings).Returns(books);

            var result = _sut.GetRecommendedBookIdsForUser(userId);

            result.Content.Select(x => x.ISBN).ShouldBe(new[] {"1", "2"});
            var book = result.Content.FirstOrDefault(x => x.ISBN == "1");
            book.ShouldSatisfyAllConditions(() => book.ISBN.ShouldBe("1"),
                                            () => book.BookTitle.ShouldBe("Title_1"),
                                            () => book.ImageURLL.ShouldBe("big_1"),
                                            () => book.ImageURLM.ShouldBe("medium_1"),
                                            () => book.ImageURLS.ShouldBe("small_1"),
                                            () => book.BookAuthor.ShouldBe("Author_1"),
                                            () => book.Publisher.ShouldBe("Editor"),
                                            () => book.YearOfPublication.ShouldBe(2012));
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public void GetRecommendedBookIdsForUser_ReturnsUnavailableStatus()
        {
            const int userId = 1;

            _runner.RecommendBooksForUser(userId, _settings).Throws(new Exception());

            var result = _sut.GetRecommendedBookIdsForUser(userId);

            result.Content.ShouldBeNull();
            result.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }
    }
}
