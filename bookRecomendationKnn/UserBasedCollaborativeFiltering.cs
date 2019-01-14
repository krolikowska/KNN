using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public UserBasedCollaborativeFiltering(IBookRecommender recommender, INearestNeighborsSearch nearestNeighbors, ICommon common)
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

        public BookScore[] PredictScoresForBooksUserAlreadyRead(int userId)
        {
            Console.WriteLine("Get neighbors from DB");
            var similarUsers = _nearestNeighbors.GetNearestNeighborsFromDb(userId);
            Console.WriteLine($"Founded {similarUsers.Count} similar users");
            return _recommender.PredictScoreForAllUsersBooks(similarUsers, userId);
        }

        private List<Tuple<int, long>> GetNearestNeighborsWithElapsedTime(List<User> users)
        {
            var stopWatch = new Stopwatch();
            var stopwatchValues = new List<Tuple<int, long>>();
            var errorIds = new List<int>();

            Console.WriteLine($"Computing neighbors for users {users.Count}");
            foreach (var user in users)
            {
                stopWatch.Start();

                try
                {
                    _nearestNeighbors.GetNearestNeighbors(user.UserId);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception for {user.UserId}");
                    errorIds.Add(user.UserId);
                    continue;
                    
                }
                stopWatch.Stop();
                var elapsed = stopWatch.ElapsedMilliseconds / 1000;
                Console.WriteLine($"Computed neighbors for users {user.UserId} in {elapsed} seconds");
                stopwatchValues.Add(new Tuple<int, long>(user.UserId, elapsed));
                stopWatch.Reset();

            }


            Console.WriteLine($"Finished. There was {errorIds.Count} exceptions");
            foreach (var e in errorIds)
            {
                Console.WriteLine($"Exception for user {e}");
            }

            return stopwatchValues;
        }

        public void InvokeNearestNeighbors(string path, int userIdWhenError)
        {
            var users = _common.GetUsersWhoRatedAtLeastNBooks().Where(x => x.UserId > userIdWhenError).ToList();
            
            var times = GetNearestNeighborsWithElapsedTime(users);
           _common.SaveTimesInCsvFile(times, path);
        }


        public void InvokeNearestNeighborsForUsersWhoRatedPopularBooks(int numUsersWhoReadBook, string path)
        {
            var users = _common.GetUsersWhoReadMostPopularBooks(numUsersWhoReadBook);
            var times = GetNearestNeighborsWithElapsedTime(users);
            _common.SaveTimesInCsvFile(times, path);
        }


    }
}