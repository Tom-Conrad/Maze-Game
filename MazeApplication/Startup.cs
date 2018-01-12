using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MazeApplication.Startup))]
namespace MazeApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
