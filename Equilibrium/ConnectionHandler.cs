using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equilibrium.Hubs;
using Microsoft.AspNet.SignalR;

namespace Equilibrium
{
	public static class UserConnectionManager
	{
		private static readonly ConcurrentDictionary<string, UserData> ConnectedUsers = new ConcurrentDictionary<string, UserData>();
		private static StatusHub StatusHub;

		public static bool Add(string login, string connectionId, StatusHub statusHub)
		{
			var same = StatusHub == statusHub;
			StatusHub = statusHub;

			var success = false;
			if (!string.IsNullOrWhiteSpace(login) &&
			    login.Contains("CROC\\")
			)
			{
				login = login.Replace("CROC\\",string.Empty).ToLower();

				if (!ConnectedUsers.ContainsKey(login))
				{
					success = ConnectedUsers.TryAdd(login,new UserData(login) {ConnectionIds = new List<string>() { connectionId }});
				}
				else
				{
					while (ConnectedUsers.TryGetValue(login, out UserData oldData))
					{
						if (!oldData.ConnectionIds.Contains(connectionId))
						{
							var newIds = new List<string>() { connectionId };
							newIds.AddRange(oldData.ConnectionIds);
							var newData = new UserData(login) {ConnectionIds = newIds, NeedToNotify = true};
							success = ConnectedUsers.TryUpdate(login, newData, oldData);

							LogHelper.Log($"Новый коннект {login}-{connectionId} [{newIds.Count}]");
						}

						break;
					}
				}
				
				if (!success)
				{
					LogHelper.Log($"Неудачная попытка добавить юзера {login}-{connectionId}");
				}
			}

			return success;
		}

		public static bool Update(string login, string connectionId)
		{
			var success = false;
			if (!string.IsNullOrWhiteSpace(login) &&
			    login.Contains("CROC\\") &&
			    ConnectedUsers.ContainsKey(login)
			)
			{
				login = login.Replace("CROC\\", string.Empty).ToLower();

				while (ConnectedUsers.TryGetValue(login, out UserData oldData))
				{
					if (!oldData.ConnectionIds.Contains(connectionId))
					{
						var newData = new UserData(login) {ConnectionIds = new List<string>(){ connectionId }, Disbalance = null};
						newData.ConnectionIds.AddRange(oldData.ConnectionIds);

						success = ConnectedUsers.TryUpdate(login, newData, oldData);

						LogHelper.Log($"Переподлючён {login}-{connectionId}");
					}
					break;
				}
				if (!success)
				{
					LogHelper.Log($"Неудачная попытка обновить юзера {login}-{connectionId}");
				}
			}

			return success;
		}

		public static bool Remove(string login, string connectionId)
		{
			var success = false;
			if (!string.IsNullOrWhiteSpace(login) &&
			    login.Contains("CROC\\") &&
			    ConnectedUsers.ContainsKey(login)
			)
			{
				login = login.Replace("CROC\\", string.Empty).ToLower();

				while (ConnectedUsers.TryGetValue(login, out UserData oldData))
				{
					if (oldData.ConnectionIds.Contains(connectionId))
					{
						if(oldData.ConnectionIds.Count == 1)
							success = ((IDictionary<string, string>)ConnectedUsers).Remove(login);
						else
						{
							var newData = new UserData(login) {ConnectionIds = new List<string>(oldData.ConnectionIds)};
							newData.ConnectionIds.Remove(connectionId);

							success = ConnectedUsers.TryUpdate(login, newData, oldData);

							LogHelper.Log($"Закрыт коннект {login}-{connectionId}");
						}
					}
					break;
				}

				if (!success)
				{
					LogHelper.Log($"Неудачная попытка удаления юзера {login}");
				}
			}

			return success;
		}

		public static List<string> GetActiveUsers()
		{
			var logins = ConnectedUsers.Select(u => u.Key.Replace("CROC\\", string.Empty));
			return logins.ToList();
		}

		public static void SetDisbalance(List<DisbalanceResult> usersDisbalanceList)
		{
			foreach (var user in usersDisbalanceList)
			{
				while (ConnectedUsers.TryGetValue(user.Login, out UserData oldData))
				{
					oldData.NeedToNotify = oldData.Disbalance != user.Disbalance;
					oldData.Disbalance = user.Disbalance;
					break;
				}
			}
		}

		public static void NotifyAll()
		{
			var hubContext = GlobalHost.ConnectionManager.GetHubContext<StatusHub>();

			var userList = GetNotificationList();
			Parallel.ForEach(userList, data =>
			{
				foreach (var dataConnectionId in data.ConnectionIds)
				{
					hubContext.Clients.Client(dataConnectionId).SetDisbalance(data.Login,data.Disbalance);
				}
			});
		}

		private static List<UserData> GetNotificationList()
		{
			return ConnectedUsers.Where(u => u.Value.NeedToNotify)
				.Select(u => u.Value).ToList();
		}
	}
}