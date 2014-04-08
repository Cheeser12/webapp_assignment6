using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Quotations.Startup))]
namespace Quotations
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
