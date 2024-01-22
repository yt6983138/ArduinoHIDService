using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoHIDService;
public static class WinEventCode
{
	public const int WM_DEVICECHANGE = 0x0219;
	public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // https://github.com/wine-mirror/wine/blob/master/include/dbt.h
	public const int DBT_DEVICEARRIVAL = 0x8000;
}
