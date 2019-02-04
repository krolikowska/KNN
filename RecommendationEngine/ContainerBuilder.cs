using DataAccess;
using RecommendationEngine.Properties;
using SimpleInjector;

namespace RecommendationEngine
{
    public class ContainerBuilder
    {
        public Container ConfigureContainer()
        {
            var container = new Container();

            container.Register<IBookRecommender, BookRecommender>();
            container.Register<INearestNeighborsSearch, NearestNeighborsSearch>();
            container.Register<IUsersSelector, UsersSelector>();
            container.Register<ISettings, Settings>();
            container.Register<IDataManager, DataManager>();
            container.Register<IUsersSimilarity, UsersSimilarity>();
            container.Register<IUserBasedCollaborativeFiltering, UserBasedCollaborativeFiltering>();
            container.Register<RecommendationEvaluator>();
            container.Register<CollaborativeFilteringHelpers>();
            container.GetInstance<UsersSimilarity>();

            return container;
        }
    }
}