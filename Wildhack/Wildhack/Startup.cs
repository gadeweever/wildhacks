using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Wildhack.Startup))]
namespace Wildhack
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
