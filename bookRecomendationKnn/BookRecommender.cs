///-------------------------------------------------------------------------------------------------
// file:	BookRecommender.cs
//
// summary:	Implements the book recommender class
///-------------------------------------------------------------------------------------------------

using DataAccess;
using RecommendationEngine.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecommendationEngine
{
    /// <summary>   A book recommender. </summary>
    public class BookRecommender : IBookRecommender
    {
        /// <summary>   Options for controlling the operation. </summary>
        private readonly ISettings _settings;

        /// <summary>   The common. </summary>
        private ICommon _common;

        /// <summary>   Number of books to recommends. </summary>
        private readonly int _numOfBooksToRecommend;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="settings"> Options for controlling the operation. </param>
        /// <param name="common">   The common. </param>
        ///-------------------------------------------------------------------------------------------------

        public BookRecommender(ISettings settings, ICommon common)
        {
            _settings = settings;
            _common = common;
            _numOfBooksToRecommend = settings.NumOfBooksToRecommend;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets recommended books. </summary>
        ///
        /// <param name="similarUsers"> The similar users. </param>
        /// <param name="userId">       Identifier for the user. </param>
        ///
        /// <returns>   An array of book score. </returns>
        ///-------------------------------------------------------------------------------------------------

        public BookScore[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = _common.PreparePotentionalBooksToRecommendation(similarUsers);
            var booksRates = PredictScore(similarUsers, userId, booksIds);
            var result = booksRates.Take(_numOfBooksToRecommend).ToArray();
            _common.PersistRecommendedBooksInDb(result, userId);
            return result.ToArray();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Predict score. </summary>
        ///
        /// <param name="similarUsers"> The similar users. </param>
        /// <param name="userId">       Identifier for the user. </param>
        /// <param name="booksIds">     List of identifiers for the books. </param>
        ///
        /// <returns>   A SortedSet&lt;BookScore&gt; </returns>
        ///-------------------------------------------------------------------------------------------------

        public SortedSet<BookScore> PredictScore(List<UsersSimilarity> similarUsers, int userId, string[] booksIds)
        {
            if (similarUsers?.Count == 0) return null;
            var recommendedBooks = new SortedSet<BookScore>(new BookScoreComparer());

            if (booksIds.Length == 0)
            {
                _common.PersistRecommendedBooksInDb(null, userId);
                return null;
            };

            var meanRateForUser = _common.GetAverageRateForUser(userId) ?? 0;

            foreach (var u in similarUsers)
            {
                u.AverageScoreForComparedUser = _common.GetAverageRateForUser(u.ComparedUserId);
            }

            for (int i = 0; i < booksIds.Length; i++)
            {
                var bookScore = EvaluateScore(similarUsers, booksIds[i], meanRateForUser);
                if (bookScore != null)
                    recommendedBooks.Add(bookScore);
            }
            return recommendedBooks;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Evaluate score. </summary>
        ///
        /// <param name="similarUsers">     The similar users. </param>
        /// <param name="bookId">           Identifier for the book. </param>
        /// <param name="meanRateForUser">  The mean rate for user. </param>
        ///
        /// <returns>   A BookScore. </returns>
        ///-------------------------------------------------------------------------------------------------

        private BookScore EvaluateScore(List<UsersSimilarity> similarUsers, string bookId, double meanRateForUser)
        {
            var scoreNominator = 0.0;
            var scoreDenominator = 0.0;

            foreach (var u in similarUsers)
            {
                var book = u.BooksUniqueForComparedUser.Where(x => x.BookId == bookId).FirstOrDefault();

                if (book != null)
                {
                    scoreNominator += (book.Rate - u.AverageScoreForComparedUser.Value) * u.Similarity.Value;
                    scoreDenominator += Math.Abs(u.Similarity.Value);
                }
            }

            if (scoreDenominator != 0)
            {
                var recommendedBook = new BookScore
                {
                    BookId = bookId,
                    PredictedRate = Math.Round((scoreNominator / scoreDenominator) + meanRateForUser, 0)
                };

                return recommendedBook;
            }

            return null;
        }
    }
}