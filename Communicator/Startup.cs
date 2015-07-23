using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Communicator.Startup))]
namespace Communicator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
