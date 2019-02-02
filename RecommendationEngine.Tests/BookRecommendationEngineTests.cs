using System.Collections.Generic;
using System.Linq;
using DataAccess;
using NSubstitute;
using Priority_Queue;
using RecommendationEngine.Properties;
using Shouldly;
using Xunit;

namespace RecommendationEngine.Tests
{
    public class BookRecommendationEngineTests
    {
        private readonly NearestNeighborsSearch _sut;
        private readonly IDataManager _dm;
        public BookRecommendationEngineTests()
        {
            var settings = Substitute.For<ISettings>();
            _dm = Substitute.For<IDataManager>();
            _sut = new NearestNeighborsSearch(settings);
        }

      

        public User CreateUser(Book[] books, short[] rates, int id)
        {
            return new User
            {
                UserId = id,
                BooksRatings = CreateBookRatings(books, rates, id)
            };
        }

        public List<BooksRating> CreateBookRatings(Book[] books, short[] rates, int userId)
        {
            var booksRatings = new List<BooksRating>();
            for (var i = 0; i < books.Length; i++)
                booksRatings.Add(new BooksRating
                {
                    Book = books[i],
                    Rate = rates[i],
                    ISBN = books[i].ISBN,
                    UserId = userId
                });
            return booksRatings;
        }

        public static int CompareBySmt(double x, double y)
        {
            if (x > y) return -1;

            if (x == y) return 0;
            return 1;
            ;
        }

        [Fact]
        public void ComputeSimilarityBetweenUsers()
        {
            Book[] books1 =
            {
                new Book {ISBN = "1"},
                new Book {ISBN = "2"},
                new Book {ISBN = "3"},
                new Book {ISBN = "4"},
                new Book {ISBN = "5"}
            };
            short[] rates1 = {5, 4, 2, 0, 2};

            Book[] books2 =
            {
                new Book {ISBN = "11"},
                new Book {ISBN = "12"},
                new Book {ISBN = "3"},
                new Book {ISBN = "4"},
                new Book {ISBN = "5"}
            };
            short[] rates2 = {5, 0, 0, 0, 2};

            var user1 = CreateUser(books1, rates1, 1);
            var user2 = CreateUser(books2, rates2, 2);
            _dm.GetBooksReadByUser(user1).Returns(books1);
            _dm.GetBooksReadByUser(user2).Returns(books2);

            // var actual = _sut.ComputeSimilarityBetweenUsers(user1, user2);

            //actual.Select(x => x.ISBN).ToArray().ShouldBe(new string[] { "3", "4", "5" }, false);
        }

        [Fact]
        public void ListOfBooks()
        {
            var mutual = new[] //te poodbne ktore ocenil sasiad
            {
                //new BookRates{ BookId = "1", Rate = 1},
                new BookScore {BookId = "2", Rate = 4},
                new BookScore {BookId = "3", Rate = 3}
                //new BookRates{ BookId = "4", Rate = 10},
                //new BookRates{ BookId = "5", Rate = 10},
                //new BookRates{ BookId = "6", Rate = 6}
            };
            var unique = new[]
            {
                new BookScore {BookId = "15", Rate = 9}
                //new BookRates{ BookId = "16", Rate = 10}
            };

            var unique2 = new[]
            {
                new BookScore {BookId = "15", Rate = 10},
                new BookScore {BookId = "16", Rate = 10}
            };

            //var sim = new UsersSimilarity
            //{
            //    UserId = 1,
            //    ComparedUserId = 2,
            //    Similarity = 1,
            //    ComparedUserRatesForMutualBooks = mutual,
            //    BooksUniqueForComparedUser = unique
            //};

            //var sim2 = new UsersSimilarity
            //{
            //    UserId = 1,
            //    ComparedUserId = 3,
            //    Similarity = 1,
            //    ComparedUserRatesForMutualBooks = mutual,
            //    BooksUniqueForComparedUser = unique2
            //};

            //var userSimList = new List<UsersSimilarity>
            //{
            //    sim, sim2
            //};
            //var actual = _sut.GetUniqueBooksIds(userSimList);
            //actual.Count().ShouldBe(2);
        }

        [Fact]
        public void PredictRecommendBooks()
        {
            _dm.GetAverageRateForUser(1).Returns(5);
            var mutual = new[] //te poodbne ktore ocenil sasiad
            {
                //new BookRates{ BookId = "1", Rate = 1},
                new BookScore {BookId = "2", Rate = 4},
                new BookScore {BookId = "3", Rate = 3}
                //new BookRates{ BookId = "4", Rate = 10},
                //new BookRates{ BookId = "5", Rate = 10},
                //new BookRates{ BookId = "6", Rate = 6}
            };

            var unique = new[]
            {
                new BookScore {BookId = "15", Rate = 9}
                //new BookRates{ BookId = "16", Rate = 10}
            };

            var unique2 = new[]
            {
                new BookScore {BookId = "15", Rate = 10},
                new BookScore {BookId = "16", Rate = 10}
            };

            //var sim = new UsersSimilarity
            //{
            //    UserId = 1,
            //    ComparedUserId = 2,
            //    Similarity = 1,
            //    ComparedUserRatesForMutualBooks = mutual,
            //    BooksUniqueForComparedUser = unique
            //};

            //var sim2 = new UsersSimilarity
            //{
            //    UserId = 1,
            //    ComparedUserId = 3,
            //    Similarity = 1,
            //    ComparedUserRatesForMutualBooks = mutual,
            //    BooksUniqueForComparedUser = unique2
            //};
            //var userSimList = new List<UsersSimilarity>
            //{
            //    sim, sim2
            //};

            Book[] books1 =
            {
                //new Book() {ISBN = "1"},
                new Book {ISBN = "2"},
                new Book {ISBN = "3"}
                //new Book() {ISBN = "4"},
                //new Book() {ISBN = "5"},
                //new Book() {ISBN = "6"}
            };
            short[] rates1 = {7, 6};

            var user1 = CreateUser(books1, rates1, 1);

            // var actual = _sut.PredictRecommendBooks(userSimList, user1, 1, DistanceSimilarityEnum.PearsonSimilarity, 0);
            //actual.Count().ShouldBe(1);
            //actual[0].PredictedRate.ShouldBe(10.5);
            //actual[1].RatePredictionCoeficient.ShouldBe(2);
        }

        [Fact]
        public void Simtest()
        {
            var bookRatings1 = new[]
            {
                new BookScore {BookId = "1", Rate = 5},
                new BookScore {BookId = "2", Rate = 2},
                new BookScore {BookId = "5", Rate = 5},
                new BookScore {BookId = "3", Rate = 0},
                new BookScore {BookId = "4", Rate = 1},
                new BookScore {BookId = "3", Rate = 1}
            };

            var bookRatings2 = new[]
            {
                new BookScore {BookId = "5", Rate = 5},
                new BookScore {BookId = "12", Rate = 2},
                new BookScore {BookId = "3", Rate = 4},
                new BookScore {BookId = "4", Rate = 2},
                new BookScore {BookId = "54", Rate = 1}
            };
            _dm.GetBooksRatesByUserId(1).Returns(bookRatings1);
            _dm.GetBooksRatesByUserId(2).Returns(bookRatings2);

            //  var actualUser1 = _sut.GetSameBooksFromUsers(1, 2);
            // var actualUser2 = _sut.GetSameBooksFromUsers(2, 1);

            //actualUser1.Select(x => x.BookId).ToArray().ShouldBe(new string[] { "3", "4", "5" });
            //actualUser2.Select(x => x.BookId).ToArray().ShouldBe(new string[] { "3", "4", "5" });
            //actualUser1.Select(x => x.Rate).ToArray().ShouldBe(new short[] { 0, 1, 5 });
            //actualUser2.Select(x => x.Rate).ToArray().ShouldBe(new short[] { 4, 2, 5 });
        }

        [Fact]
        public void SomeTest()
        {
            //double[] similarity1 = { -1, 0, -0.5, 0.8, 0.3, 0.9, -0.7 };
            //var sum = similarity1.Sum();
            //var absSum = Math.Abs(sum);

            //var actual = sum / absSum;

            //actual.ShouldBe(1);

            /// dla pearsona - jesli mamy od -1 do 1, gdzie -1 niepodobny, a 1 podobny, to gdybysmy go przesuneli o 1, to mam od 0 do 2, gdzie 0 niepodobny, a 2 podobny
            /// najpierw wynik * -1

            var queue = new SimplePriorityQueue<int, double>(Comparer<double>.Default);

            //queue.Enqueue(3, -1);
            //queue.Enqueue(4, 0.001);
            //queue.Enqueue(5, 1);
            //queue.Enqueue(0, -0.5);
            //queue.Enqueue(1, 0.5);
            //queue.Enqueue(2, 2);

            queue.Enqueue(3, 1);
            queue.Enqueue(4, 0.001);
            queue.Enqueue(5, 11);
            queue.Enqueue(0, 0.5);
            queue.Enqueue(1, 0.15);
            queue.Enqueue(2, 2);

            //queue.Count.ShouldBe(6);
            // var list = queue.ToList();

            var list = new List<int>();
            foreach (var i in queue) list.Add(queue.Dequeue());

            list[0].ShouldBe(4); //-1
            list[1].ShouldBe(1); //-0.5
            list[2].ShouldBe(0); //0
            list[3].ShouldBe(3); //0.5
            list[4].ShouldBe(2); //1
            list[5].ShouldBe(5); // 2

            // wnioski --> jak jest deque to zwraca dobrze, a jak jest take to ma problem

            // jesli teraz odwrocimy czyli nie 1, a -1 bedzie o najlepszym tym to powinno byc dobrze

            int[] a1 = {1, 2, 3, 4};
            int[] a2 = {1, 2, 6, 7};

            a2.Except(a1).ShouldBe(new[] {6, 7});
        }

        [Fact]
        public void TestGetDistance()
        {
            var bookRatings1 = new[]
            {
                new BookScore {BookId = "1", Rate = 1},
                new BookScore {BookId = "2", Rate = 2},
                new BookScore {BookId = "3", Rate = 1},
                new BookScore {BookId = "4", Rate = 1},
                new BookScore {BookId = "5", Rate = 10},
                new BookScore {BookId = "6", Rate = 10}
            };

            var bookRatings2 = new[]
            {
                new BookScore {BookId = "1", Rate = 1},
                new BookScore {BookId = "2", Rate = 2},
                new BookScore {BookId = "3", Rate = 1},
                new BookScore {BookId = "4", Rate = 1},
                new BookScore {BookId = "5", Rate = 10},
                new BookScore {BookId = "6", Rate = 10}
            };
            var actual3 = _sut.GetCosineDistance(bookRatings1, bookRatings2);
            var actual5 = _sut.GetPearsonCorrelationSimilarity(bookRatings1, bookRatings2);

            actual3.ShouldBe(0);
            actual5.ShouldBe(1, 0.0001);

            //1. "manhattan";
            //2. "czebyszew";
            //3. "cosinus";
            //4. "euclidian";
            //5. "pearson similiarity";
        }

        [Fact]
        public void TestGetFarestDistance()
        {
            var bookRatings1 = new[]
            {
                new BookScore {BookId = "1", Rate = 10},
                new BookScore {BookId = "2", Rate = 9},
                new BookScore {BookId = "3", Rate = 10},
                new BookScore {BookId = "4", Rate = 10},
                new BookScore {BookId = "5", Rate = 1},
                new BookScore {BookId = "6", Rate = 1}
            };

            var bookRatings2 = new[]
            {
                new BookScore {BookId = "1", Rate = 1},
                new BookScore {BookId = "2", Rate = 2},
                new BookScore {BookId = "3", Rate = 1},
                new BookScore {BookId = "4", Rate = 1},
                new BookScore {BookId = "5", Rate = 10},
                new BookScore {BookId = "6", Rate = 10}
            };
            var actual3 = _sut.GetCosineDistance(bookRatings1, bookRatings2);
            var actual5 = _sut.GetPearsonCorrelationSimilarity(bookRatings1, bookRatings2);

            actual3.ShouldBe(76.02, 0.01);
            actual5.ShouldBe(-1, 0.0001);
        }

        [Fact]
        public void TestGetOtherDistance()
        {
            var bookRatings1 = new[]
            {
                new BookScore {BookId = "1", Rate = 152},
                new BookScore {BookId = "2", Rate = 543},
                new BookScore {BookId = "3", Rate = 153},
                new BookScore {BookId = "4", Rate = 153},
                new BookScore {BookId = "5", Rate = 1},
                new BookScore {BookId = "6", Rate = 1}
            };

            var bookRatings2 = new[]
            {
                new BookScore {BookId = "1", Rate = 1},
                new BookScore {BookId = "2", Rate = 2},
                new BookScore {BookId = "3", Rate = 1},
                new BookScore {BookId = "4", Rate = 1},
                new BookScore {BookId = "5", Rate = 10},
                new BookScore {BookId = "6", Rate = 10}
            };
            ////var actual1 = _sut.GetManhattanDistance(bookRatings1, bookRatings2);
            //var actual2 = _sut.GetChebyshevDistance(bookRatings1, bookRatings2);
            var actual3 = _sut.GetCosineDistance(bookRatings1, bookRatings2);
            //var actual4 = _sut.GetEculidianDistance(bookRatings1, bookRatings2);
            //var actual5 = _sut.GetPearsonCorrelationSimilarity(bookRatings1, bookRatings2);

            //actual1.ShouldBe(52);
            //actual2.ShouldBe(9);
            actual3.ShouldBe(0, 0.01);
            ////actual4.ShouldBe(21.31, 0.01);
            //actual5.ShouldBe(1, 0.0001);

            //1. "manhattan";
            //2. "czebyszew";
            //3. "cosinus";
            //4. "euclidian";
            //5. "pearson similiarity";
        }

      
    }
}