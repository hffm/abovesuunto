using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

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
