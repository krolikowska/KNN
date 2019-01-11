using System.Collections.Generic;

namespace bookRecomendationKnn
{
    public interface IDataManager
    {
        UserInfo CreateUserInfo(int userId);

        List<User> GetAllUsersWithRatedBooks();

        List<User> GetUsersWithMoreThanNratedBooks(int n);

        string[] GetBooksIdsSameForBothUsers(int user1, int user2);
        string[] GetBooksIdsUniqueForSecondUser(int user1, int user2);

        List<Book> GetBooksRatedByUser(User user);

        List<BookRates> GetBooksRatesByUserId(int userId);

        List<Book> GetBooksReadByUser(User user);

        void GetStDevScore(out double mean, out double stdDev);

        List<BookRates> GetUsersRatesForGivenIsbnList(string[] isbn, int userId);
        double GetAverageRateForUser(int userId);

        void AddSimilarUsers(List<UsersSimilarity> similarUsers, int userId);
        void AddRecommendedBooksForUser(List<BookRates> books, int userId);

        List<Book> GetRecomenndedBooksForUser(int userId);
    }
}