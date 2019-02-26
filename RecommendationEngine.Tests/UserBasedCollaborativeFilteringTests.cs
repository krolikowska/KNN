using System.Linq;
using DataAccess;
using NSubstitute;
using Shouldly;
using Xunit;

namespace RecommendationEngine.Tests
{
    public class UserBasedCollaborativeFilteringTests
    {
        private readonly INearestNeighborsSearch _nearestNeighbors;
        private readonly IBookRecommender _recommender;
        private readonly IUsersSelector _selector;
        private readonly TestSettings _settings;
        private readonly UserBasedCollaborativeFiltering _sut;

        public UserBasedCollaborativeFilteringTests()
        {
            _nearestNeighbors = Substitute.For<INearestNeighborsSearch>();
            _recommender = Substitute.For<IBookRecommender>();
            _selector = Substitute.For<IUsersSelector>();
            _settings = new TestSettings();

            _sut = new UserBasedCollaborativeFiltering(_recommender, _nearestNeighbors, _selector);
        }

        [Fact]
        public void RecommendBooksForUser_ShouldGetFromDbByDefault()
        {
            const int userId = 1;
            var books = new[] {new Book {ISBN = "1",}, new Book {ISBN = "2"}};

            _recommender.GetRecommendedBooksFromDatabase(userId).Returns(books);

            var result = _sut.RecommendBooksForUser(userId, _settings);

            _selector.DidNotReceiveWithAnyArgs().SelectUsersIdsToCompareWith(userId);
            _nearestNeighbors.DidNotReceiveWithAnyArgs().GetNearestNeighbors(userId, null, _settings);
            result.Select(x => x.ISBN).ShouldBe(new []{"1", "2"});
        }

        [Fact]
        public void RecommendBooksForUser_ShouldRunAlghorithmIfThereIsNothingInDb()
        {
            const int userId = 1;
            var books = new[] { new Book { ISBN = "1", }, new Book { ISBN = "2" } };

            _recommender.GetRecommendedBooksFromDatabase(userId).Returns(new Book[0]);

            _sut.RecommendBooksForUser(userId, _settings);

            var users = _selector.Received().SelectUsersIdsToCompareWith(userId);
            _nearestNeighbors.Received().GetNearestNeighbors(userId, users, _settings);
        }
    }
}
