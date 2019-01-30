using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DataAccess;
using CsvHelper;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    public class CollaborativeFilteringHelpers
    {
        private readonly IDataManager _context;
        private readonly ISettings _settings;

        public CollaborativeFilteringHelpers(IDataManager context, ISettings settings)
        {
            _context = context;
            _settings = settings;
        }

        public Parameter GetParametersFromSettingsOrDb(bool parametersFromDb, int paramVersionInDb)
        {
            Parameter param;
            string text;
            if (parametersFromDb)
            {
                text = "database";
                param = _context.GetParameters(paramVersionInDb);
            }
            else
            {
               
                text = "settings";
                param = _settings.CreateParameterSetFromSettings();
                _context.SaveParametersSet(param);
            }
           

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
           
            Console.WriteLine($"Parameters from {text} with following setup:" +
                              $"\n Parameters ID:\t{param.Id}" +
                              $"\n Book popularity:\t{param.BookPopularity}" +
                              $"\n Distance Type:\t{(DistanceSimilarityEnum) param.DistanceType}" +
                              $"\n K neighbors:\t{param.Kneigbors}" +
                              $"\n No of books each user at least rated:\t{param.NumberOfBooksEachUserRated}");
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(3000);
            return param;

        }

        public void PrintErrors(List<int> errorIds)
        {
            Console.WriteLine($"Finished. There was {errorIds.Count} exceptions");
            foreach (var e in errorIds)
            {
                Console.WriteLine($"Exception for user {e}");
            }
        }

        public void PrintStats(double i, int length, long sum, int user)
        {
            var _ = new object();
            Monitor.Enter(_);
            var progress = i / length * 100.0;
            var average = sum / (i * 1000.0);
            var speed = average == 0 ? 0 : 1 / average;
            var remainingTime = (length - i) * speed / 1000.0;

            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{i} UserId {user}");
            Console.WriteLine($"Current progress is {progress:F}%");

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Evaluated in average\t{average:F}\tseconds");
            Console.WriteLine($"Remaining time \t{remainingTime:F}\tminutes");

            Monitor.Exit(_);
        }

        public void PersistTestResults(List<BookScore[]> books, int settingsId) =>
            _context.AddTestResults(books, settingsId);

        public void PersistSimilarUsersInDb(List<List<UsersSimilarity>> neighbors, int settingsId) =>
            _context.AddSimilarUsers(neighbors, settingsId); //problem z tym jest

        public void SaveTimesInCsvFile(List<Tuple<int, long>> elapsedTimes, string path)
        {
            using (var writer = new StreamWriter(@"..\..\elapsedTimes.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(elapsedTimes);
            }
        }
    }
}