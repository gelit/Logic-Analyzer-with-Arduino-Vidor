// Generator tested on Arduino Due with 7 outputs (2-)

void setup() {
  Serial.begin(115200); Serial.println("Hi");
  pinMode(2,OUTPUT); pinMode(3,OUTPUT); pinMode(4,OUTPUT); pinMode(5,OUTPUT); pinMode(6,OUTPUT); pinMode(7,OUTPUT); pinMode(8,OUTPUT);
  digitalWrite(2,0); digitalWrite(3,0); digitalWrite(4,0); digitalWrite(5,0); digitalWrite(6,0); digitalWrite(7,0); digitalWrite(8,0); 
  Serial3.begin(100000);
}

void loop() {
  if (Serial.available() > 0) {
    byte Y = Serial.read();  // consommer

    digitalWrite(2,1); delayMicroseconds(100); digitalWrite(2,0);  // D1=Trigger    
//  100
    digitalWrite(6,1); delayMicroseconds(50); digitalWrite(6,0);
//  150    
    digitalWrite(3,1); delayMicroseconds(100);     
//  250
    digitalWrite(3,0); digitalWrite(4,1); delayMicroseconds(200); digitalWrite(4,0);  
//  450    
    digitalWrite(5,1); delayMicroseconds(400); digitalWrite(5,0);
//  850
    digitalWrite(7,1); delayMicroseconds(100); digitalWrite(7,0);
    Serial3.print("7");  // pin 14
  }
}



