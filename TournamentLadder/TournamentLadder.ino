#include <LiquidCrystal.h>

// Initializing the LCD (pins: RS, E, D4, D5, D6, D7)
LiquidCrystal lcd(2, 3, 4, 5, 6, 7);

const int btnLeft = A0;   // Analog input A0 - left button
const int btnRight = A1;  // Analog input A1 - right button

int lastStateLeft = HIGH;   // Previous state of the left button
int lastStateRight = HIGH;  // Previous state of the right button

String receivedData = "";    // Buffer for data received from the PC

void setup() {
  Serial.begin(9600);          // Start serial communication
  lcd.begin(16, 2);            // Start LCD 16x2
  lcd.print("Waiting..."); // Initial text

  pinMode(btnLeft, INPUT_PULLUP);  // Pull-up buttons
  pinMode(btnRight, INPUT_PULLUP);
}

void loop() {

  // Retrieving data from a PC
  if (Serial.available() > 0) {
    receivedData = Serial.readStringUntil('\n'); // Read until the end of the line
    receivedData.trim(); // Removing \n and spaces

    lcd.clear();         // Clear LCD
    lcd.setCursor(0, 0);

    // Displaying a message on the LCD based on the command
    if (receivedData == "1") {
      lcd.print("Pressed");
      lcd.setCursor(0, 1);
      lcd.print("button 1");
    } else if (receivedData == "2") {
      lcd.print("Pressed");
      lcd.setCursor(0, 1);
      lcd.print("button 2");
    } else if (receivedData == "3") {
      lcd.print("Pressed");
      lcd.setCursor(0, 1);
      lcd.print("button 3");
    } else {
      lcd.print("Unknown command");z
    }
  }

  // Physical buttons read
  int currentLeft = digitalRead(btnLeft);   // Read left button
  int currentRight = digitalRead(btnRight); // Read right button

  // Detection of a downward slope (click)
  if (lastStateLeft == HIGH && currentLeft == LOW) {
    Serial.println("A0"); // Send a message to the PC indicating that the left button has been pressed
  }

  if (lastStateRight == HIGH && currentRight == LOW) {
    Serial.println("A1"); // Send a message to the PC indicating that the right button has been pressed
  }

  // Save the button states
  lastStateLeft = currentLeft;
  lastStateRight = currentRight;

  delay(50); // Debounce
}
