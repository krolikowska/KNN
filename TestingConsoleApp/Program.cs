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
            var selector = container.GetInstance<UsersSelector>();

            var parameters = helper.GetParametersFromSettingsOrDb(parametersFromDb: false);

            // FindNearestNeighborsForUsersWhoReadMostPopularBooks(runner, false, parameters);
        }

        private static List<int> FindNearestNeighborsForUsers(UserBasedCollaborativeFiltering runner, bool error,
            Parameter parameters)
        {
            List<int> users;
            try
            {
                users = runner.InvokeNearestNeighbors(path, error, parameters.Id, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception{e}\n, inner ex {e.InnerException}\n, {e.Message}");
                return null;
            }

            return users;
        }
    }
}