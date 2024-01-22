using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace ArduinoHIDService
{
	public partial class WinEvent : Form
	{
		public WinEvent()
		{
			InitializeComponent();
		}

		public void StartUp()
		{
			var message = new Message() { Msg = WinEventCode.WM_DEVICECHANGE, WParam = WinEventCode.DBT_DEVICEARRIVAL };
			WndProc(ref message);
		}
		private void WinEvent_Load(object sender, EventArgs e)
		{

		}
		protected override void WndProc(ref Message m)
		{
			//Wait for your message here
			if (m.Msg != WinEventCode.WM_DEVICECHANGE || (m.WParam != WinEventCode.DBT_DEVICEREMOVECOMPLETE && m.WParam != WinEventCode.DBT_DEVICEARRIVAL))
			{ // note, DBT_DEVICEREMOVECOMPLETE && DBT_DEVICEARRIVAL are only sent if the device is serial/parallel device (this is what we want :D)	
				base.WndProc(ref m);
				return;
			}

			Program.Logger.Log(LogType.Verbose, $"Window message received {m.Msg} {m.LParam} {m.WParam}");

			Program.Pause = true;
			string[] ports = SerialPort.GetPortNames();

			foreach (var pair in Program.SerialPorts)
			{
				if (ports.Contains(pair.Key)) continue;
				// will do below if the serial device do not exist anymore
				if (pair.Value.IsOpen) pair.Value.Close();
				Program.SerialPorts.Remove(pair.Key);
			}

			Action<string> detect = async (string port) =>
			{
				if (ConfigManager.Config.DisableOverride.Contains(port)) return;

				ConfigChild config = ConfigManager.Config.GetConfigForPort(port);
				SerialPort sPort;
				bool shouldAdd = false;
				if (!Program.SerialPorts.ContainsKey(port))
				{
					sPort = new SerialPort(port, config.BaudRate, Parity.None, 8, StopBits.One);
					shouldAdd = true;
				}
				else sPort = Program.SerialPorts[port];
				sPort.ReadTimeout = config.DetectionTimeOut;

				string result = "No";
				try
				{
					if (!sPort.IsOpen)
					{
						sPort.Open();
						await Task.Delay(config.OnInsertPrepareTime);
					}

					sPort.WriteLine("AreYouClient"); // asking if the device is a client
					result = sPort.ReadLine().Trim();
				}
				catch (IOException)
				{
					Program.SerialPorts.Remove(sPort.PortName);
				}
				catch (Exception ex)
				{
					Program.Logger.Log(LogType.Error, ex);
					Program.SerialPorts.Remove(sPort.PortName);
				}

				bool isResultCorrect = result.Length >= 3 && result[^3..] == "Yes";
				if (!isResultCorrect)
				{
					Program.Logger.Log(LogType.Info, $"Device '{port}' is not client!");
					shouldAdd = false;
					sPort.Close();
					Program.SerialPorts.Remove(sPort.PortName);
				}
				if (shouldAdd) Program.SerialPorts.Add(port, sPort);

				if (isResultCorrect)
				{
					Program.Logger.Log(LogType.Info, $"Found client on '{port}'!");
					sPort.ReadTimeout = config.MainTaskTimeOut;

					if (!Program.Running.ContainsKey(port))
						Program.Running.Add(port, Task.Run(() => Program.SerialTask(port)));
					else if (Program.Running[port].IsCompleted)
						Program.Running[port] = Task.Run(() => Program.SerialTask(port));

					new ToastContentBuilder()
						.AddText("New Device Connected")
						.AddText($"A client on '{port}' has been found!")
						.Show();
				}
				Program.Logger.Log(LogType.Verbose, $"port: {port}, shouldAdd: {shouldAdd}, result: {result}, isResultCorrect: {isResultCorrect}");
			};

			List<Task> tasks = new();
			foreach (string p in ports) tasks.Add(Task.Run(() => detect(p)));
			foreach (Task t in tasks) t.Wait();
			Program.Pause = false;
			base.WndProc(ref m);
		}

		private void WinEvent_FormClosing(object sender, FormClosingEventArgs e)
		{
			ConfigManager.Save();
		}
	}
}
