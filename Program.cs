using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO.Ports;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ArduinoHIDService;

internal class Program
{
	public volatile static bool Pause = false;
	public volatile static bool ShouldCheckSerialPort = false;
	public volatile static Dictionary<string, SerialPort> SerialPorts = new();
	public volatile static Dictionary<string, Task> Running = new();
	public static Logger Logger { get; set; } = new(new FileInfo(ConfigManager.Config.LogLocation), ConfigManager.Config.Verbose);

	[STAThread]
	static void Main()
	{
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		WinEvent app = new();
		Application.Run(app);

		app.Load += (object? _, EventArgs _2) => app.StartUp();
	}
	public static async void SerialTask(string name)
	{
		SerialPort serialPort = SerialPorts[name]; // assume it have already opened
		int delay = ConfigManager.Config.ReadDelay;
		while (true)
		{
			await Task.Delay(delay);
			while (Pause) await Task.Delay(delay * 8);
			try
			{
				if (!serialPort.IsOpen) return;
				string received = serialPort.ReadLine();						// format: "operation|arg1|arg2..."
				Logger.Log(LogType.Verbose, $"[{name}] {received.TrimEnd()}");	// keyboard: "KeyboardEvent|Vk|PressOrRelease(true/false)|bScan"
				string[] first = received.Trim().Split('|');					// mouse: "MouseEvent|FlagName|posX|posY"
				if (first[0] == "KeepAlive")
				{
					continue;
				}
				else if (first[0] == "MouseEvent")
				{
					HIDOperationsHelper.MouseEvent(
						Utils.ParseEnum<HIDOperationsHelper.MouseEventFlags>(first[1]),
						int.Parse(first[2]),
						int.Parse(first[3])
					);
				}
				else if (first[0] == "KeyboardEvent")
				{
					HIDOperationsHelper.KeyboardEvent(
						HIDOperationsHelper.KeycodeTo_bVk[first[1]],
						bool.Parse(first[2]),
						byte.Parse(first[3])
					);
				}
				else throw new Exception($"Unable to parse \"{received.Trim()}\"");
				serialPort.WriteLine("Ok");
			}
			catch (TimeoutException)
			{
				Logger.Log(LogType.Warning, $"{serialPort.PortName} Timing out!");
			}
			catch (UnauthorizedAccessException) {
				Logger.Log(LogType.Warning, $"Device '{name}' disconnected or program not permitted to read COMs.");
				goto SendDisconnectToast;
			}
			catch (Exception e)
			{
				Logger.Log(LogType.Error, e);
				goto SendDisconnectToast;
			}
		}
	SendDisconnectToast:
		new ToastContentBuilder()
			.AddText("Device Disconnected")
			.AddText($"The device {name} has disconnected (or exception happened).")
			.Show();
		return;
	}
}