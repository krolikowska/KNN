using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine {
    public interface ICollaborativeFilteringHelpers {
        void PrintStats(double i, int length, long sum, int user);
        void PersistTestResult(List<BookScore> books, int settingsId);
        void SaveSettings(int settingsId);
        void PersistSimilarUsersInDb(List<UsersSimilarity> neighbors, int settingsId);
        List<int> ReadFromCsv(string path);
        void SaveInCsvFile<T>(List<T> errors, string fileName);
    }
}