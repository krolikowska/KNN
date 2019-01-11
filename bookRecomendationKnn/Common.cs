///-------------------------------------------------------------------------------------------------
// file:	Common.cs
//
// summary:	Implements the common class
///-------------------------------------------------------------------------------------------------

using DataAccess;
using RecommendationEngine.Properties;
using System.Collections.Generic;
using System.Linq;

namespace RecommendationEngine
{
    /// <summary>   A common. </summary>
    public class Common : ICommon
    {
        /// <summary>   The dm. </summary>
        private IDataManager _dm;

        private ISettings _settings;
        private readonly int _minNumberOfBooksUserRated;
        private readonly int _minNumOfUsersWhoRatedBook;
        private readonly int _settingsVersion;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="settings">     Options for controlling the operation. </param>
        /// <param name="dataManager">  Manager for data. </param>
        ///-------------------------------------------------------------------------------------------------

        public Common(ISettings settings, IDataManager dataManager)
        {
            _dm = dataManager;
            _settings = settings;
            _minNumberOfBooksUserRated = _settings.MinNumberOfBooksEachUserRated;
            _minNumOfUsersWhoRatedBook = _settings.MinNumberOfBooksEachUserRated;
            _settingsVersion = _settings.Id;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Select users to compare with. </summary>
        ///
        /// <param name="userId">               Identifier for the user. </param>
        /// <param name="usersToCompareIds">    List of identifiers for the users to compares. </param>
        ///
        /// <returns>   A List&lt;User&gt; </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<User> SelectUsersToCompareWith(int userId, int[] usersToCompareIds)
        {
            var user = _dm.GetUser(userId);
            var usersToCompare = _dm.GetUsers(usersToCompareIds);
            if (user == null || usersToCompare == null || usersToCompare.Count == 0) return null;

            usersToCompare.Remove(user); //we won't compare with itself
            return usersToCompare;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets unique books identifiers. </summary>
        ///
        /// <param name="similarUsers"> The similar users. </param>
        ///
        /// <returns>   An array of string. </returns>
        ///-------------------------------------------------------------------------------------------------

        public string[] GetUniqueBooksIds(List<UsersSimilarity> similarUsers)
        {
            // unique list of books we recommend
            return similarUsers
                    .SelectMany(x => x.BooksUniqueForComparedUser
                    .Select(b => b.BookId))
                    .Distinct()
                    .ToArray();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Identify unique and mutual books for users. </summary>
        ///
        /// <param name="userSimilarFromDb">    The user similar from database. </param>
        ///
        /// <returns>   An UsersSimilarity. </returns>
        ///-------------------------------------------------------------------------------------------------

        public UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(UserSimilar userSimilarFromDb)
        {
            var similarity = IdentifyUniqueAndMutualBooksForUsers(userSimilarFromDb.UserId, userSimilarFromDb.NeighborId);
            similarity.Similarity = userSimilarFromDb.Similarity;

            return similarity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Identifief unique and mutual books for users. </summary>
        ///
        /// <param name="userId">           Identifier for the user. </param>
        /// <param name="comparedUserID">   Identifier for the compared user. </param>
        ///
        /// <returns>   An UsersSimilarity. </returns>
        ///-------------------------------------------------------------------------------------------------

        public UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(int userId, int comparedUserID)
        {
            var mutualBooksIds = _dm.GetBooksIdsSameForBothUsers(userId, comparedUserID);
            if (mutualBooksIds.Length == 0) return null;
            var uniqueBooks = _dm.GetBooksIdsUniqueForSecondUser(userId, comparedUserID);

            var similarity = new UsersSimilarity()
            {
                BooksUniqueForComparedUser = _dm.GetUsersRatesForGivenIsbnList(uniqueBooks, comparedUserID),
                UserRatesForMutualBooks = _dm.GetUsersRatesForGivenIsbnList(mutualBooksIds, userId),
                ComparedUserRatesForMutualBooks = _dm.GetUsersRatesForGivenIsbnList(mutualBooksIds, comparedUserID),
                UserId = userId,
                ComparedUserId = comparedUserID,
            };
            return similarity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Select users to compare with. </summary>
        ///
        /// <param name="userId">   Identifier for the user. </param>
        ///
        /// <returns>   A List&lt;User&gt; </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<User> SelectUsersToCompareWith(int userId)
        {
            var user = _dm.GetUser(userId);
            var usersToCompare = _dm.GetUsersWithNorMoreRatedBooks(_minNumberOfBooksUserRated).ToList();
            if (user == null || usersToCompare == null || usersToCompare.Count == 0) return null;

            usersToCompare.Remove(user); //we won't compare with itself
            return usersToCompare;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets similar users from database. </summary>
        ///
        /// <param name="userID">   Identifier for the user. </param>
        ///
        /// <returns>   The similar users from database. </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<UsersSimilarity> GetSimilarUsersFromDb(int userID)
        {
            var similarUsersFromDb = _dm.GetUsersNeighbors(userID, _settingsVersion);
            var similarUsers = new List<UsersSimilarity>();

            foreach (var similarity in similarUsersFromDb)
            {
                var temp = IdentifyUniqueAndMutualBooksForUsers(similarity);
                if (temp != null)
                    similarUsers.Add(temp);
            }

            return similarUsers;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   method for adding neighbors. </summary>
        ///
        /// <param name="books">    The books. </param>
        /// <param name="userId">   Identifier for the user. </param>
        ///-------------------------------------------------------------------------------------------------

        public void PersistRecommendedBooksInDb(BookScore[] books, int userId)
        {
            _dm.AddRecommendedBooksForUser(books, userId);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Persist similar users in database. </summary>
        ///
        /// <param name="neighbors">    The neighbors. </param>
        /// <param name="userId">       Identifier for the user. </param>
        ///-------------------------------------------------------------------------------------------------

        public void PersistSimilarUsersInDb(List<UsersSimilarity> neighbors, int userId)
        {
            _dm.AddSimilarUsers(neighbors, userId, _settingsVersion);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Prepare potentional books to recommendation. </summary>
        ///
        /// <param name="similarUsers"> The similar users. </param>
        ///
        /// <returns>   A string[]. </returns>
        ///-------------------------------------------------------------------------------------------------

        public string[] PreparePotentionalBooksToRecommendation(List<UsersSimilarity> similarUsers)
        {
            var booksIds = GetUniqueBooksIds(similarUsers); // we get list contains all books read by neighbors,
            if (_minNumOfUsersWhoRatedBook == 0)
            {
                return booksIds;
            }
            return _dm.GetBooksIdsRatedByAtLeastNusers(booksIds, _minNumOfUsersWhoRatedBook);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets average rate for user. </summary>
        ///
        /// <param name="userId">   Identifier for the user. </param>
        ///
        /// <returns>   The average rate for user. </returns>
        ///-------------------------------------------------------------------------------------------------

        public double? GetAverageRateForUser(int userId)
        {
            return _dm.GetAverageRateForUser(userId);
        }
    }
}