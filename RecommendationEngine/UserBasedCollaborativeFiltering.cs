using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;

namespace RecommendationEngine
{
    public class UserBasedCollaborativeFiltering : IUserBasedCollaborativeFiltering
    {
        private readonly INearestNeighborsSearch _nearestNeighbors;
        private readonly IBookRecommender _recommender;
        private readonly IUsersSelector _selector;
        private readonly CollaborativeFilteringHelpers _helpers;

        public UserBasedCollaborativeFiltering(IBookRecommender recommender, INearestNeighborsSearch nearestNeighbors,
            CollaborativeFilteringHelpers helpers, IUsersSelector selector)
        {
            _recommender = recommender;
            _nearestNeighbors = nearestNeighbors;
            _helpers = helpers;
            _selector = selector;
        }

        public Book[] RecommendBooksForUser(int userId)
        {
            var users = _selector.SelectUsersIdsToCompareWith(userId);
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId, users);

            return _recommender.GetRecommendedBooks(similarUsers, userId);
            
        }

        public void InvokeScoreEvaluation(bool fromDbFlag, int settingId, string path, int[] users)
        {
            if (fromDbFlag)
            {
                users = _selector.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);
            }

            var times = EvaluateScores(settingId, users);
            _helpers.SaveTimesInCsvFile(times, path);
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
                                        _helpers.PrintStats(i, users.Length, sum, user);

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
            _helpers.PersistTestResults(scores, settingId);
            _helpers.PrintErrors(errorIds);

            return stopwatchValues;
        }

        public BookScore[] PredictScoresForBooksUserAlreadyRead(int userId, int settingsId)
        {
            var similarUsers = _selector.GetSimilarUsersFromDb(userId, settingsId);
            return _recommender.PredictScoreForAllUsersBooks(similarUsers, userId);
        }

        private void GetNearestNeighborsWithElapsedTime(List<int> userIds, string path, int settingsId)
        {
            var stopWatch = new Stopwatch();
            var stopwatchValues = new List<Tuple<int, long>>();
            var errorIds = new List<int>();
            var i = 0;
            var sum = 0L;

            Console.WriteLine($"Computing neighbors for userIds {userIds.Count}");
            var _ = new object();

            Parallel.ForEach(userIds, user =>
                                      {
                                          Interlocked.Increment(ref i);
                                          stopWatch.Start();
                                          _helpers.PrintStats(i, userIds.Count, sum, user);

                                          try
                                          {
                                              var temp = _nearestNeighbors.GetNearestNeighbors(user, userIds);
                                              _helpers.PersistSimilarUsersInDb(temp, settingsId);
                                          }
                                          catch (Exception e)
                                          {
                                              Console.WriteLine($"Exception for {user}, trace: {e.StackTrace}");
                                              errorIds.Add(user);
                                          }

                                          stopWatch.Stop();
                                          var elapsed = stopWatch.Elapsed;
                                          sum += elapsed.Milliseconds;

                                          Console.BackgroundColor = ConsoleColor.Black;
                                          Console.Clear();

                                          stopwatchValues.Add(new Tuple<int, long>(user, elapsed.Milliseconds));
                                      });

            _helpers.PrintErrors(errorIds);
            _helpers.SaveTimesInCsvFile(stopwatchValues, path);
        }

        public List<int> InvokeNearestNeighbors(string path, bool error, int settingId)
        {
            var users = _selector.GetUsersWhoRatedAtLeastNBooks();
            if (error)
            {
                var computedUsers = _selector.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);
                users = users.Except(computedUsers).ToList();
            }

            GetNearestNeighborsWithElapsedTime(users, path, settingId);
            return users;
        }

        public List<int> InvokeNearestNeighborsForUsersWhoRatedPopularBooks(int bookPopularity, string path,
            bool error, int settingId)
        {
            var users = _selector.GetMostActiveUsersWhoReadMostPopularBooks(bookPopularity, 5);
            if (error)
            {
                var computedUsers = _selector.GetListOfUsersWithComputedSimilarityForGivenSettings(settingId);
                users = users.Except(computedUsers).ToList();
            }

            GetNearestNeighborsWithElapsedTime(users, path, settingId);

            return users;
        }
    }
}