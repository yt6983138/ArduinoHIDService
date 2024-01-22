using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoHIDService;

public enum LogType
{
	Verbose,
	Info,
	Warning,
	Error
}
public sealed class Logger : IDisposable
{
	public List<string> AllLogs { get; private set; } = new();
	public string LatestLogMessage { get; private set; } = string.Empty;
	public string LatestLogMessageUnformatted { get; private set; } = string.Empty;
	public FileInfo? WriteTo { get; private set; }
	public FileStream? LogStream { get; private set; }
	public bool Verbose { get; private set; }

	/// <summary>
	/// The argument pass in is formatted message.
	/// </summary>
	public Action<string> OnLog { get; set; } = (Message) => { };
	public string MessageFormat { get; set; } = "[{0}] [{1}] {2}\n";
	public string ExceptionFormat { get; set; } = "{0}: {1}\nInner: {2}\nStack Trace:\n{3}";

	public Logger(FileInfo? writeTo, bool verbose)
	{
		WriteTo = writeTo;
		if (writeTo != null)
		{
			try
			{
				LogStream = new FileStream(writeTo.FullName, FileMode.Append, FileAccess.Write);
			}
			catch (Exception ex) { Log(LogType.Error, ex); }
		}
		else LogStream = null;
		Verbose = verbose;
	}
	public Dictionary<LogType, string> TypeToMessage { get; set; } = new()
	{
		{ LogType.Info, "Info" },
		{ LogType.Warning, "Warning" },
		{ LogType.Error, "Error" },
		{ LogType.Verbose, "Verbose" }
	};
	public void Log(LogType type, string message)
	{
		if (type == LogType.Verbose && !Verbose) return;
		string formatted = string.Format(MessageFormat, DateTime.Now, TypeToMessage[type], message);
		Console.Write(formatted);
		AllLogs.Add(formatted);
		LatestLogMessageUnformatted = message;
		LatestLogMessage = formatted;
		Write(formatted);
	}
	public void Log(LogType type, Exception ex)
	{
		if (type == LogType.Verbose && !Verbose) return;
		string compiled = string.Format(
			ExceptionFormat,
			ex.GetType().Name,
			ex.Message,
			ex.InnerException == null ? "Empty" : ex.InnerException.Message,
			ex.StackTrace
		);
		string formatted = string.Format(
			MessageFormat,
			DateTime.Now,
			TypeToMessage[type],
			compiled
		);
		Console.Write(formatted);
		AllLogs.Add(formatted);
		LatestLogMessageUnformatted = compiled;
		LatestLogMessage = formatted;
		Write(formatted);
	}
	private void Write(string message)
	{
		OnLog(message);
		if (LogStream == null) return;
		LogStream.Write(Encoding.UTF8.GetBytes(message));
	}
	public void Dispose()
	{
		LogStream?.Dispose();
	}
}
