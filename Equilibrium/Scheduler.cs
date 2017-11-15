using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;
using Equilibrium.Hubs;
using Microsoft.AspNet.SignalR;

namespace Equilibrium
{
	public class Scheduler
	{
		private static List<Tuple<double, int, int>> _requestTimings = new List<Tuple<double, int, int>>();

		private ThresholdHelper ThresholdHelper { get; }
		private ClarityProvider ClarityProvider { get; }
		public Scheduler()
		{
			var path = AppDomain.CurrentDomain.BaseDirectory + "ThresholdConfig.xml";

			var serializer = new XmlSerializer(typeof(ThresholdHelper));

			var reader = new StreamReader(path);
			ThresholdHelper = (ThresholdHelper)serializer.Deserialize(reader);
			reader.Close();

			ClarityProvider = new ClarityProvider();	
		}

		public void Start()
		{
			Task.Delay(TimeSpan.FromMilliseconds(3000))
				.ContinueWith(task => GetDisbalanceAndSendToClients());
		}
		
		private void GetDisbalanceAndSendToClients()
		{
			if (_requestTimings.Count > 100)
			{
				LogHelper.LogStatistic(_requestTimings);
				_requestTimings.Clear();
			}
			var disbalance = ClarityProvider.GetDisbalance(UserConnectionManager.GetActiveUsers());
			if (disbalance.Ok && disbalance.Data.Any())
			{
				UserConnectionManager.SetDisbalance(disbalance.Data);
				UserConnectionManager.NotifyAll();
				var timeToNextCall = ThresholdHelper.GetTimeToNextCall(disbalance.Elapsed.TotalSeconds);
				Task.Delay(TimeSpan.FromMilliseconds(timeToNextCall))
					.ContinueWith(task => GetDisbalanceAndSendToClients());

				_requestTimings.Add(new Tuple<double, int, int>(disbalance.Elapsed.TotalSeconds, disbalance.Data.Count, timeToNextCall));
			}
			else
			{
				Task.Delay(TimeSpan.FromMilliseconds(1000))
					.ContinueWith(task => GetDisbalanceAndSendToClients());

				_requestTimings.Add(new Tuple<double, int, int>(0, 0, 0));
			}
		}
	}

	[XmlRoot(ElementName = "ThresholdHelper")]
	public class ThresholdHelper
	{
		/*
		 Время выполнения запроса ​​Время до следующего запроса
​                  t < 1 с			  1 мин
​			1 с < t < 5 с			  5 мин
​			5 с < t < 10 с​ 			  10 мин
			​10 с < t < 15 с			  15 мин
​			15 с < t < 30 c			  30 мин
​    ​		30 с < t				​ ​ 60 мин		 
			 */

		[XmlRoot(ElementName = "KeyValuePairOfInt32Int32")]
		public struct KeyValuePair<TK, TV>
		{
			private double _v1;
			private int _v2;

			public KeyValuePair(double v1, int v2) : this()
			{
				this._v1 = v1;
				this._v2 = v2;
			}
			[XmlElement(ElementName = "Threshold")]
			public TK Threshold { get; set; }
			[XmlElement(ElementName = "TimeToNextCall")]
			public TV TimeToNextCall { get; set; }
		}

		[XmlArray("ThresholdTimeToNextCallList")]
		[XmlArrayItem("ThresholdTimeToNextCallItem")]
		public List<KeyValuePair<int, int>> ThresholdTimeToNextCallList;

		public int GetTimeToNextCall(double currentCallElapsedTime)
		{
			var list =  ThresholdTimeToNextCallList.OrderBy(x => x.Threshold);
			if (currentCallElapsedTime > list.Last().Threshold)
			{
				return list.Last().TimeToNextCall;
			}
			else if(currentCallElapsedTime < list.First().Threshold)
			{
				return list.First().TimeToNextCall;
			}

			return list.First(x => x.Threshold < currentCallElapsedTime).TimeToNextCall;
		}
	}
}