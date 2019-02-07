using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;
using NSubstitute;

namespace DataAccess.Tests
{
    public class DataManagerTests
    {
        private readonly DataManager _sut;
        private readonly DataManagerHelpers _helper;

        public DataManagerTests()
        {
            _sut = new DataManager();
            _helper = new DataManagerHelpers();
        }

        [Fact]
        public void GetAllUsersWithRatedBooks_ShouldReturnNone()
        {
            var actual = _sut.GetAllUsersWithRatedBooks();
            actual.ShouldBeEmpty();
        }

        [Fact]
        public void GetAllUsersWithRatedBooks_ShouldReturnAllUsersWithBooks()
        {
            _helper.AddBooksRatedByUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(2, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(3, new[] {"1113", "1114", "1115"}, new short[] {8, 5, 7});
            _helper.AddBooksRatedByUser(4, new[] {"1111", "2222", "1113", "1115"}, new short[] {7, 4, 5, 5});
            _helper.AddUser(5);

            var actual = _sut.GetAllUsersWithRatedBooks();

            actual.ShouldSatisfyAllConditions(
                                              () => actual.Length.ShouldBe(4),
                                              () => actual.Select(x => x.UserId).ShouldBe(new[] {1, 2, 3, 4}));
        }

        [Fact]
        public void GetUserIdsWithNorMoreRatedBooks_ReturnNullWhenUsersReadToLessBooks()
        {
            _helper.AddBooksRatedByUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(2, new[] {"1111", "1112"}, new short[] {10, 5});

            var actual = _sut.GetUserIdsWithNorMoreRatedBooks(4);

            actual.ShouldBeEmpty();
        }

        [Fact]
        public void GetUserIdsWithNorMoreRatedBooks_ReturnsCorrectAmountOfUsers()
        {
            _helper.AddBooksRatedByUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(2, new[] {"1111", "1112"}, new short[] {10, 5});
            _helper.AddBooksRatedByUser(3, new[] {"1113", "1114", "1115", "1144"}, new short[] {8, 5, 7, 4});
            _helper.AddBooksRatedByUser(4, new[] { "1113", "1114", "1115", "1144" }, new short[] { 8, 5, 7, 4 });

            var actual = _sut.GetUserIdsWithNorMoreRatedBooks(3);

            actual.ShouldSatisfyAllConditions(
                                              () => actual.Count.ShouldBe(2),
                                              () => actual.ShouldBe(new[] {1, 3}));
        }

        [Fact]
        public void GetBooksReadByUser_ShouldReturnCorrectResult()
        {
            var user = new User {UserId = 1};
            var books = new[] {"1111", "1112", "1113"};
            _helper.AddBooksRatedByUser(user.UserId, books, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(2, new[] {"145", "17"}, new short[] {10, 5});

            var actual = _sut.GetBooksReadByUser(user);

            actual.Select(x => x.ISBN).ShouldBe(books);
        }

        [Fact]
        public void GetBooksRatesByUserId_ShouldReturnCorrectResult()
        {
            var books = new[] {"1111", "1112", "1113"};
            var rates = new short[] {1, 10, 5};
            _helper.AddBooksRatedByUser(1, books, rates);
            _helper.AddBooksRatedByUser(2, new[] {"145", "17"}, new short[] {6, 2});

            var actual = _sut.GetBooksRatesByUserId(1);

            actual.Select(x => x.Rate).ShouldBe(rates);
        }

        [Fact]
        public void GetAverageRateForUser_WhenUserDoesNotRateAnyBookReturnsNull()
        {
            var actual = _sut.GetAverageRateForUser(1000);
            actual.ShouldBe(null);
        }

        [Fact]
        public void GetAverageRateForUser_WhenUserRatesBookReturnsValue()
        {
            const int userId = 1;
            var books = new[] {"1111", "1112", "1113"};
            var rates = new short[] {1, 10, 5};
            _helper.AddBooksRatedByUser(userId, books, rates);

            var actual = _sut.GetAverageRateForUser(userId);

            actual.ShouldSatisfyAllConditions(
                                              () => actual.ShouldNotBeNull(),
                                              () => actual.ShouldBe(Math.Round(rates.Average(x => x), 2)));
        }

        [Fact]
        public void AddRecommendedBooksForUser_ShouldAddCorrectEntriesForBothUsers()
        {
            var booksForFirstUser =
                _helper.CreateBookScoreBasedOnUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            var booksForSecondUser = _helper.CreateBookScoreBasedOnUser(2, new[] {"1131", "1111"}, new short[] {10, 2});

            _sut.AddRecommendedBooksForUser(booksForFirstUser, 1);
            _sut.AddRecommendedBooksForUser(booksForSecondUser, 2);

            var actual = _helper.GetAllBookRecomendadtion();
            actual.ShouldSatisfyAllConditions(
                                              () => actual.Count.ShouldBe(5),
                                              () => actual
                                                    .Select(x => x.BookId)
                                                    .ShouldBe(new[] {"1111", "1112", "1113", "1131", "1111"}));
        }

        [Fact]
        public void AddRecommendedBooksForUser_ShouldRemoveEntriesBeforeAddNewOnes()
        {
            var recommendedBooks =
                _helper.CreateBookScoreBasedOnUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            var recommendedBooksSecondTime =
                _helper.CreateBookScoreBasedOnUser(1, new[] {"1131", "1122"}, new short[] {10, 2});

            _sut.AddRecommendedBooksForUser(recommendedBooks, 1);
            _sut.AddRecommendedBooksForUser(recommendedBooksSecondTime, 1);

            var actual = _helper.GetAllBookRecomendadtion();
            actual.ShouldSatisfyAllConditions(
                                              () => actual.Count.ShouldBe(2),
                                              () => actual.Select(x => x.BookId).ShouldBe(new[] {"1131", "1122"}));
        }

        [Fact]
        public void GetUsersWhoRatedAnyOfGivenBooks()
        {
            _helper.AddBooksRatedByUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(2, new[] {"1111", "1112"}, new short[] {10, 5});
            _helper.AddBooksRatedByUser(3, new[] {"1113", "1114", "1115", "1444"}, new short[] {8, 5, 7, 4});
            _helper.AddBooksRatedByUser(4, new[] {"2"}, new short[] {7});

            var books = new[] {"1111", "1144", "2"};

            var actual = _sut.GetUsersWhoRatedAnyOfGivenBooks(books);

            actual.ShouldBe(new[] {1, 2, 4});
        }

        [Fact]
        public void AddSimilarUsers()
        {
            var neighbors = new[] {2, 3, 4, 5};
            const int settingsId = 1;
            var users = _helper.CreateSimilarUsers(1, neighbors, settingsId);

            _sut.AddSimilarUsers(users, settingsId);

            var actual = _helper.GetAllSimilarUsers(settingsId);
            actual.Select(x => x.NeighborId).ShouldBe(neighbors);
        }

        [Fact]
        public void GetUserNeighbors_ShouldReturnsOnlyForGivenUser()
        {
            var neighbors1 = new[] {2, 3, 4, 5};
            var neighbors2 = new[] {3, 7};
            const int settingsId = 1;
            var users1 = _helper.CreateSimilarUsers(1, neighbors1, settingsId);
            var users2 = _helper.CreateSimilarUsers(2, neighbors2, settingsId);

            _sut.AddSimilarUsers(users1, settingsId);
            _sut.AddSimilarUsers(users2, settingsId);

            var actual = _sut.GetUserNeighbors(1, settingsId);

            actual.Select(x => x.NeighborId).ShouldBe(neighbors1);
        }

        [Theory]
        [InlineData(1, 1, new[] {2, 3, 4, 5})]
        [InlineData(1, 2, new int[] { })]
        [InlineData(2, 1, new int[] { })]
        [InlineData(2, 2, new[] {3, 7})]
        public void GetUserNeighbors_ShouldReturnsOnlyForGivenParameterSet(int userId, int settingsId, int[] expected)
        {
            var neighbors1 = new[] {2, 3, 4, 5};
            var neighbors2 = new[] {3, 7};
            const int settingsId1 = 1;
            const int settingsId2 = 2;
            var users1 = _helper.CreateSimilarUsers(1, neighbors1, settingsId1);
            var users2 = _helper.CreateSimilarUsers(2, neighbors2, settingsId2);

            _sut.AddSimilarUsers(users1, settingsId1);
            _sut.AddSimilarUsers(users2, settingsId2);

            var actual = _sut.GetUserNeighbors(userId, settingsId);

            actual.Select(x => x.NeighborId).ShouldBe(expected);
        }

        [Theory]
        [InlineData(1, 1, new[] {1, 2})]
        [InlineData(1, 2, new[] {1})]
        public void GetAllUsersWithComputedSimilarity(int settingsId1, int settingsId2, int[] expected)
        {
            var neighbors1 = new[] {2, 3, 4, 5};
            var neighbors2 = new[] {3, 7};

            var users1 = _helper.CreateSimilarUsers(1, neighbors1, settingsId1);
            var users2 = _helper.CreateSimilarUsers(2, neighbors2, settingsId2);

            _sut.AddSimilarUsers(users1, settingsId1);
            _sut.AddSimilarUsers(users2, settingsId2);

            var actual = _sut.GetAllUsersWithComputedSimilarity(settingsId1);

            actual.ShouldBe(expected);
        }

        [Fact]
        public void SaveAndGetParameters()
        {
            const int distanceType = 2;
            const int settingsVersion = 1;
            _helper.AddDistanceSimilarityTypes(distanceType);
            var param = new Parameter
            {
                Id = settingsVersion,
                BookPopularity = 3,
                DistanceType = distanceType,
                Kneigbors = 10,
                NumberOfBooksEachUserRated = 5
            };

            _sut.SaveParametersSet(param);
            var actual = _sut.GetParameters(settingsVersion);

            actual.ShouldSatisfyAllConditions(
                                              () => actual.Id.ShouldBe(param.Id),
                                              () => actual.BookPopularity.ShouldBe(param.BookPopularity),
                                              () => actual.DistanceType.ShouldBe(param.DistanceType),
                                              () => actual.Kneigbors.ShouldBe(param.Kneigbors),
                                              () =>
                                                  actual.NumberOfBooksEachUserRated
                                                        .ShouldBe(param.NumberOfBooksEachUserRated));
        }

        [Fact]
        public void GetBooksRatedByMoreThanNusers_ReturnsCorrect()
        {
            _helper.AddBooksRatedByUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(2, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            _helper.AddBooksRatedByUser(3, new[] {"1113", "1114", "1115"}, new short[] {8, 5, 7});
            _helper.AddBooksRatedByUser(4, new[] {"1111", "2222", "1113", "1115"}, new short[] {7, 4, 5, 5});
            var books = new[] {"1111", "1113", "1115", "2222"};

            var actual = _sut.GetBooksIdsRatedByAtLeastNUsers(books, 3);

            actual.ShouldBe(new[] {"1111", "1113"});
        }

        [Fact]
        public void GetRecommendedBooksForUser_ReturnsResultForGivenUser()
        {
            var booksForFirstUser =
                _helper.CreateBookScoreBasedOnUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            var booksForSecondUser = _helper.CreateBookScoreBasedOnUser(2, new[] {"1131", "1111"}, new short[] {10, 2});

            _sut.AddRecommendedBooksForUser(booksForFirstUser, 1);
            _sut.AddRecommendedBooksForUser(booksForSecondUser, 2);

            var actual = _sut.GetRecommendedBooksForUser(1);
            actual.ShouldSatisfyAllConditions(
                                              () => actual.Length.ShouldBe(3),
                                              () => actual
                                                    .Select(x => x.ISBN)
                                                    .ShouldBe(new[] {"1111", "1112", "1113"}, true));
        }
    }
}