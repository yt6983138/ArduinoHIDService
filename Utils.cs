using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoHIDService;
public static class Utils
{
	public static T ParseEnum<T>(string input) where T : struct, Enum
	{
		return Enum.Parse<T>(input);
	}
}
