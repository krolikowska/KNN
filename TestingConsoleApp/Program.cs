using System;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
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

            var container = builder.BuildContainer();

            var data = container.GetInstance<IDataManager>();
            var runner = container.GetInstance<UserBasedCollaborativeFiltering>();
            var context = container.GetInstance<IDataManager>();
            var settings = container.GetInstance<ISettings>();

           var par = data.GetParameters(2);
            

            string path = @"..\..\ElapsedTime.csv";
            bool error = false;
            bool fromDbFlag = false;

               var users1 = FindNearestNeighborsForUsers(settings, context, runner, path, error, 4);
            var users = FindNearestNeighborsForUsersWhoReadMostPopularBooks(settings, context, runner, path, error, settings.BookPopularityAmongUsers,settings.Id);

            runner.InvokeScoreEvaluation(fromDbFlag, settings.Id, path, users);

        }

        private static int[] FindNearestNeighborsForUsers(ISettings settings, IDataManager context,
            UserBasedCollaborativeFiltering runner, string path, bool error, int settingId)
        {
            var parameters = settings.CreateParameterSetFromSettings();
            context.SaveParametersSet(parameters);
            int[] users;
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

        private static int[] FindNearestNeighborsForUsersWhoReadMostPopularBooks(ISettings settings, IDataManager context,
            UserBasedCollaborativeFiltering runner, string path, bool error, int numUsersWhoReadBook, int settingId)
        {
            var parameters = settings.CreateParameterSetFromSettings();
            context.SaveParametersSet(parameters);
            int[] users;
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
