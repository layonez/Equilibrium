using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Equilibrium
{
	public static class LogHelper
	{
		private static readonly string LogFileName = AppDomain.CurrentDomain.BaseDirectory + "log.txt";
		private static readonly string statisticsFileName = AppDomain.CurrentDomain.BaseDirectory + "statistics.txt";


		public static void Log(string logMessage)
		{
			using (var w = File.AppendText(LogFileName))
			{
				LogMessage(logMessage, w);
			}
		}
		private static void LogMessage(string logMessage, TextWriter w)
		{
			w.WriteLine("{0} {1}   :   {2}", DateTime.Now.ToLongTimeString(),
				DateTime.Now.ToLongDateString(), logMessage);
		}

		public static void LogStatistic(List<Tuple<double, int, int>> requestTimings)
		{
			try
			{
				var statisticLine =
					$"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()} : Выполненно запросов - {requestTimings.Count}; Среднее время запроса - {requestTimings.Sum(t => t.Item1) / requestTimings.Count}; Среднее время ожидания - {requestTimings.Sum(t => t.Item3) / requestTimings.Count}; Среднее колличество активных пользователей - {requestTimings.Sum(t => t.Item2) / requestTimings.Count};";
				var txtfile = new FileInfo(statisticsFileName);
				if (!txtfile.Exists)
				{
					File.Create(statisticsFileName);
					txtfile = new FileInfo(statisticsFileName);

					Log($"Создан файл статистики {statisticsFileName}");
				}
				if (txtfile.Length > (20 * 1024 * 1024))       // ## NOTE: 20MB max file size
				{
					var lines = File.ReadAllLines(statisticsFileName).Skip(10).ToList();
					lines.Add(statisticLine);
					File.WriteAllLines(statisticsFileName, lines);
				}
				else
				{
					File.AppendAllLines(statisticsFileName, new List<string>() { statisticLine });
				}
			}
			catch (Exception e)
			{
				Log($"Во время записи статистики в файл {statisticsFileName} возникла ошибка: {e}");
			}
		}
	}
}