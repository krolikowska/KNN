using RecommendationEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestingConsoleApp
{
    class Program
    {
        private static CollaborativeFilteringHelpers _helper;
        private static IRecommendationEvaluator _evaluator;
        private static IUsersSelector _selector;
        private static ISettings _settings;
        private static readonly string deafultPath = @"..\..\UsersList.csv";
        const int noBooksRange = 200;
        const int neighborsRange = 200;

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            var container = builder.ConfigureContainer();
            _helper = container.GetInstance<CollaborativeFilteringHelpers>();
            _evaluator = container.GetInstance<IRecommendationEvaluator>();
            _selector = container.GetInstance<IUsersSelector>();
            _settings = container.GetInstance<ISettings>();


            if (args.Length < 1)
            {
                PrintIntro();
                return;
            }
            else
            {
                string UserId;
                switch (args[0])
                {
                    case "-user":

                        if (args.Length < 2)
                        {
                            Console.WriteLine("Provide user id");
                            UserId = Console.ReadLine();
                        }
                        else
                        {
                            UserId = args[1];
                        }
                        if (EvaluateScoreForUser(UserId) == false) return;
                        break;

                    case "-all-users":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Evaluate for parameters from config file");
                            PrintSettings();
                            EvaluateScoresForAllUsers(deafultPath);
                        }
                        else
                        {
                            if (int.TryParse(args[1], out int n) && int.TryParse(args[2], out int k))
                            {
                                EvaluateForSpecifiedParameters(n, k);
                            }
                            else
                            {
                                Console.WriteLine("Wrong user id");
                                return;
                            }
                        }
                        break;
                    case "-search":

                        if (args.Length < 2)
                        {
                            Console.WriteLine("Provide user id");
                            UserId = Console.ReadLine();
                        }
                        else
                        {
                            UserId = args[1];
                        }
                        if (EvaluateForVariousParametersSets(UserId) == false) return;
                        break;

                    default:
                        Console.WriteLine($"Invalid command: {args[0]}");
                        return;
                }
            }
        }

        private static void PrintIntro()
        {
            Console.WriteLine($"\nUse option: -user --id, -all-users, -all-users --N --K, --search");
            Console.WriteLine($"\n-user --id         Evaluate book scores for given user id and setup from app.config, errors are printed in console. Parameter --id must be a int value" +
                              $"\n-all-users         Evalute book scores for given users list from csv file, results are persited in csv file" +
                              $"\n-all-users --N --K Evaluate for given BooksRead (N) and NearestNeighbors (K)" +
                              $"\n-search --id       Evaluate book scores for given user id and all various parametes set, results are persited in csv file. Parameter --id must be a int value");
        }
        private static bool EvaluateScoreForUser(string UserId)
        {
            if (int.TryParse(UserId, out int id))
            {
                EvaluateScoreForUser(id);
                return true;
            }
            else
            {
                Console.WriteLine("Wrong user id");
                return false;
            }
        }
        private static bool EvaluateForVariousParametersSets(string UserId)
        {
            if (int.TryParse(UserId, out int id))
            {
                EvaluateForVariousParametersSets(id, UserId);
                return true;
            }
            else
            {
                Console.WriteLine("Wrong user id");
                return false;
            }
        }
        private static void EvaluateForSpecifiedParameters(int n, int k)
        {
            _settings.MinNumberOfBooksEachUserRated = n;
            _settings.NumOfNeighbors = k;

            PrintSettings();
            EvaluateScoresForAllUsers(deafultPath);
        }

        private static void EvaluateScoreForUser(int userId)
        {
            PrintSettings();
            var users = _selector.SelectUsersIdsToCompareWith(userId);
            var (mae, rsme) = _evaluator.EvaluateScoreForUserWithErrors(userId, _settings, users);

            Console.WriteLine($"N: {_settings.MinNumberOfBooksEachUserRated} K: {_settings.NumOfNeighbors}, MAE: {mae}, RSME: {rsme}");
        }

        private static void PrintSettings()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Parameters ID:\t{_settings.Id}\n Distance Type:\t{_settings.SimilarityDistance}\n K neighbors:\t{_settings.NumOfNeighbors}\n No of books each user at least rated:\t{_settings.MinNumberOfBooksEachUserRated}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void EvaluateScoresForAllUsers(string path)
        {
            var listOfErrors = new List<Tuple<int, int, int, double, double>>();
            var users = _helper.ReadFromCsv(path);
            var usersToCompare = _selector.SelectUsersIdsToCompareWith(_settings.MinNumberOfBooksEachUserRated);

            var counter = 0;
            try
            {
                foreach (var user in users)
                {
                    var (mae, rsme) = _evaluator.EvaluateScoreForUserWithErrors(user, _settings, usersToCompare);
                    listOfErrors.Add(new Tuple<int, int, int, double, double>(user,
                                                                              _settings.NumOfNeighbors,
                                                                              _settings.MinNumberOfBooksEachUserRated,
                                                                              mae,
                                                                              rsme));

                    counter++;
                    Console.WriteLine($"User {user} N: {_settings.MinNumberOfBooksEachUserRated} K: {_settings.NumOfNeighbors}, MAE: {mae}, RSME: {rsme}");
                    Console.WriteLine($" Progress {counter / users.Count * 100:P}");
                }
            }
            finally
            {
                _helper.SaveInCsvFile(listOfErrors,
                                      $"errors_K{_settings.NumOfNeighbors}_N{_settings.MinNumberOfBooksEachUserRated}");
            }
        }

        private static void EvaluateForVariousParametersSets(int userId, string name)
        {
            var listOfErrors = new List<Tuple<int, int, double, double>>();
            var stopWatch = new Stopwatch();

            const int loops = ((neighborsRange / 10) - 1) * (noBooksRange / 10);

            var counter = 0;
            var sum = 0L;

            Console.WriteLine($"Evaluate for N from 5 to {noBooksRange} and K from 20 to {neighborsRange} with iterator = 10");

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

                    var users = _selector.SelectUsersIdsToCompareWith(userId);
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