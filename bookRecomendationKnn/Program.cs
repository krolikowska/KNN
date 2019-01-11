using DataAccess;
using RecommendationEngine.Properties;
using System;

namespace RecommendationEngine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            var container = builder.BuildContainer();

            var recommnder = container.GetInstance<BookRecommender>();
            var nearestNeighbors = container.GetInstance<BookRecommender>();
            var dm = container.GetInstance<DataManager>();
            var runner = container.GetInstance<UserBasedCollaborativeFiltering>();
            var settings = container.GetInstance<Settings>();
            var user = dm.GetUser(17);

            Console.WriteLine($" User {user.UserId}, alg started ");

            var books = runner.RecommendBooksForUser(17);

            foreach (var b in books)
            {
                Console.WriteLine($" Book  {b.BookId}, predicted score: {b.PredictedRate}");
            }

            Console.WriteLine($" books {books.Length}");

            //var parameter = set.CreateParameterSetFromSettings();
            //dm.SaveParametersSet(parameter);

            //NearestNeighborsSearch en = new NearestNeighborsSearch(dm, set);

            //var neighbors = Settings.Default.NumOfNeighbors;

            //     var users = dm.GetAllUsersWithRatedBooks();

            //    en.GetNearestNeighbors(1);
            //        var numberOfBooksEachUserRated ;= set.numOfBooksToRecommend;
            //      var distance = set.SimilarityDistance;

            //// for prediction
            //var numOfBooksToRecommend = set.numOfBooksToRecommend;
            //var bookPopularity = set.BookPopularityAmongUsers;

            //Stopwatch stopWatch = new Stopwatch();

            //Console.WriteLine($"started {distance}");
            //stopWatch.Start();

            //foreach (var user in users)
            //{
            //    Console.WriteLine($" Getting neigbors for user {user}");
            //    en.GetNearestNeighbors(user.UserId, users);
            //}

            ////  var similarUsers = en.GetKNN(user, distance, neighbors, numberOfBooksEachUserRated);
            //stopWatch.Stop();
            //Console.WriteLine($"Knn took {stopWatch.ElapsedMilliseconds / 1000} sec");

            //  stopWatch.Reset();
            // stopWatch.Start();
            //Console.WriteLine("recommendation");

            // var rec = en.PredictRecommendBooks(similarUsers, user, numOfBooksToRecommend, distance, books);
            //   var similarUsers = en.GetSimilarUsersFromDb(user.UserId, distance);
            //var books = en.PreparePotentionalBooksToRecommendation(similarUsers, bookPopularity);

            //  var rec = en.PredictRecommendBooks(similarUsers, user, numOfBooksToRecommend, distance, books);
            //  stopWatch.Stop();
            Console.WriteLine("finished");
            //  Console.WriteLine($"Recommendadtion took {stopWatch.ElapsedMilliseconds / 1000} sec");

            Console.ReadLine();
        }
    }
}