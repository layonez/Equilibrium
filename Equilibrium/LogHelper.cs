using System;
using System.IO;
using System.Reflection;

namespace Equilibrium
{
	public static class LogHelper
	{
		private static readonly string Filename = AppDomain.CurrentDomain.BaseDirectory + "log.txt";


		public static void Log(string logMessage)
		{
			using (var w = File.AppendText(Filename))
			{
				LogMessage(logMessage, w);
			}
		}
		private static void LogMessage(string logMessage, TextWriter w)
		{
			w.Write("\r\nLog Entry : ");
			w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
				DateTime.Now.ToLongDateString());
			w.WriteLine("  :{0}", logMessage);
			w.WriteLine("-------------------------------");
		}
	}
}