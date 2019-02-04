using System;
using System.Collections.Generic;
using System.Threading;
using DataAccess;
using RecommendationEngine;

namespace TestingConsoleApp
{
    class Program
    {
        const string path = @"..\..\ElapsedTime.csv";

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            var container = builder.ConfigureContainer();
            var runner = container.GetInstance<UserBasedCollaborativeFiltering>();
            var helper = container.GetInstance<CollaborativeFilteringHelpers>();

            var parameters = helper.GetParametersFromSettingsOrDb(parametersFromDb: false);

            FindNearestNeighborsForUsersWhoReadMostPopularBooks(runner, false, parameters);
        }

        private static List<int> FindNearestNeighborsForUsers(UserBasedCollaborativeFiltering runner, bool error,
            Parameter parameters)
        {
            List<int> users;
            try
            {
                users = runner.InvokeNearestNeighbors(path, error, parameters.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception{e}\n, inner ex {e.InnerException}\n, {e.Message}");
                return null;
            }

            return users;
        }

        private static void FindNearestNeighborsForUsersWhoReadMostPopularBooks(
            UserBasedCollaborativeFiltering runner,
            bool error,
            Parameter parameters)
        {
            try
            {
                runner.InvokeNearestNeighborsForUsersWhoRatedPopularBooks(parameters.BookPopularity,
                                                                          path,
                                                                          error,
                                                                          parameters.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception{e}\n, inner ex {e.InnerException}\n, {e.Message}");
                Thread.Sleep(2000);
            }
        }
    }
}