﻿using System.Web.Http;
using DataAccess;
using RecommendationEngine;
using RecommendationEngine.Properties;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

namespace RecommendationAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register<IBookRecommender, BookRecommender>(Lifestyle.Scoped);
            container.Register<INearestNeighborsSearch, NearestNeighborsSearch>(Lifestyle.Scoped);
            container.Register<ISettings, Settings>(Lifestyle.Scoped);
            container.Register<IDataManager, DataManager>(Lifestyle.Scoped);
            container.Register<IUserBasedCollaborativeFiltering, UserBasedCollaborativeFiltering>(Lifestyle.Scoped);

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            container.Verify();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                                       "DefaultApi",
                                       "api/{controller}/{id}",
                                       new {id = RouteParameter.Optional}
                                      );
        }
    }
}