using Microsoft.Owin;
using Owin;
using System.Web.Http;

[assembly: OwinStartup(typeof(RecommendationAPI.Startup))]

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