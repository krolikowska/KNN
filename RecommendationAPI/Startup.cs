using System.Web.Http;
using Microsoft.Owin;
using Owin;
using RecommendationAPI;

[assembly: OwinStartup(typeof(Startup))]

namespace RecommendationAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWebApi(GlobalConfiguration.Configuration);
        }
    }
}