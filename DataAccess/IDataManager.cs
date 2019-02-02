using System.Collections.Generic;

namespace DataAccess
{
    public interface IDataManager
    {
        BookScore[] GetBooksRatesByUserId(int userId);
        void AddRecommendedBooksForUser(BookScore[] books, int userId);

        Parameter GetParameters(int settingsVersion);
        void AddTestResults(List<BookScore[]> scores, int settingVersion);

        int[] GetAllUsersWithComputedSimilarity(int settingVersion);
        List<int> GetUserIdsWithNorMoreRatedBooks(int n);
        void AddSimilarUsers(List<UsersSimilarity> similarUsers, int settingsVersion);
        User[] GetAllUsersWithRatedBooks();

        double? GetAverageRateForUser(int userId);

        string[] GetBooksIdsRatedByAtLeastNUsers(string[] ids, int n);

        IEnumerable<string> GetBooksIdsRatedByAtLeastNUsers(int n);

        Book[] GetBooksReadByUser(User user);

        Book[] GetRecommendedBooksForUser(int userId);

        List<UserSimilar> GetUserNeighbors(int userId, int settingsVersion);

        void SaveParametersSet(Parameter parameter);

        List<int> GetUsersWhoRatedAnyOfGivenBooks(string[] bookIds);
    }
}