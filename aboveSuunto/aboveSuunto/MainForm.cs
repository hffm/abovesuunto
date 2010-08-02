using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

//#include <sys/types.h>
//#include <unistd.h>
//#include <sys/stat.h>
//#include <fcntl.h>
//#include <termios.h>
//#include <stdio.h>
//#include <stdlib.h> 
//#include <strings.h>


///* baudrate settings are defined in <asm/termbits.h>, which is
//   included by <termios.h> */
//#define BAUDRATE B115200
///* change this definition for the correct port */
//#define SERIAL_PORT "/dev/ttyUSB0"
//#define _POSIX_SOURCE 1 /* POSIX compliant source */

//#define FALSE 0
//#define TRUE 1


///*The header length of Packates */
//#define MSG_HEADER_LEN 3

///* The maximum message payload size is 9, plus one for the checksum */
//#define MSG_CONTENTS_MAX_LEN 10


///* ANT Protocol Constants */
//#define ANT_WRITE(fd, command) write(fd, command, sizeof(command))
//const unsigned char AntStartByte = 0xA4;
//const unsigned char AntCapabilities[6] = {0xA4, 0x02, 0x4d, 0x00, 0x54, 0xbf};
//const unsigned char AntHostAssignChannel[7] = {0xA4, 0x03, 0x42, 0x00, 0x00, 0x01, 0xE4};
//const unsigned char AntHostChannelID[9] = {0xA4, 0x05, 0x51, 0x00, 0x00, 0x00, 0x00, 0x02, 0xF2};
//const unsigned char AntHostChannelSearchTimeout[6] = {0xA4, 0x02, 0x44, 0x00, 0x0C, 0xEE};
//const unsigned char AntHostChannelRFFreq[6] = {0xA4, 0x02, 0x45, 0x00, 0x41, 0xA2};
//const unsigned char AntBeginTransmission[5] = {0xA4, 0x01, 0x55, 0x00, 0xF0};



//void print_bytes(unsigned char* bytes, int count){
//  int i;
//  for(i = 0; i < count; i++){
//    printf("%02X  ", bytes[i]);
//  }
//}


//int main()
//{
//  int fd;
//  struct termios oldtio,newtio;
//  /* 
//     Open modem device for reading and writing and not as controlling tty
//     because we don't want to get killed if linenoise sends CTRL-C.
//  */
//  printf("Welcome to Will's Suunto PC Pod interface program.\n");
//  fd = open(SERIAL_PORT, O_RDWR | O_NOCTTY ); 
//  if (fd <0) {
//    perror(SERIAL_PORT); exit(-1);
//  }else{
//    printf("Serial port opened as File Descriptor %d\n", fd);
//  }

//  tcgetattr(fd,&oldtio); /* save current serial port settings */
//  bzero(&newtio, sizeof(newtio)); /* clear struct for new port settings */

//  /* 
//     BAUDRATE: Set bps rate. You could also use cfsetispeed and cfsetospeed.
//     CRTSCTS : output hardware flow control (only used if the cable has
//     all necessary lines. See sect. 7 of Serial-HOWTO)
//     CS8     : 8n1 (8bit,no parity,1 stopbit)
//     CLOCAL  : local connection, no modem contol
//     CREAD   : enable receiving characters
//  */
//  newtio.c_cflag = BAUDRATE | CRTSCTS | CS8 | CLOCAL | CREAD;
         
//  /*
//    IGNPAR  : ignore bytes with parity errors
//    ICRNL   : map CR to NL (otherwise a CR input on the other computer
//    will not terminate input)
//    otherwise make device raw (no other input processing)
//  */
//  newtio.c_iflag = IGNPAR;
         
//  /*
//    Raw output.
//  */
//  newtio.c_oflag = 0;
         
//  /*  Set input mode (non-conical, no echo, ...)  */
//  newtio.c_lflag = 0;
         

//  newtio.c_cc[VTIME]    = 0;   /* inter-character timer unused */
//  newtio.c_cc[VMIN]     = 3;   /* blocking read until 3 chars received */

        
//  /* 
//     now clean the modem line and activate the settings for the port
//  */
//  tcflush(fd, TCIFLUSH);
//  tcsetattr(fd,TCSANOW,&newtio);
        
//  /*
//    terminal settings done, now waking up the HR monitor
//  */

//  printf("Waking up the HR Monitor\n");

//  ANT_WRITE(fd, AntCapabilities);
//  ANT_WRITE(fd, AntHostAssignChannel);
//  ANT_WRITE(fd, AntHostChannelID);
//  ANT_WRITE(fd, AntHostChannelSearchTimeout);
//  ANT_WRITE(fd, AntHostChannelRFFreq);
//  ANT_WRITE(fd, AntBeginTransmission);



//  while(1){     /* loop forever */
//    unsigned char msgheader[MSG_HEADER_LEN]; /* Header of the ANT packets */
//    unsigned char msg[MSG_CONTENTS_MAX_LEN]; /* a buffer where the info is stored */
//    unsigned char checksum;
//    int i;

//    read(fd, msgheader, MSG_HEADER_LEN);

//    printf("Recieved a message\n");
//    if(msgheader[0] == AntStartByte){
//      printf("Correct Message Header\n");
//    }else{
//       printf("Bad Message, Exiting");
//       break;
//    }
       
//    if(msgheader[1] != 0){
//      printf("Payload of %d bytes\n",msgheader[1]);
//    }
    
//    read(fd, msg, msgheader[1] + 1);
    

//    checksum = msgheader[0]^msgheader[1]^msgheader[2];
//    printf("Message header checksum: %x\n",checksum );

//    for(i = 0; i < msgheader[1]; i++){
//  checksum = msg[i]^checksum;
//    }
//    printf("Computed checksum:%x\n", checksum);
//    printf("Real checksum:%x\n", msg[msgheader[1]]);
//    if(msg[msgheader[1]] == checksum){
//      printf("Checksum passed\n");
//      printf("Heart rate is thought to be %d\n", msg[5]);
//    }else{
//      printf("Checksum failed\n");
//    }
//    print_bytes(msgheader,MSG_HEADER_LEN);
//    print_bytes(msg, msgheader[1] + 1);

//  }
//  tcsetattr(fd,TCSANOW,&oldtio);
//  return 0;
//}
//#endif

namespace aboveSuunto
{
  public partial class MainForm : Form
  {
    readonly SerialPort m_suunto = new SerialPort();
    public MainForm()
    {
      InitializeComponent();
    }

    private void ButtonConnectClick(object sender, System.EventArgs e)
    {
      m_suunto.PortName = "COM3";
      m_suunto.BaudRate = 115200;
      m_suunto.Parity = Parity.None;
      m_suunto.DataBits = 8;
      m_suunto.StopBits = StopBits.One;
      m_suunto.Handshake = Handshake.None;
      m_suunto.Open();

      if (m_suunto.IsOpen)
      {
        byte[] antPort = {0xA4, 0x02, 0x4D, 0x00, 0x3D, 0xD6};
        byte[] antReset = {0xA4, 0x01, 0x4A, 0x00, 0xEF};
        byte[] antCapabilities = { 0xA4, 0x02, 0x4d, 0x00, 0x54, 0xbf };
        byte[] antHostAssignChannel = { 0xA4, 0x03, 0x42, 0x00, 0x00, 0x01, 0xE4 };
        byte[] antHostChannelID = { 0xA4, 0x05, 0x51, 0x00, 0x00, 0x00, 0x00, 0x02, 0xF2 };
        byte[] antHostChannelSearchTimeout = { 0xA4, 0x02, 0x44, 0x00, 0x0C, 0xEE };
        byte[] antHostChannelRfFreq = { 0xA4, 0x02, 0x45, 0x00, 0x41, 0xA2 };
        byte[] antBeginTransmission = { 0xA4, 0x01, 0x55, 0x00, 0xF0 };

        m_suunto.DataReceived += m_suunto_DataReceived;

        Send(antPort);
        Send(antReset);
        Send(antCapabilities);
        Send(antHostAssignChannel);
        Send(antHostChannelID);
        Send(antHostChannelSearchTimeout);
        Send(antHostChannelRfFreq);
        Send(antBeginTransmission);
      }
    }

    private void Send(byte[] data)
    {
      m_suunto.Write(data, 0, data.Length);
      Thread.Sleep(3000);
    }

    void m_suunto_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      SerialPort serialPort = (SerialPort)sender;
        while (serialPort.BytesToRead != 0)
        {
          AntMessage antMessage = new AntMessage(serialPort);
          if (antMessage.Valid)
            antMessage.Dump();
          else
            Trace.WriteLine("Invalid ant message");
        }
    }

    internal class AntMessage
    {
      public bool Valid { get; private set; }
      public byte[] Message = new byte[0];
      public byte Command { get; private set; }
      public AntMessage(SerialPort serialPort)
      {
        while (serialPort.BytesToRead != 0)
        {
          Valid = serialPort.ReadByte() == 0xA4;
          if (!Valid || serialPort.BytesToRead == 0) continue;
          int messageLength = serialPort.ReadByte();
          byte checkSum = 0xA4;
          checkSum ^= (byte)messageLength;
          Valid = serialPort.BytesToRead != 0;
          if (!Valid) continue;
          Command = (byte)serialPort.ReadByte();
          checkSum ^= Command;
          Message = new byte[messageLength];
          for (int i = 0; i < messageLength && serialPort.BytesToRead != 0; ++i)
          {
            Message[i] = (byte)serialPort.ReadByte();
            checkSum ^= Message[i];
          }
          Valid = serialPort.BytesToRead != 0;
          if (Valid)
            Valid = checkSum == (byte)serialPort.ReadByte();
          if (Valid)
            break;
          Trace.WriteLine("Checksum error");
        }
      }

      public void Dump()
      {
        Trace.Write(Command.ToString("X2") + " : ");
        foreach (byte data in Message)
          Trace.Write(data.ToString("X2"));
        Trace.WriteLine("");
      }
    }
  }
}
