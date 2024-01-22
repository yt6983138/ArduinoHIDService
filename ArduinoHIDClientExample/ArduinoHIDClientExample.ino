bool hasSentKeepAlive = false;
bool hasBeenAsked = false;

bool k1LastState = 0;
bool k1NowState = 0;
bool k2LastState = 0;
bool k2NowState = 0;
bool k3LastState = 0;
bool k3NowState = 0;
bool k4LastState = 0;
bool k4NowState = 0;

void setup() {
	Serial.begin(115200);
	// Serial.setTimeout(250);
	pinMode(13, OUTPUT); // testing purpose
	pinMode(A0, INPUT);
}

void TrySendKeyBoardMessage(bool &lastState, bool &nowState, uint8_t pin, String key) {
	nowState = analogRead(pin) > 512 ? true : false;
	if (lastState == 1 && nowState == 0) {
		Serial.println("KeyboardEvent|" + key + "|false|0");
	}
	if (lastState == 0 && nowState == 1) {
		Serial.println("KeyboardEvent|" + key + "|true|0");
	}
	lastState = nowState;
}

void loop() {
	if (Serial.available() > 0) {
		String read = Serial.readStringUntil('\n');
		read.trim();
		if (read == "AreYouClient") {
			delay(40);
			Serial.println("Yes");
			hasBeenAsked = true;
		}
	}
	if ((millis() % (5 * 1000)) == 0 && !hasSentKeepAlive && hasBeenAsked) {
		Serial.println("KeepAlive");
		hasSentKeepAlive = true;
	}
	if ((millis() % (5 * 1000)) > 0 && hasSentKeepAlive && hasBeenAsked) {
		hasSentKeepAlive = false;
	}

	TrySendKeyBoardMessage(k1LastState, k1NowState, A0, "VK_KEY_D");
	TrySendKeyBoardMessage(k2LastState, k2NowState, A1, "VK_KEY_F");
	TrySendKeyBoardMessage(k3LastState, k3NowState, A2, "VK_KEY_J");
	TrySendKeyBoardMessage(k4LastState, k4NowState, A3, "VK_KEY_K");
}
