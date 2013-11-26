using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LBCAlerterWeb.Startup))]
namespace LBCAlerterWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
