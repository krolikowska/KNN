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
    /// <summary>
    ///     A user based collaborative filtering.
    /// </summary>
    public class UserBasedCollaborativeFiltering : IUserBasedCollaborativeFiltering
    {
        /// <summary>
        ///     The nearest neighbors.
        /// </summary>
        private readonly INearestNeighborsSearch _nearestNeighbors;

        /// <summary>
        ///     The recommender.
        /// </summary>
        private readonly IBookRecommender _recommender;

        private readonly ICommon _common;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="recommender">The recommender.</param>
        /// <param name="nearestNeighbors">The nearest neighbors.</param>
        public UserBasedCollaborativeFiltering(IBookRecommender recommender, INearestNeighborsSearch nearestNeighbors,
            ICommon common)
        {
            _recommender = recommender;
            _nearestNeighbors = nearestNeighbors;
            _common = common;
        }

        /// <summary>
        ///     Recommend books for user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     A BookScore[].
        /// </returns>
        public BookScore[] RecommendBooksForUser(int userId)
        {
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId);

            return _recommender.GetRecommendedBooks(similarUsers, userId);
        }

        public List<Tuple<int, long>> EvaluateScores(int settingId)
        {
            var scores = new List<BookScore[]>();
            var stopWatch = new Stopwatch();
            var stopwatchValues = new List<Tuple<int, long>>();
            var errorIds = new List<int>();
            var users = _common.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);

            var i = 1;
            
            Console.WriteLine($"Evaluating scores for {users.Length} users");
            var _ = new Object();

            Parallel.ForEach(users, user =>
                                    {
                                        var sum = 0L;
                                        lock (_)
                                        {
                                            PrintStats(i, users.Length, sum, user);
                                        }

                                        stopWatch.Start();
                                        try
                                        {
                                            var temp = PredictScoresForBooksUserAlreadyRead(user, settingId);
                                            scores.Add(temp);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine($"Exception for {user}");
                                            errorIds.Add(user);
                                        }

                                        
                                            stopWatch.Stop();
                                            var elapsed = stopWatch.Elapsed;
                                            Interlocked.Add(ref sum, elapsed.Milliseconds);
                                            stopwatchValues.Add(new Tuple<int, long>(user, elapsed.Milliseconds));
                                        
                                        
                                        Interlocked.Increment( ref i);
                                       
                                        stopWatch.Reset();
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
            var progress = i / length * 100.0;
            var average = sum / (i * 1000);
            var speed = 1 / average;
            var remainingTime = (length - i) * speed/1000;

            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{i} UserId {user}");
            Console.WriteLine($"Current progress is {progress:F}%");

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Evaluated in average\t{average:F}\tseconds");
            Console.WriteLine($"Remaining time \t{remainingTime:F}\tminutes");
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
            var i = 1.0;
            var sum = 0L;

            Console.WriteLine($"Computing neighbors for userIds {userIds.Count}");

            Parallel.ForEach(userIds, user =>
                                      {
                                          PrintStats(i, userIds.Count, sum, user);
                                          stopWatch.Start();

                                          try
                                          {
                                              _nearestNeighbors.GetNearestNeighbors(user);
                                          }
                                          catch (Exception e)
                                          {
                                              Console.WriteLine($"Exception for {user}");
                                              errorIds.Add(user);
                                          }

                                          stopWatch.Stop();
                                          var elapsed = stopWatch.Elapsed;
                                          sum += elapsed.Milliseconds;
                                          i++;
                                          stopwatchValues.Add(new Tuple<int, long>(user, elapsed.Milliseconds));
                                          stopWatch.Reset();
                                          Console.Clear();
                                      });


           

            PrintErrors(errorIds);

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