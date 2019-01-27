using System.Collections.Generic;

namespace DataAccess
{
    public interface IDataManager
    {

        BookScore[] GetBooksRatesByUserId(int userId);
        void AddRecommendedBooksForUser(BookScore[] books, int userId);

        Parameter GetParameters(int settingsVersion);
        void AddTestResults(List<BookScore[]> scores, int settingVersion);

        int[] GetUsersWithComputedSimilarity(int settingVersion);
        List<int> GetUsersIdsWithNorMoreRatedBooks(int n);
      void AddSimilarUsers(List<List<UsersSimilarity>> similarUsers, int settingsVersion);

        User[] GetAllUsersWithRatedBooks();

        double? GetAverageRateForUser(int userId);

        string[] GetBooksIdsRatedByAtLeastNUsers(string[] ids, int n);

        IEnumerable<string> GetBooksIdsRatedByAtLeastNUsers(int n);

        Book[] GetBooksReadByUser(User user);

        Book[] GetRecommendedBooksForUser(int userId);

        List<UserSimilar> GetUsersNeighbors(int userId, int settingsVersion);

       // IEnumerable<User> GetUsersWithNorMoreRatedBooks(int n);

        void SaveParametersSet(Parameter parameter);
        List<BooksRating> GetBookRatingsForUsersWhoRatedGivenBook(string isbn);

        int[] GetUsersWhoRatedAnyOfGivenBooks(string[] bookIds);
    }
}