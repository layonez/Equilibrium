using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Oracle.ManagedDataAccess.Client;


namespace Equilibrium
{
	public class ClarityProvider
	{
		private string ClarityConnectionString { get; }

		public ClarityProvider()
		{
			ClarityConnectionString = ConfigurationManager.ConnectionStrings["clarity"].ConnectionString;
		}

		public GetDisbalanceResult GetDisbalance(IEnumerable<string> users)
		{
			var result = new GetDisbalanceResult()
			{
				Ok = false
			};

			if (users.Any())
			{
				users = users.Select(u => u.Replace("CROC\\", string.Empty));

				using (var con = new OracleConnection(ClarityConnectionString))
				{
					var sw = new Stopwatch();
					sw.Start();

					con.Open();
					var cmdText =
						$"SELECT * FROM CROC_DISBALANCE_PERIOD WHERE employeeclarityid IN ({string.Join(",", users.Select(u => "'" + u.ToLower() + "'"))})";
					var cmd =
						new OracleCommand(cmdText
							, con);
					using (var da = new OracleDataAdapter(cmd))
					{
						var dt = new DataTable();
						da.Fill(dt);

						foreach (DataRow row in dt.Rows)
						{
							result.Data.Add(new DisbalanceResult
							{
								Disbalance = Convert.ToInt32(row[1]),
								Login = (string) row[0]
							});
						}
					}

					sw.Stop();
					result.Elapsed = sw.Elapsed;
					result.Ok = true;
				}
			}
			else
			{
				result.Ok = true;
			}

			return result;
		}
	}

	public class GetDisbalanceResult
	{
		public bool Ok { get; set; } 
		public readonly List<DisbalanceResult> Data = new List<DisbalanceResult>();
		public TimeSpan Elapsed { get; set; }
	}
	public struct DisbalanceResult
	{
		public string Login { get; set; }
		public int Disbalance { get; set; }
	}
}