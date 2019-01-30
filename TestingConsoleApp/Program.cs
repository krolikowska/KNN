using System;
using System.Collections.Generic;
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

            
           var parameters = helper.GetParametersFromSettingsOrDb( false, 2);

           FindNearestNeighborsForUsersWhoReadMostPopularBooks(runner, false, parameters);


        }

     

        private static List<int> FindNearestNeighborsForUsers(UserBasedCollaborativeFiltering runner, bool error, Parameter parameters)
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

        private static List<int> FindNearestNeighborsForUsersWhoReadMostPopularBooks(UserBasedCollaborativeFiltering runner, bool error, Parameter parameters)
        {
           List<int> users;
            try
            {
                users = runner.InvokeNearestNeighborsForUsersWhoRatedPopularBooks(parameters.BookPopularity, path, error, parameters.Id);
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
