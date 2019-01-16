using System.Collections.Generic;

namespace DataAccess
{
    /// <summary>
    ///     Interface for data manager.
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        ///     Adds a recommended <paramref name="books" /> for user to 'userId'.
        /// </summary>
        /// <param name="books">The books.</param>
        /// <param name="userId">Identifier for the user.</param>
        void AddRecommendedBooksForUser(BookScore[] books, int userId);

        List<int> GetUsersIdsWithNorMoreRatedBooks(int n);
        List<SelectMutualBooks_Result> GetMutualBooksForUsers(int user1, int user2);

        /// <summary>
        ///     Adds a similar users.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="settingsVersion">The settings version.</param>
        void AddSimilarUsers(List<UsersSimilarity> similarUsers, int userId, int settingsVersion);

        /// <summary>
        ///     Gets all users with rated books.
        /// </summary>
        /// <returns>
        ///     An array of user.
        /// </returns>
        User[] GetAllUsersWithRatedBooks();

        /// <summary>
        ///     Gets average rate for all books.
        /// </summary>
        /// <returns>
        ///     The average rate for all books.
        /// </returns>
        double GetAverageRateForAllBooks();

        /// <summary>
        ///     Gets average rate for given book.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <returns>
        ///     The average rate for given book.
        /// </returns>
        double GetAverageRateForGivenBook(string isbn);

        /// <summary>
        ///     Gets average rate for user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     The average rate for user.
        /// </returns>
        double? GetAverageRateForUser(int userId);

        /// <summary>
        ///     Gets the books identifiers rated by at least nusers in this
        ///     collection.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <param name="n">An <see langword="int" /> to process.</param>
        /// <returns>
        ///     An enumerator that allows <see langword="foreach" /> to be used to
        ///     process the books identifiers rated by at least nusers in this
        ///     collection.
        /// </returns>
        string[] GetBooksIdsRatedByAtLeastNUsers(string[] ids, int n);

        /// <summary>
        ///     Gets the books identifiers rated by at least nusers in this
        ///     collection.
        /// </summary>
        /// <param name="n">An <see langword="int" /> to process.</param>
        /// <returns>
        ///     An enumerator that allows <see langword="foreach" /> to be used to
        ///     process the books identifiers rated by at least nusers in this
        ///     collection.
        /// </returns>
        IEnumerable<string> GetBooksIdsRatedByAtLeastNUsers(int n);

        /// <summary>
        ///     Gets books identifiers same for both users.
        /// </summary>
        /// <param name="user1">The first user.</param>
        /// <param name="user2">The second user.</param>
        /// <returns>
        ///     An array of string.
        /// </returns>
        string[] GetBooksIdsSameForBothUsers(int user1, int user2);

        /// <summary>
        ///     Gets books identifiers unique for second user.
        /// </summary>
        /// <param name="user1">The first user.</param>
        /// <param name="user2">The second user.</param>
        /// <returns>
        ///     An array of string.
        /// </returns>
        string[] GetBooksIdsUniqueForSecondUser(int user1, int user2);

        /// <summary>
        ///     Gets books rated by user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     An array of book.
        /// </returns>
        Book[] GetBooksRatedByUser(User user);

        /// <summary>
        ///     Gets books rates by user identifier.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book score.
        /// </returns>
        BookScore[] GetBooksRatesByUserId(int userId);

        /// <summary>
        ///     Gets books read by user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     An array of book.
        /// </returns>
        Book[] GetBooksReadByUser(User user);

        /// <summary>
        ///     Gets recomennded books for user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book.
        /// </returns>
        Book[] GetRecommendedBooksForUser(int userId);

        /// <summary>
        ///     Gets a user.
        /// </summary>
        /// <param name="UserId">Identifier for the user.</param>
        /// <returns>
        ///     The user.
        /// </returns>
        User GetUser(int UserId);

        /// <summary>
        ///     Gets the users.
        /// </summary>
        /// <param name="UserIds">
        ///     <see cref="List`1" /> of identifiers for the users.
        /// </param>
        /// <returns>
        ///     The users.
        /// </returns>
        List<User> GetUsers(int[] UserIds);

        /// <summary>
        ///     Gets users neighbors.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="settingsVersion">The settings version.</param>
        /// <returns>
        ///     The users neighbors.
        /// </returns>
        List<UserSimilar> GetUsersNeighbors(int userId, int settingsVersion);

        /// <summary>
        ///     Gets users rates for given <paramref name="isbn" /> list.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book score.
        /// </returns>
        BookScore[] GetUsersRatesForGivenIsbnList(string[] isbn, int userId);

        /// <summary>
        ///     Gets the users with nor more rated books in this collection.
        /// </summary>
        /// <param name="n">An <see langword="int" /> to process.</param>
        /// <returns>
        ///     An enumerator that allows <see langword="foreach" /> to be used to
        ///     process the users with nor more rated books in this collection.
        /// </returns>
        IEnumerable<User> GetUsersWithNorMoreRatedBooks(int n);

        /// <summary>
        ///     Saves the parameters set.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        void SaveParametersSet(Parameter parameter);

        /// <summary>
        ///     Gets users who rated given book.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <returns>
        ///     The users who rated given book.
        /// </returns>
        List<BooksRating> GetBookRatingsForUsersWhoRatedGivenBook(string isbn);

        int[] GetUsersWhoRatedAnyOfGivenBooks(string[] bookIds);
    }
}