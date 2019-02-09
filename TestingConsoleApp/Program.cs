using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DataAccess;
using RecommendationEngine;
using SimpleInjector;

namespace TestingConsoleApp
{
    class Program
    {
        const string path = @"..\..\ElapsedTime.csv";

        static CollaborativeFilteringHelpers _helper;
        static IRecommendationEvaluator _evaluator;
        private static IUsersSelector _selector;
        static ISettings _settings;

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            var container = builder.ConfigureContainer();
            _helper = container.GetInstance<CollaborativeFilteringHelpers>();
            _evaluator = container.GetInstance<IRecommendationEvaluator>();
            _selector = container.GetInstance<IUsersSelector>();
            _settings = container.GetInstance<ISettings>();

            int[] n = {5, 30, 60};
            int[] k = {30, 30, 60};
        }

        private static void ComputeForDifferentSetup(int n, int k)
        {
            _settings.MinNumberOfBooksEachUserRated = n;
            _settings.NumOfNeighbors = k;

            ComputeForUsers();
        }

        private static void EvaluateScoreForUSer(int userId)
        {
            PrintSettings();
            var users = _selector.GetUsersWhoRatedAtLeastNBooks(_settings.MinNumberOfBooksEachUserRated);
            var (mae, rsme) = _evaluator.EvaluateScoreForUserWithErrors(userId, _settings, users);

            Console.WriteLine($"N: {_settings.MinNumberOfBooksEachUserRated} K: {_settings.NumOfNeighbors}, MAE: {mae}, RSME: {rsme}");
        }

        private static void PrintSettings()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Parameters ID:\t{_settings.Id}\n Distance Type:\t{_settings.SimilarityDistance}\n K neighbors:\t{_settings.NumOfNeighbors}\n No of books each user at least rated:\t{_settings.MinNumberOfBooksEachUserRated}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ComputeForUsers()
        {
            var listOfErrors = new List<Tuple<int, int, int, double, double>>();
            var users = _helper.ReadFromCsv(Properties.Resources.UsersList);
            Console.WriteLine($"Read {users.Count} users from file");
            var counter = 0;
            try
            {
                foreach (var user in users)
                {
                    var (mae, rsme) = _evaluator.EvaluateScoreForUserWithErrors(user, _settings, users);
                    listOfErrors.Add(new Tuple<int, int, int, double, double>(user,
                                                                              _settings.NumOfNeighbors,
                                                                              _settings.MinNumberOfBooksEachUserRated,
                                                                              mae,
                                                                              rsme));

                    counter++;
                    Console.WriteLine($"User {user} N: {_settings.MinNumberOfBooksEachUserRated} K: {_settings.NumOfNeighbors}, MAE: {mae}, RSME: {rsme}");
                    Console.WriteLine($" Progress {counter / users.Count:P}");
                }
            }
            finally
            {
                _helper.SaveInCsvFile(listOfErrors,
                                      $"errors_K{_settings.NumOfNeighbors}_N{_settings.MinNumberOfBooksEachUserRated}");
            }
        }

        private static void ComputeForDifferentParameters(int userId, string name)
        {
            var listOfErrors = new List<Tuple<int, int, double, double>>();
            var stopWatch = new Stopwatch();

            const int noBooksRange = 200;
            const int neighborsRange = 200;
            const int loops = (neighborsRange / 10 - 1) * (noBooksRange / 10);

            var counter = 0;
            var sum = 0L;

            try
            {
                for (var i = 0; i <= noBooksRange; i += 10)
                {
                    stopWatch.Start();
                    _settings.MinNumberOfBooksEachUserRated = i;

                    if (i == 0)
                    {
                        _settings.MinNumberOfBooksEachUserRated = 5;
                    }

                    var users = _selector.GetUsersWhoRatedAtLeastNBooks(_settings.MinNumberOfBooksEachUserRated);
                    Console.BackgroundColor = ConsoleColor.Black;

                    for (var j = 20; j <= neighborsRange; j += 10)
                    {
                        _settings.NumOfNeighbors = j;
                        var (mae, rsme) = _evaluator.EvaluateScoreForUserWithErrors(userId, _settings, users);
                        listOfErrors.Add(new Tuple<int, int, double, double>(_settings.NumOfNeighbors,
                                                                             _settings.MinNumberOfBooksEachUserRated,
                                                                             mae,
                                                                             rsme));
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.WriteLine($"N: {_settings.NumOfNeighbors} K: {_settings.NumOfNeighbors}, MAE: {mae}, RSME: {rsme}");
                        counter++;

                        stopWatch.Stop();
                        var elapsed = stopWatch;
                        sum += elapsed.ElapsedMilliseconds;
                        _helper.PrintStats(counter, loops, sum, userId);
                    }
                }
            }
            finally
            {
                _helper.SaveInCsvFile(listOfErrors, $"errors_{userId}_{name}");
            }
        }
    }
}