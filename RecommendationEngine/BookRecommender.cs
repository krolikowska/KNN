using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    /// <summary>
    ///     A book recommender.
    /// </summary>
    public class BookRecommender : IBookRecommender
    {
        /// <summary>
        ///     The common.
        /// </summary>
        private readonly ICommon _common;

        /// <summary>
        ///     Number of books to recommends.
        /// </summary>
        private readonly int _numbOfBooksToRecommend;

        /// <summary>
        ///     Options for controlling the operation.
        /// </summary>
        private readonly ISettings _settings;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="settings">
        ///     Options for controlling the operation.
        /// </param>
        /// <param name="common">The common.</param>
        public BookRecommender(ISettings settings, ICommon common)
        {
            _settings = settings;
            _common = common;
            _numbOfBooksToRecommend = settings.NumOfBooksToRecommend;
        }

        /// <summary>
        ///     Gets recommended books.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book score.
        /// </returns>
        public BookScore[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = _common.PreparePotentialBooksToRecommendation(similarUsers, userId);
            var booksRates = GetAllRecommendedBooksForUser(similarUsers, userId, booksIds);
            var result = booksRates.Take(_numbOfBooksToRecommend).ToArray();
            _common.PersistRecommendedBooksInDb(result, userId);
            return result.ToArray();
        }

        /// <summary>
        ///     Predict score.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="booksIds">
        ///     <see cref="List`1" /> of identifiers for the books.
        /// </param>
        /// <returns>
        ///     A SortedSet<BookScore>
        /// </returns>
        public SortedSet<BookScore> GetAllRecommendedBooksForUser(List<UsersSimilarity> similarUsers, int userId,
            string[] booksIds)
        {
            if (similarUsers == null || similarUsers?.Count == 0) return null;
            var recommendedBooks = new SortedSet<BookScore>(new BookScoreComparer());

            if (booksIds.Length == 0)
            {
                _common.PersistRecommendedBooksInDb(null, userId);
                return null;
            }

            ;

            var meanRateForUser = _common.GetAverageRateForUser(userId) ?? 0;

            //foreach (var u in similarUsers)
            //    u.AverageScoreForComparedUser = _common.GetAverageRateForUser(u.ComparedUserId);

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

        /// <summary>
        ///     Predict score for given book.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="bookId">Identifier for the book.</param>
        /// <returns>
        ///     A BookScore.
        /// </returns>
        //public BookScore PredictScoreForGivenBook(List<UsersSimilarity> similarUsers, int userId, string bookId)
        //{
        //    if (similarUsers == null || similarUsers.Count == 0) return null;
        //    var meanRateForUser = _common.GetAverageRateForUser(userId) ?? 0;

        //    return EvaluateScore(similarUsers, bookId, meanRateForUser, userId);
        //}

        public List<UsersSimilarity> CombineUniqueAndMutualBooksForUser(List<UsersSimilarity> similarUsers)
        {
            foreach (var u in similarUsers)
                u.BooksUniqueForComparedUser =
                    u.BooksUniqueForComparedUser.Concat(u.ComparedUserRatesForMutualBooks).ToArray();

            return similarUsers;
        }

        public BookScore[] PredictScoreForAllUsersBooks(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksUserRead = _common.GetAllBooksUserReadWithScores(userId);

            var predictedScores = new BookScore[booksUserRead.Length];
            var meanRateForUser = _common.GetAverageRateForUser(userId) ?? 0;

            // we want to predict scores even for books that user already rated
            similarUsers = CombineUniqueAndMutualBooksForUser(similarUsers);
            for (var i = 0; i < booksUserRead.Length; i++)
            {
                var book = booksUserRead[i];
                predictedScores[i] = EvaluateScore(similarUsers, book, meanRateForUser);
            }
            
            return predictedScores;
        }

        /// <summary>
        ///     Evaluate score.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="bookScore"></param>
        /// <param name="meanRateForUser">The mean rate for user.</param>
        /// <returns>
        ///     A BookScore.
        /// </returns>
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
    }
}