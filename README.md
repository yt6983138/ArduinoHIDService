# ArduinoHIDSerivce
## Usage:
### Arduino Side
First, when recieve `AreYouClient\n` message you need to reply `Yes\n` <br>
Second, you need to send `KeepAlive\n` message every few seconds(recommend 5~30) <br>
Third, you can send HID controls by sending `Operation Type|Arg1|Arg2...` [Details in Method SerialTask](https://github.com/yt6983138/ArduinoHIDService/Program.cs) <br>
### Computer side
Just kept the program running, reminder: you need to start this program before you plug the Arduino on. <br>
Configurations are in Config.json.