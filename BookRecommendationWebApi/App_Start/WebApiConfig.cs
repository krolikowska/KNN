using System.Web.Http;
using DataAccess;
using RecommendationEngine;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

namespace RecommendationApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<IBookRecommender, BookRecommender>();
            container.Register<INearestNeighborsSearch, NearestNeighborsSearch>();
            container.Register<IUsersSelector, UsersSelector>();
            container.Register<ISettings, Settings>();
            container.Register<IDataManager, DataManager>();
            container.Register<IUserBasedCollaborativeFiltering, UserBasedCollaborativeFiltering>();
            container.Register<IRecommendationEvaluator, RecommendationEvaluator>();
            container.Register<CollaborativeFilteringHelpers>();

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            container.Verify();


            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
