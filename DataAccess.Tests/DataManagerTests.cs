using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

namespace DataAccess.Tests
{
    public class DataManagerTests
    {
        public DataManagerTests()
        {
            _sut = new DataManager();
            _dm = new DataManagerHelpers();

            _dm.AddUsers(new[] {1, 2, 3, 4});
            _dm.AddBooks(new[] {"1111", "1112", "1113", "1114", "1115", "2222"});

            AddBooksRatedByUser(1, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            AddBooksRatedByUser(2, new[] {"1111", "1112", "1113"}, new short[] {1, 10, 5});
            AddBooksRatedByUser(3, new[] {"1113", "1114", "1115"}, new short[] {8, 5, 7});
            AddBooksRatedByUser(4, new[] {"1111", "2222", "1113", "1115"}, new short[] {7, 4, 5, 5});
        }

        private readonly DataManager _sut;
        private readonly DataManagerHelpers _dm;

        [InlineData(0, 4)]
        [InlineData(4, 1)]
        [InlineData(5, 0)]
        [Theory]
        public void GetBooksRatedByMoreThanNusers(int n, int expectedCount)
        {
            var books = new[] {"1111", "1113", "1115", "2222"};
            var actual = _sut.GetBooksIdsRatedByAtLeastNUsers(books, n);
            actual.Length.ShouldBe(expectedCount);
        }

        private BookScore[] CreateBookRatesBasedOnUser(int userId, List<BooksRating> booksRatedByUser)
        {
            var result = new List<BookScore>();
            for (var i = 0; i < booksRatedByUser.Count(); i++)
                result.Add(new BookScore
                {
                    BookId = booksRatedByUser[i].ISBN,
                    Rate = booksRatedByUser[i].Rate,
                    PredictedRate = _dm.RandomNumber(i)
                });
            return result.ToArray();
        }

        private void AddBooksRatedByUser(int userId, string[] isbn, short[] rate)
        {
            for (var i = 0; i < isbn.Length; i++)
            {
                var bookRate = new BooksRating
                {
                    ISBN = isbn[i],
                    UserId = userId,
                    Rate = rate[i]
                };

                _dm.AddBooksRatings(bookRate);
            }
        }

        //dobry
        [Fact]
        public void AddRecommendedBooksForUser_ShouldAddCorrectEntriesForBothUsers()
        {
            var userId = 1;
            var booksRatedByUser = _dm.GetBooksRatingsForGivenUser(userId);
            var books = CreateBookRatesBasedOnUser(userId, booksRatedByUser);

            _sut.AddRecommendedBooksForUser(books, 1);

            userId = 2;
            booksRatedByUser = _dm.GetBooksRatingsForGivenUser(userId);
            books = CreateBookRatesBasedOnUser(userId, booksRatedByUser);

            _sut.AddRecommendedBooksForUser(books, 2);

            var actual = _dm.GetAllBookRecomendadtion();
            actual.Count.ShouldBe(6);
        }

        //dobry
        [Fact]
        public void AddRecommendedBooksForUser_ShouldRemoveEntriesBeforeAddNewOnes()
        {
            var userId = 1;
            var booksRatedByUser = _dm.GetBooksRatingsForGivenUser(userId);
            var books = CreateBookRatesBasedOnUser(userId, booksRatedByUser);

            _sut.AddRecommendedBooksForUser(books, 1);

            booksRatedByUser = _dm.GetBooksRatingsForGivenUser(2);
            books = CreateBookRatesBasedOnUser(2, booksRatedByUser);

            _sut.AddRecommendedBooksForUser(books, 1);

            var actual = _dm.GetAllBookRecomendadtion();
            actual.Count.ShouldBe(3);
        }

        [Fact]
        public void AddUser2()
        {
            //AddUserWithRatedBook(2, "12345", 5);
            var actual = _sut.GetAllUsersWithRatedBooks();
            actual.Length.ShouldBe(4);
        }

        //dobry
        [Fact]
        public void GetAverageRateForUser_WhenUserDoesntRateAnyBookReturnsNull()
        {
            var acutal = _sut.GetAverageRateForUser(1000);
            acutal.ShouldBe(null);
        }

        [Fact] //dobry
        public void GetAverageRateForUser_WhenUserRatesBookReturnsValue()
        {
            var userId = 1;
            var booksRatedByUser = _dm.GetBooksRatingsForGivenUser(userId);
            var actual = _sut.GetAverageRateForUser(userId);

            actual.ShouldNotBeNull();
            actual.ShouldBe(Math.Round(booksRatedByUser.Average(x => x.Rate), 2));
        }

        [Fact]
        public void GetBooksRatedByMoreThanNusers_ReturnsCorrect()
        {
            var books = new[] {"1111", "1113", "1115", "2222"};
            var actual = _sut.GetBooksIdsRatedByAtLeastNUsers(books, 3);
            actual.ShouldBe(new[] {"1111", "1113"});
        }

        //dobry
        [Fact]
        public void GetRecomendationsForUser()
        {
            var userId = 1;
            var booksRatedByUser = _dm.GetBooksRatingsForGivenUser(userId);
            var books = CreateBookRatesBasedOnUser(userId, booksRatedByUser);

            _sut.AddRecommendedBooksForUser(books, userId);

            var actual = _sut.GetRecommendedBooksForUser(userId);
            actual.Length.ShouldBe(3);
        }
    }
}