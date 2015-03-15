using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace Dataway.DataAccess
{
	/// <summary>
	/// Used for logging database actively details
	/// </summary>
	public static class Logger
	{
		/// <summary>
		/// Gets or sets the start date time
		/// </summary>
		public static DateTime Start { get; set; }

		/// <summary>
		/// Gets or sets if logging is used
		/// </summary>
		public static bool HasLogging
		{
			get
			{
				return ConfigurationManager.AppSettings.AllKeys.Contains("DbLogFilename");
			}
		}

		/// <summary>
		/// Gets or sets whether to log everything or just errors
		/// </summary>
		public static bool LogEverything
		{
			get
			{
				return ConfigurationManager.AppSettings.AllKeys.Contains("DbLogEverything");
			}
		}

		/// <summary>
		/// Log details into the log file
		/// </summary>
		/// <param name="log">Log content</param>
		public static void Log(string log)
		{
			if (HasLogging)
			{
				string path = ConfigurationManager.AppSettings["DbLogFilename"]; //"c:\\dal-log.txt";

				string logLine = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff tt") + " - " + log;
				
				if (!File.Exists(path))
				{
					using (StreamWriter writer = File.CreateText(path))
					{
						writer.WriteLine(logLine);
					}
				}
				else
				{
					using (StreamWriter writer = File.AppendText(path))
					{
						writer.WriteLine(logLine);
					}
				}
			}
		}
	}
}
