using System.Web;
using System.IO;
using System.Web.Routing;
using Microsoft.Owin.Extensions;
using Microsoft.Owin;
using Owin;
using Equilibrium;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(Startup))]
namespace Equilibrium
{ 
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			LogHelper.Log($"Приложение запустилось");
			app.UseCors(CorsOptions.AllowAll);

			app.MapSignalR(new HubConfiguration()
			{
				EnableJSONP = true,
				EnableDetailedErrors = true,
				EnableJavaScriptProxies = true
			});
			GlobalHost.HubPipeline.RequireAuthentication();

			var scheduler = new Scheduler();
			scheduler.Start();

			LogHelper.Log($"Все службы запустились");

		}
	}
}