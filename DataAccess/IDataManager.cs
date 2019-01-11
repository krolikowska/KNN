using System.Collections.Generic;

namespace DataAccess
{
    public interface IDataManager
    {
        void AddRecommendedBooksForUser(BookScore[] books, int userId);

        void AddSimilarUsers(List<UsersSimilarity> similarUsers, int userId, int settingsVersion);

        User[] GetAllUsersWithRatedBooks();

        double GetAverageRateForAllBooks();

        double GetAverageRateForGivenBook(string isbn);

        double? GetAverageRateForUser(int userId);

        string[] GetBooksIdsRatedByAtLeastNusers(string[] ids, int n);

        string[] GetBooksIdsSameForBothUsers(int user1, int user2);

        string[] GetBooksIdsUniqueForSecondUser(int user1, int user2);

        Book[] GetBooksRatedByUser(User user);

        BookScore[] GetBooksRatesByUserId(int userId);

        Book[] GetBooksReadByUser(User user);

        Book[] GetRecomenndedBooksForUser(int userId);

        User GetUser(int UserId);

        List<User> GetUsers(int[] UserIds);

        List<UserSimilar> GetUsersNeighbors(int userId, int settingsVersion);

        BookScore[] GetUsersRatesForGivenIsbnList(string[] isbn, int userId);

        IEnumerable<User> GetUsersWithNorMoreRatedBooks(int n);

        void SaveParametersSet(Parameter parameter);
    }
}