using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using DataAccess;
using RecommendationEngine;
using RecommendationEngine.Properties;

namespace TestingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            var container = builder.ConfigureContainer();
            var runner = container.GetInstance<UserBasedCollaborativeFiltering>();
            var helper = container.GetInstance<CollaborativeFilteringHelpers>();


            const string path = @"..\..\ElapsedTime.csv";

           var parameters = helper.GetParametersFromSettingsOrDb( false, 2);

          //  var users1 = FindNearestNeighborsForUsers(settings, context, runner, path, error, 4);
      
            //    runner.InvokeScoreEvaluation(fromDbFlag, settings.Id, path, users1);
            

        runner.InvokeNearestNeighborsForUsersWhoRatedPopularBooks(parameters.BookPopularity, path, false, parameters.Id);

        }

        private static List<int> FindNearestNeighborsForUsers(ISettings settings, IDataManager context,
            UserBasedCollaborativeFiltering runner, string path, bool error, int settingId)
        {
            var parameters = settings.CreateParameterSetFromSettings();
            context.SaveParametersSet(parameters);
            List<int> users;
            try
            {
                Console.WriteLine($"Current setting num is {parameters.Id}");
                users = runner.InvokeNearestNeighbors(path, error, settingId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception{e}\n, inner ex {e.InnerException}\n, {e.Message}");
                return null;
            }

            return users;
        }

        private static List<int> FindNearestNeighborsForUsersWhoReadMostPopularBooks(ISettings settings, IDataManager context,
            UserBasedCollaborativeFiltering runner, string path, bool error, int numUsersWhoReadBook, int settingId)
        {
            var parameters = settings.CreateParameterSetFromSettings();
            context.SaveParametersSet(parameters);
            List<int> users;
            try
            {
                Console.WriteLine($"Current setting num is {parameters.Id}");
                users = runner.InvokeNearestNeighborsForUsersWhoRatedPopularBooks(numUsersWhoReadBook, path, error, settingId);
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
