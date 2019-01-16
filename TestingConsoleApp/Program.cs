using System.Diagnostics;
using System.Linq;
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

            var recommnder = container.GetInstance<BookRecommender>();
            var nearestNeighbors = container.GetInstance<NearestNeighborsSearch>();
            var dm = container.GetInstance<DataManager>();
            var runner = container.GetInstance<UserBasedCollaborativeFiltering>();
            var settings = container.GetInstance<Settings>();
            var ev = container.GetInstance<RecommendationEvaluator>();

            string path = @"..\..\ElapsedTime.csv";

            runner.InvokeNearestNeighbors(path, 16943);
        }
    }
}
