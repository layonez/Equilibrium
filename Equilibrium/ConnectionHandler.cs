using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Equilibrium
{
	public static class UserConnectionManager
	{
		private static readonly ConcurrentDictionary<string, List<string>> ConnectedUsers = new ConcurrentDictionary<string, List<string>>();

		public static bool Add(string login, string connectionId)
		{
			var success = false;
			if (!string.IsNullOrWhiteSpace(login) &&
			    login.Contains("CROC\\")
			)
			{
				if (!ConnectedUsers.ContainsKey(login))
				{
					success = ConnectedUsers.TryAdd(login, new List<string>() { connectionId });
				}
				else
				{
					while (ConnectedUsers.TryGetValue(login, out List<string> oldIds))
					{
						if (!oldIds.Contains(connectionId))
						{
							var newIds = new List<string>() { connectionId };
							newIds.AddRange(oldIds);

							success = ConnectedUsers.TryUpdate(login, newIds, oldIds);
						}
						break;
					}
				}
				
				if (!success)
				{
					LogHelper.LogMessage($"Неудачная попытка добавить юзера {login}-{connectionId}");
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
				while (ConnectedUsers.TryGetValue(login, out List<string> oldIds))
				{
					if (!oldIds.Contains(connectionId))
					{
						var newIds = new List<string>(){ connectionId };
						newIds.AddRange(oldIds);

						success = ConnectedUsers.TryUpdate(login, newIds, oldIds);
					}
					break;
				}
				if (!success)
				{
					LogHelper.LogMessage($"Неудачная попытка обновить юзера {login}-{connectionId}");
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
				while (ConnectedUsers.TryGetValue(login, out List<string> oldIds))
				{
					if (oldIds.Contains(connectionId))
					{
						if(oldIds.Count == 1)
							success = ((IDictionary<string, string>)ConnectedUsers).Remove(login);
						else
						{
							var newIds = new List<string>(oldIds);
							newIds.Remove(connectionId);

							success = ConnectedUsers.TryUpdate(login, newIds, oldIds);
						}
					}
					break;
				}

				if (!success)
				{
					LogHelper.LogMessage($"Неудачная попытка удаления юзера {login}");
				}
			}

			return success;
		}
	}
}