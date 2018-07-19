using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Fleet_WorkShop.Startup))]
namespace Fleet_WorkShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
