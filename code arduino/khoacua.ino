#include<Keypad.h>
#include<LiquidCrystal_I2C.h>
#include<EEPROM.h>

int buzzer = 13;

char password[4];
char pass[4], pass1[4];
int wrong = 0;
int i = 0; // khởi tạo biến đếm cho độ dài chuỗi vừa nhập vào
int signalPin = 12; //khai báo chân relay
char customKey = 0; // giá trị vừa nhấn là gì

//----- khai bao cho phim 4*4
const byte ROWS = 4;
const byte COLS = 4;
char hexaKeys[ROWS][COLS] =
{
  {'1', '2', '3', 'A'},
  {'4', '5', '6', 'B'},
  {'7', '8', '9', 'C'},
  {'*', '0', '#', 'D'}
};

byte rowPins[ROWS] = {9, 8, 7, 6};
byte colPins[COLS] = {5, 4, 3, 2};
Keypad customKeypad = Keypad(makeKeymap(hexaKeys), rowPins, colPins, ROWS, COLS);

// ------------- khai báo cho LCD
LiquidCrystal_I2C lcd(0x27, 16, 2);

// ------------------------------------------------------------------------
void setup()
{
  pinMode(buzzer, OUTPUT);
  digitalWrite( buzzer, HIGH);
  //khai báo cho LCD
  lcd.init();
  lcd.backlight();
  pinMode(signalPin, OUTPUT);
  lcd.setCursor(2, 0);
  lcd.print("GET STARTED");
  lcd.setCursor(5, 1);
  lcd.print("READY");
  delay(2000);
  lcd.clear();
  lcd.print("Enter Passkey:");
  lcd.setCursor(0, 1);

  // tạo mật khẩu mặc định từ 1->4
  for (int j = 0; j < 4; j++)
    EEPROM.write(j, j + 49);
  for (int j = 0; j < 4; j++)
    pass[j] = EEPROM.read(j);
  // khai báo uart
  Serial.begin(9600);
}


//---------------------------------------
void loop()
{
  customKey = customKeypad.getKey();
  if (customKey == '#')change();

  if (customKey) // hiển thị kí tự vừa nhập lên LCD
  {
    password[i++] = customKey; //
    lcd.print(customKey);
  }
  if (i == 4) // kiểm tra nếu nhập đủ 4 kí tự thì tự thực hiện so sánh với mật khẩu
  {
    delay(200);
    for (int j = 0; j < 4; j++)
      pass[j] = EEPROM.read(j);
    if (!(strncmp(password, pass, 4))) // so sánh dùng hàm strncmp
    {
      Serial.println("0"); // giá trị in ra màn hình
      digitalWrite(signalPin, HIGH);
      delay(5000); // đền sáng trong 5s
      lcd.clear();
      lcd.print("Passkey Accepted");
      delay(1000);
      lcd.setCursor(0, 1);
      lcd.print("#.Change Passkey");
      delay(1000);
      lcd.clear();
      lcd.print("Enter Passkey:");  // đưa về trạng thái ban đầu
      lcd.setCursor(0, 1);
      i = 0; //set lại độ dài chuỗi nhập vào = 0
      wrong = 0; // set lại số lần sai = 0
      digitalWrite(signalPin, LOW);  // đưa role xuống mức thấp
    }
    else
    {
      wrong++;
      lcd.clear();
      //Serial.println("0"); // giá trị in ra màn hình
      lcd.print("Access Denied...");
      lcd.setCursor(0, 1);
      delay(2000);
      lcd.clear();
      lcd.print("Enter Passkey:");
      lcd.setCursor(0, 1);
      i = 0;
    }
  }
        if (wrong == 3)
      {
        block();
      }
}
//-----------Hàm đổi pass
void change()
{
  int j = 0;
  lcd.clear();
  lcd.print("Current Pass");
  lcd.setCursor(0, 1);
  while (j < 4)
  {
    char key = customKeypad.getKey();
    if (key)
    {
      pass1[j++] = key;
      lcd.print(key);      
    }
    key = 0;
  }
  delay(500);
  if ((strncmp(pass1, pass, 4)))
  {
    lcd.clear();
    lcd.setCursor(3, 0);
    lcd.print("WRONG PASS");
    lcd.setCursor(3, 1);
    lcd.print("TRY AGAIN");
    delay(1000);
  }
  else
  {
    j = 0;
    lcd.clear();
    lcd.print("Enter New Pass:");
    lcd.setCursor(0, 1);
    while (j < 4)
    {
      char key = customKeypad.getKey();
      if (key)
      {
        pass[j] = key;
        lcd.print(key);
        EEPROM.write(j, key);
        j++;        
      }
    }
    lcd.print(" Done......");
    delay(1000);
  }
  lcd.clear();
  lcd.print("Enter New Pass");
  lcd.setCursor(0, 1);
  customKey = 0;
}
void beep()
{
  digitalWrite(buzzer, HIGH);
  delay(500);
  digitalWrite(buzzer, LOW);
  delay(500);
}
void block()
{
  lcd.clear();
  lcd.print("WARNING");
  lcd.setCursor(0, 1);
          for (int k=0; k < 5; k++) // số lần kêu
        {
        beep();
        }
        digitalWrite(buzzer, HIGH);        
        wrong = 0;
  delay(5000);
  lcd.clear();
  lcd.print("Enter Passkey:");
  lcd.setCursor(0, 1);
  i = 0;  
}
