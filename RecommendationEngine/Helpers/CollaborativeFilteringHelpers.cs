using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DataAccess;
using CsvHelper;

namespace RecommendationEngine
{
    public class CollaborativeFilteringHelpers : ICollaborativeFilteringHelpers
    {
        private readonly IDataManager _context;
        private readonly ISettings _settings;

        public CollaborativeFilteringHelpers(IDataManager context, ISettings settings)
        {
            _context = context;
            _settings = settings;
        }
    
        public void PrintStats(double i, int length, long sum, int user)
        {
            var _ = new object();
            var progress = i / length * 100.0;
            var average = sum / (i * 1000.0);
            var remainingTime = (length - i) * average / 60.0;

            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{i} UserId {user}");
            Console.WriteLine($"Current progress is {progress:F}%");

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Evaluated in average\t{average:F}\tseconds");
            Console.WriteLine($"Remaining time \t{remainingTime:F}\tminutes");
        }

        public void PersistTestResult(List<BookScore> books, int settingsId)
        {
            _context.AddTestResult(books, settingsId);
        }

        public void SaveSettings(int settingsId)
        {
            var param = _context.GetParameters(settingsId);
            if (param != null) return;
            param = _settings.CreateParameterSetFromSettings();
            _context.SaveParametersSet(param);
        }

        public void PersistSimilarUsersInDb(List<UsersSimilarity> neighbors, int settingsId) =>
            _context.AddSimilarUsers(neighbors, settingsId);

        public List<int> ReadFromCsv(string path)
        {
            var result = new List<int>();
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                result = csv.GetRecords<int>().ToList();
            }
            Console.WriteLine($"Read {result.Count} records from file");
            return result;
        }

        public void SaveInCsvFile<T>(List<T> errors, string fileName)
        {
            using (var writer = new StreamWriter($@"..\..\{fileName}.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(errors);
            }
        }
    }
}