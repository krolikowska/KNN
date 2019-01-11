﻿using DataAccess;
using RecommendationEngine.Properties;
using SimpleInjector;

namespace RecommendationEngine
{
    public class ContainerBuilder
    {
        public Container BuildContainer()
        {
            Container container = new Container();

            container.Register<IBookRecommender, BookRecommender>();
            container.Register<INearestNeighborsSearch, NearestNeighborsSearch>();
            container.Register<ICommon, Common>();
            container.Register<ISettings, Settings>();
            container.Register<IDataManager, DataManager>();
            container.Register<IUserBasedCollaborativeFiltering, UserBasedCollaborativeFiltering>();

            return container;
        }
    }
}