using System.Collections.Generic;

namespace Equilibrium
{
	public class UserData
	{
		public UserData(string login)
		{
			Login = login;
		}

		public List<string> ConnectionIds = new List<string>();
		public int? Disbalance;
		public bool NeedToNotify;
		public string Login { get; }
	}
}