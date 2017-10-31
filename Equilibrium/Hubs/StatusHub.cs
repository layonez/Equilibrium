using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Equilibrium.Hubs
{
	[HubName("statusHub")]
	public class StatusHub : Hub
	{

		public override Task OnDisconnected(bool stopCalled)
		{
			UserConnectionManager.Remove(Context.User.Identity.Name, Context.ConnectionId);

			return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString(CultureInfo.InvariantCulture));
		}
		
		public override Task OnConnected()
		{
			UserConnectionManager.Add(Context.User.Identity.Name, Context.ConnectionId);

			return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString(CultureInfo.InvariantCulture));
		}

		public override Task OnReconnected()
		{
			return Clients.All.rejoined(Context.ConnectionId, DateTime.Now.ToString(CultureInfo.InvariantCulture));
		}

		public void Ping()
		{
			Clients.Caller.pong();
		}
	}
}