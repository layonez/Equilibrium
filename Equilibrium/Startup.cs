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
			// Any connection or hub wire up and configuration should go here
			app.MapSignalR();
			GlobalHost.HubPipeline.RequireAuthentication();
		}
	}
}