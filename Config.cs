using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArduinoHIDService;
public class Config
{
	public ConfigChild Default { get; set; } = new();
	public Dictionary<string, ConfigChild> OverrideForSerialPort { get; set; } = new();
	public List<string> DisableOverride { get; set; } = new();
	public bool Verbose { get; set; } = true;
	public string LogLocation { get; set; } = "./Latest.log";
	public int ReadDelay { get; set; } = 32;

	public ConfigChild GetConfigForPort(string name)
	{
		if (OverrideForSerialPort.TryGetValue(name, out ConfigChild? value)) return value;
		return Default;
	}
}
public class ConfigChild
{
	public int BaudRate { get; set; } = 115200;
	public int DetectionTimeOut { get; set; } = 1000;
	public int OnInsertPrepareTime { get; set; } = 2000;
	public int MainTaskTimeOut { get; set; } = 30 * 1000;
}

public static class ConfigManager
{
	public const string ConfigLocation = "./Config.json";
	public static Config Config { get; set; } = new();

	static ConfigManager()
	{
		try
		{
			Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigLocation))!;
		} 
		catch
		{
			Config = new();
			Save();
		}
	}
	public static void Save()
	{
		File.WriteAllText(ConfigLocation, JsonConvert.SerializeObject(Config));
	}
}