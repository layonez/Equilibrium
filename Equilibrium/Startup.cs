using System.Web;
using System.IO;
using Microsoft.Owin.Extensions;
using Microsoft.Owin;
using Owin;
using Equilibrium;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(Startup))]
namespace Equilibrium
{ 
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			LogHelper.Log($"Приложение запустилось");

			app.MapSignalR();
			GlobalHost.HubPipeline.RequireAuthentication();

			var scheduler = new Scheduler();
			scheduler.Start();

			LogHelper.Log($"Все службы запустились");

		}
	}
}