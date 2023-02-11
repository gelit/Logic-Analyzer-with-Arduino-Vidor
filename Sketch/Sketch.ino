/* 10 fév 2023 from EmptySketch
Pin Used : D0		    oTshift
           D1-D7		7 input	
           D8-D14	  7 output	
           A0       Generator	
           A1		    Byte Available		
           A2-A4		Frequency cmd	
           A5-A6		Trigger	cmd
*/

#include <arduino.h>
#include <SPI.h>
#include "jtag.h"

// For High level functions such as pinMode or digitalWrite, you have to use FPGA_xxx
// Low level functions (in jtag.c file) use other kind of #define (TDI,TDO,TCK,TMS) with different values
#define FPGA_TDI                            (26u)
#define FPGA_TDO                            (29u)
#define FPGA_TCK                            (27u)
#define FPGA_TMS                            (28u)

// Clock send by SAMD21 to the FPGA
#define FPGA_CLOCK                        (30u)

// SAMD21 to FPGA control signal (interrupt ?)
#define FPGA_MB_INT                       (31u)

// FPGA to SAMD21 control signal (interrupt ?)
#define FPGA_INT                          (33u) //B2 N2

// For MKR pinout assignments see : https://systemes-embarques.fr/wp/brochage-connecteur-mkr-vidor-4000/

extern void enableFpgaClock(void);

#define no_data    0xFF, 0xFF, 0xFF, 0xFF, \
          0xFF, 0xFF, 0xFF, 0xFF, \
          0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, \
          0xFF, 0xFF, 0xFF, 0xFF, \
          0x00, 0x00, 0x00, 0x00  \

#define NO_BOOTLOADER   no_data
#define NO_APP        no_data
#define NO_USER_DATA    no_data

__attribute__ ((used, section(".fpga_bitstream_signature")))
const unsigned char signatures[4096] = {
  //#include "signature.ttf"
  NO_BOOTLOADER,

  0x00, 0x00, 0x08, 0x00,
  0xA9, 0x6F, 0x1F, 0x00,   // Don't care.
  0x20, 0x77, 0x77, 0x77, 0x2e, 0x73, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x65, 0x73, 0x2d, 0x65, 0x6d, 0x62, 0x61, 0x72, 0x71, 0x75, 0x65, 0x73, 0x2e, 0x66, 0x72, 0x20, 0x00, 0x00, 0xff, 0xf0, 0x0f,
  0x01, 0x00, 0x00, 0x00,   
  0x01, 0x00, 0x00, 0x00,   // Force

  NO_USER_DATA,
};
__attribute__ ((used, section(".fpga_bitstream")))
const unsigned char bitstream[] = {
  #include "D:\Vidor\LA\VidorFPGA-master\projects\MKRVIDOR4000_template\output_files\app.h"
};

//================================================================================================ VAR
volatile byte           Div, Mode;
volatile bool           Red_Led, ON;
volatile bool           C[8][2000];   // Buffer = ShiftRegister
volatile byte           Level[8];     // High / Low / Change during last second
volatile int            Bn, N, Delta;
volatile bool           State;
volatile unsigned long  Time;

void setup() {

  int ret;
  uint32_t ptr[1];

  enableFpgaClock();

  //Init Jtag Port
  ret = jtagInit();
  mbPinSet();

  // Load FPGA user configuration
  ptr[0] = 0 | 3;
  mbEveSend(ptr, 1);

  // Give it delay
  delay(1000);

  // Configure onboard LED Pin as output
  pinMode(LED_BUILTIN, OUTPUT);

  // Disable all JTAG Pins (usefull for USB BLASTER connection)
  pinMode(FPGA_TDO, INPUT);
  pinMode(FPGA_TMS, INPUT);
  pinMode(FPGA_TDI, INPUT);
  pinMode(FPGA_TCK, INPUT);

  // Configure other share pins as input too
  pinMode(FPGA_INT, INPUT);
//------------------------------------------------------------------------------------
//pinMode(x,INPUT) is defaut
// DON'T USE pin 13-14 used by FPGA

  pinMode(LED_BUILTIN, OUTPUT); digitalWrite(LED_BUILTIN,1); Red_Led=0;  // debug purpose 
  pinMode(A0,OUTPUT); digitalWrite(A0,0); // Generator
  pinMode(A1,INPUT);                      // Byte Available from SM

  pinMode(A3,OUTPUT); pinMode(A4,OUTPUT); pinMode(A2,OUTPUT); digitalWrite(A3,1); digitalWrite(A4,0); digitalWrite(A2,0);  // command to frequency divider
//        Reset               Pulse                 Latch
   
  digitalWrite(A3,0); delayMicroseconds(50);
  Div=5; for (N=1; N<=Div; N++) {one();} // DEFAULT : Fech=1.1 MHz  T=1/Fech=900 ns                                     
  digitalWrite(A2,1); delayMicroseconds(10); digitalWrite(A2,0); delayMicroseconds(10);
  digitalWrite(A3,1);

  pinMode(A5,OUTPUT); pinMode(A6,OUTPUT); digitalWrite(A5,1); digitalWrite(A6,0); Mode='B';  // Default Trigger mode = Begin     
  
  Bn=0; State=0; ON=0;
  for (N=1; N<=7; N++) {Level[N]=digitalRead(N);}
  Time = millis() + 1000;

  Serial.begin(800000);  // PC = Lenovo T540
  attachInterrupt(digitalPinToInterrupt(0), PCi, FALLING);  // D0 = Tshift
//MKR Family boards 0, 1, 4, 5, 6, 7, 8, 9, A1, A2 from https://www.arduino.cc/reference/en/language/functions/external-interrupts/attachinterrupt/
}

const int Max=1900; // SM send 1500 bit 

void PCi() {ON=1;}  // short as possible

void PC(int Channel) {        
  int r, m; // local var

  if (Channel==1) {
    r=0; Delta=0;  
    do {r++;} while (C[Channel][r]==C[Channel][1]);
    switch (Mode) {
      case 'B' : Delta =  200-r;  break;
      case 'C' : Delta =  950-r;  break;
      case 'E' : Delta = 1700-r;  break;  
    }
    Serial.print("Delta="); Serial.println(Delta);  // Debug
  }      
  
  Serial.print(Channel); Serial.println("C"); // Clear
  bool s = C[Channel][1];
  for (m=0; m<Max; m++) {  // +D
    if (s) {if (C[Channel][m]==1) {} else {Serial.print(Channel); Serial.print("H"); Serial.println(m); s=!s;}}                                      
    else   {if (C[Channel][m]==0) {} else {Serial.print(Channel); Serial.print("L"); Serial.println(m); s=!s;}}                                                                                                                     
  }
                                           Serial.print(Channel); if (s) {Serial.print("H");} else {Serial.print("L");} Serial.println(Max);  // send residual      
}

void ToggleLed() {if (Red_Led) {digitalWrite(LED_BUILTIN,1);} else {digitalWrite(LED_BUILTIN,0);} Red_Led=!Red_Led;}

void loop() {

  for (N=1; N<=7; N++) { if (digitalRead(N) == Level[N]) {} else {Level[N]=2;}}

  if (millis() > Time) {
    Time = millis() + 1000;
    for (N=1; N<=7; N++) {Serial.print("L"); Serial.print(N); Serial.println(Level[N]); if (Level[N]==2) {Level[N]=digitalRead(N);}}
  }
  
  if (ON) {Bn=0;                          // ready for next Acquisition 
           Serial.println("N");
           for (N=1; N<=7; N++) {PC(N);}  // send String to PC
           ON=0;}

// ---------------------------------------------------  I prefer to poll A1 (A1 interrupt is critical)
  if (!digitalRead(A1)) {State=1;} else {State=2;}   // Edge detection
  if (State==1 && digitalRead(A1)) {                 // pos edge  
//--------------------------
    State=3;  
    C[1][Bn]=digitalRead(8); C[2][Bn]=digitalRead(9); C[3][Bn]=digitalRead(10); C[4][Bn]=digitalRead(11); C[5][Bn]=digitalRead(12); C[6][Bn]=digitalRead(13); C[7][Bn]=digitalRead(14);
    if (Bn<Max+10) {Bn++;}
//-------------------------- ExecTime = 10 micros --> SM2clk=17.8 kHz
  }

  if (Serial.available() > 0) {
    byte Y = Serial.read();  // consommer
    
    switch (Y) {

      case '1' : digitalWrite(A0,1); delayMicroseconds(100); digitalWrite(A0,0);                        
      break;
      
      case '2' : for (N=1; N<=14; N++) {digitalWrite(A0,1); delayMicroseconds(100); digitalWrite(A0,0); delayMicroseconds(100);}                       
      break;

      case '3' : digitalWrite(A0,1);  delayMicroseconds(200); digitalWrite(A0,0);  delayMicroseconds(200);  // 1250 micros
                 digitalWrite(A0,1);  delayMicroseconds(100); digitalWrite(A0,0);  delayMicroseconds(100);
                 digitalWrite(A0,1);  delayMicroseconds(300); digitalWrite(A0,0);  delayMicroseconds(300);
                 digitalWrite(A0,1);  delayMicroseconds(50);  digitalWrite(A0,0); 
      break;
      
      case '4' : digitalWrite(A0,1);  delayMicroseconds(100); digitalWrite(A0,0);  delayMicroseconds(100);  // 1250 micros
                 digitalWrite(A0,1);  delayMicroseconds(200); digitalWrite(A0,0);  delayMicroseconds(200);
                 digitalWrite(A0,1);  delayMicroseconds(300); digitalWrite(A0,0);  delayMicroseconds(300);
                 digitalWrite(A0,1);  delayMicroseconds(50);  digitalWrite(A0,0);
      break; 
    
      case '5' : digitalWrite(A0,1);  delayMicroseconds(50);  digitalWrite(A0,0);  delayMicroseconds(50);  // 800 micros
                 digitalWrite(A0,1);  delayMicroseconds(100); digitalWrite(A0,0);  delayMicroseconds(100);
                 digitalWrite(A0,1);  delayMicroseconds(200); digitalWrite(A0,0);  delayMicroseconds(200);
      break; 
      case '6' : digitalWrite(A0,1);  // stay in level H
      break; 
      case '7' : digitalWrite(A0,0); delayMicroseconds(100); digitalWrite(A0,1);  // Test neg edge             
      break; 
      case '8' : 
      break; 
      case '9' : 
      break; 

      case 'S' : if            (!digitalRead(A5) && !digitalRead(A6)) {  // Toggle (Running / Stopped)
                   switch (Mode) {
                     case 'B' : digitalWrite(A5,1); digitalWrite(A6,0); break; 
                     case 'C' : digitalWrite(A5,0); digitalWrite(A6,1); break;          
                     case 'E' : digitalWrite(A5,1); digitalWrite(A6,1); break; 
                   }
                 } 
                 else          {digitalWrite(A5,0); digitalWrite(A6,0);}
      break;
      case 'B' : digitalWrite(A5,1); digitalWrite(A6,0); Mode='B'; // Begin Trigger
      break; 
      case 'C' : digitalWrite(A5,0); digitalWrite(A6,1); Mode='C'; // Center
      break; 
      case 'E' : digitalWrite(A5,1); digitalWrite(A6,1); Mode='E'; // End
      break; 
                                                                                                        // UP
      case 'U' : digitalWrite(A3,0); delayMicroseconds(50);                                             // !Reset 
                 Div--; if (Div==0)  {Div=1;}  for (N=1; N<=Div; N++) {one();}                          // 
                 digitalWrite(A2,1); delayMicroseconds(10); digitalWrite(A2,0); delayMicroseconds(10);  // Latch
                 digitalWrite(A3,1);              
      break; 
                                                                                                        // DOWN
      case 'D' : digitalWrite(A3,0); delayMicroseconds(50);
                 Div++; if (Div>20) {Div=20;} for (N=1; N<=Div; N++) {one();} 
                 digitalWrite(A2,1); delayMicroseconds(10); digitalWrite(A2,0); delayMicroseconds(10);
                 digitalWrite(A3,1);     
      break; 

      case 'R' : digitalWrite(A3,0); delayMicroseconds(50);  // 5 fév : Reset
                 Div=5; for (N=1; N<=Div; N++) {one();} // DEFAULT : Fech=1.1 MHz  T=1/Fech=900 ns                                     
                 digitalWrite(A2,1); delayMicroseconds(10); digitalWrite(A2,0); delayMicroseconds(10);
                 digitalWrite(A3,1);

                 pinMode(A5,OUTPUT); pinMode(A6,OUTPUT); digitalWrite(A5,1); digitalWrite(A6,0); Mode='B';  // Default Trigger mode = Begin     
      break; 
    }    
  }    
}

void one() {
  digitalWrite(A4,1); delayMicroseconds(10);
  digitalWrite(A4,0); delayMicroseconds(10);  
}
