using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using NUnit.Framework;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    public class UserBasedCollaborativeFiltering : IUserBasedCollaborativeFiltering
    {
       
        private readonly INearestNeighborsSearch _nearestNeighbors;
        private readonly IBookRecommender _recommender;
        private readonly ICommon _common;

        public UserBasedCollaborativeFiltering(IBookRecommender recommender, INearestNeighborsSearch nearestNeighbors,
            ICommon common)
        {
            _recommender = recommender;
            _nearestNeighbors = nearestNeighbors;
            _common = common;
        }

        public BookScore[] RecommendBooksForUser(int userId)
        {
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId);

            return _recommender.GetRecommendedBooks(similarUsers, userId);
        }

        public void InvokeScoreEvaluation(bool fromDbFlag, int settingId, string path, int[] users)
        {
            if (fromDbFlag)
            {
                users = _common.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);
               
            }

            var times = EvaluateScores(settingId, users);
            _common.SaveTimesInCsvFile(times, path);
            
        }


        public List<Tuple<int, long>> EvaluateScores(int settingId, int[] users)
        {
            var scores = new List<BookScore[]>();
            var stopWatch = new Stopwatch();
            var stopwatchValues = new List<Tuple<int, long>>();
            var errorIds = new List<int>();
            

            var i = 1;

            Console.WriteLine($"Evaluating scores for {users.Length} users");

            var sum = 0L;
            Parallel.ForEach(users, user =>
                                    {
                                        PrintStats(i, users.Length, sum, user);

                                        stopWatch.Start();
                                        try
                                        {
                                            var temp = PredictScoresForBooksUserAlreadyRead(user, settingId);
                                            scores.Add(temp);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine($"Exception for {user} exception: {e.Message}");
                                            errorIds.Add(user);
                                        }

                                        stopWatch.Stop();
                                        var elapsed = stopWatch.Elapsed;
                                        sum += elapsed.Milliseconds;
                                        stopwatchValues.Add(new Tuple<int, long>(user, elapsed.Milliseconds));

                                        Interlocked.Increment(ref i);

                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.Clear();
                                    });

            Console.WriteLine($"Writing to database");
            _common.PersistTestResults(scores, settingId);
            PrintErrors(errorIds);

            return stopwatchValues;
        }

        private static void PrintStats(double i, int length, long sum, int user)
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

        public BookScore[] PredictScoresForBooksUserAlreadyRead(int userId, int settingsId)
        {
            var similarUsers = _nearestNeighbors.GetNearestNeighborsFromDb(userId, settingsId);
            return _recommender.PredictScoreForAllUsersBooks(similarUsers, userId);
        }

        private List<Tuple<int, long>> GetNearestNeighborsWithElapsedTime(IReadOnlyCollection<int> userIds)
        {
            var stopWatch = new Stopwatch();
            var stopwatchValues = new List<Tuple<int, long>>();
            var errorIds = new List<int>();
            var neighbors = new List<List<UsersSimilarity>>();
            var i = 0;
            var sum = 0L;

            Console.WriteLine($"Computing neighbors for userIds {userIds.Count}");

            Parallel.ForEach(userIds, user =>
                                      {
                                          Interlocked.Increment(ref i);
                                          PrintStats(i, userIds.Count, sum, user);
                                          stopWatch.Start();

                                          try
                                          {
                                             var temp = _nearestNeighbors.GetNearestNeighbors(user);
                                             neighbors.Add(temp);
                                          }
                                          catch (Exception e)
                                          {
                                              Console.WriteLine($"Exception for {user}");
                                              errorIds.Add(user);
                                          }

                                          stopWatch.Stop();
                                          var elapsed = stopWatch.Elapsed;
                                          sum += elapsed.Milliseconds;
                                         
                                          stopwatchValues.Add(new Tuple<int, long>(user, elapsed.Milliseconds));

                                          Console.BackgroundColor = ConsoleColor.Black;
                                          Console.Clear();
                                      });

            PrintErrors(errorIds);
            _common.PersistSimilarUsersInDb(neighbors);
            return stopwatchValues;
        }

        private static void PrintErrors(List<int> errorIds)
        {
            Console.WriteLine($"Finished. There was {errorIds.Count} exceptions");
            foreach (var e in errorIds)
            {
                Console.WriteLine($"Exception for user {e}");
            }
        }

        public int[] InvokeNearestNeighbors(string path, bool error, int settingId)
        {
            var users = _common.GetUsersWhoRatedAtLeastNBooks();
            if (error)
            {
                var computedUsers = _common.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);
                users = users.Except(computedUsers).ToArray();
            }

            var times = GetNearestNeighborsWithElapsedTime(users);
            _common.SaveTimesInCsvFile(times, path);
            return users;
        }

        public int[] InvokeNearestNeighborsForUsersWhoRatedPopularBooks(int numUsersWhoReadBook, string path,
            bool error, int settingId)
        {
            var users = _common.GetUsersWhoReadMostPopularBooks(numUsersWhoReadBook);
            if (error)
            {
                var computedUsers = _common.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);
                users = users.Except(computedUsers).ToArray();
            }

            var times = GetNearestNeighborsWithElapsedTime(users);
            _common.SaveTimesInCsvFile(times, path);
            return users;
        }
    }
}