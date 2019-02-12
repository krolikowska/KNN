using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecommendationEngine
{
    public class BookRecommender : IBookRecommender
    {
        private readonly int _numbOfBooksToRecommend;
        private readonly IDataManager _context;
        private readonly int _minNumOfUsersWhoRatedBook;

        public BookRecommender(ISettings settings, IDataManager context)
        {
            _context = context;
            _numbOfBooksToRecommend = settings.NumOfBooksToRecommend;
            _minNumOfUsersWhoRatedBook = settings.MinNumberOfBooksEachUserRated;
        }

        public Book[] GetRecommendedBooksFromDatabase(int userId)
        {
            return _context.GetRecommendedBooksForUser(userId).ToArray();
        }

        public Book[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId)
        {
            var bookIds = GetRecommendedBooksWithScores(similarUsers, userId).Select(x => x.BookId);
            return _context.GetBooksFromGivenBookScores(bookIds);
        }

        public List<BookScore> PredictScoreForAllUsersBooks(List<UsersSimilarity> usersSimilarity, int userId)
        {
            var booksUserRead = _context.GetBooksRatesByUserId(userId);

            var predictedScores = new List<BookScore>(booksUserRead.Length);
            var meanRateForUser = _context.GetAverageRateForUser(userId) ?? 0;

            // we want to predict scores for books that user already rated
            usersSimilarity = CombineUniqueAndMutualBooksForUser(usersSimilarity);
            for (var i = 0; i < booksUserRead.Length; i++)
            {
                var book = booksUserRead[i];
                var score = EvaluateScore(usersSimilarity, book, meanRateForUser);
                if (score.PredictedRate > 0)
                {
                    predictedScores.Add(score);
                }
            }

            return predictedScores;
        }

        public void PersistRecommendedBooksInDb(BookScore[] books, int userId) =>
            _context.AddRecommendedBooksForUser(books, userId);

        private BookScore[] GetRecommendedBooksWithScores(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = PreparePotentialBooksToRecommendation(similarUsers, userId);
            var booksRates = GetAllRecommendedBooksForUser(similarUsers, userId, booksIds);
            var result = booksRates.Take(_numbOfBooksToRecommend).ToArray();
            PersistRecommendedBooksInDb(result, userId);
            return result.ToArray();
        }

        private SortedSet<BookScore> GetAllRecommendedBooksForUser(List<UsersSimilarity> similarUsers, int userId,
            string[] booksIds)
        {
            if (similarUsers == null || similarUsers?.Count == 0) return null;
            var recommendedBooks = new SortedSet<BookScore>(new BookScoreComparer());

            if (booksIds.Length == 0)
            {
                PersistRecommendedBooksInDb(null, userId);
                return null;
            }

            var meanRateForUser = _context.GetAverageRateForUser(userId) ?? 0;

            foreach (var id in booksIds)
            {
                var temp = new BookScore
                {
                    UserId = userId,
                    BookId = id
                };
                var bookScore = EvaluateScore(similarUsers, temp, meanRateForUser);
                if (bookScore != null)
                    recommendedBooks.Add(bookScore);
            }

            return recommendedBooks;
        }

        private BookScore EvaluateScore(IEnumerable<UsersSimilarity> similarUsers, BookScore bookScore,
            double meanRateForUser)
        {
            var scoreNumerator = 0.0;
            var scoreDenominator = 0.0;
            var rate = 0.0;

            foreach (var u in similarUsers)
            {
                var book = u.BooksUniqueForComparedUser.FirstOrDefault(x => x.BookId == bookScore.BookId);

                if (book == null || u.AverageScoreForComparedUser == null || u.Similarity == null) continue;
                scoreNumerator += (book.Rate - u.AverageScoreForComparedUser.Value) * u.Similarity.Value;
                scoreDenominator += Math.Abs(u.Similarity.Value);
            }

            if (scoreDenominator != 0)
            {
                rate = scoreNumerator / scoreDenominator + meanRateForUser;
            }

            bookScore.PredictedRate = Math.Round(rate, 2);
            return bookScore;
        }

       

        private string[] PreparePotentialBooksToRecommendation(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = GetUniqueBooksIds(similarUsers); // we get list contains all books read by neighbors,
            return _minNumOfUsersWhoRatedBook == 0
                ? booksIds
                : _context.GetBooksIdsRatedByAtLeastNUsers(booksIds,
                                                           _minNumOfUsersWhoRatedBook);
        }

        private string[] GetUniqueBooksIds(List<UsersSimilarity> similarUsers)
        {
            // unique list of books we recommend
            return similarUsers
                   .SelectMany(x => x.BooksUniqueForComparedUser
                                     .Select(b => b.BookId))
                   .Distinct()
                   .ToArray();
        }

        private List<UsersSimilarity> CombineUniqueAndMutualBooksForUser(List<UsersSimilarity> similarUsers)
        {
            foreach (var u in similarUsers)
            {
                u.BooksUniqueForComparedUser = u.ComparedUserRatesForMutualBooks;
            }

            return similarUsers;
        }
    }
}