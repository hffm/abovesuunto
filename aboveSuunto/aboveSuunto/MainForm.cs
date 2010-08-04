using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace aboveSuunto
{
  public partial class MainForm : Form
  {
    #region ANT constants
    /////////////////////////////////////////////////////////////////////////////
    // Message Format
    // Messages are in the format:
    //
    // AX XX YY -------- CK
    //
    // where: AX    is the 1 byte sync byte either transmit or recieve
    //        XX    is the 1 byte size of the message (0-249) NOTE: THIS WILL BE LIMITED BY THE EMBEDDED RECEIVE BUFFER SIZE
    //        YY    is the 1 byte ID of the message (1-255, 0 is invalid)
    //        ----- is the data of the message (0-249 bytes of data)
    //        CK    is the 1 byte Checksum of the message
    /////////////////////////////////////////////////////////////////////////////
    // ReSharper disable InconsistentNaming
#pragma warning disable 169

    const byte MESG_TX_SYNC = 0xA4;
    const byte MESG_RX_SYNC = 0xA5;
    const byte MESG_SYNC_SIZE = 1;
    const byte MESG_SIZE_SIZE = 1;
    const byte MESG_ID_SIZE = 1;
    const byte MESG_CHANNEL_NUM_SIZE = 1;
    const byte MESG_EXT_MESG_BF_SIZE = 1; // NOTE: this could increase in the future
    const byte MESG_CHECKSUM_SIZE = 1;
    const byte MESG_DATA_SIZE = 9;
    //////////////////////////////////////////////
    // ANT Message Payload Size
    //////////////////////////////////////////////
    private const byte ANT_STANDARD_DATA_PAYLOAD_SIZE = 8;

    //////////////////////////////////////////////
    // ANT LIBRARY Extended Data Message Fields
    // NOTE: You must check the extended message
    // bitfield first to find out which fields
    // are present before accessing them!
    //////////////////////////////////////////////
    private const byte ANT_EXT_MESG_DEVICE_ID_FIELD_SIZE = 4;


    // The largest serial message is an ANT data message with all of the extended fields
    private const byte MESG_ANT_MAX_PAYLOAD_SIZE = ANT_STANDARD_DATA_PAYLOAD_SIZE;

    const byte MESG_MAX_EXT_DATA_SIZE = (ANT_EXT_MESG_DEVICE_ID_FIELD_SIZE + 4 + 2); // ANT device ID (4 bytes; + (4 bytes; + (2 bytes;

    const byte MESG_MAX_DATA_SIZE = (MESG_ANT_MAX_PAYLOAD_SIZE + MESG_EXT_MESG_BF_SIZE + MESG_MAX_EXT_DATA_SIZE); // ANT data payload (8 bytes; + extended bitfield (1 byte; + extended data (10 bytes;
    const byte MESG_MAX_SIZE_VALUE = (MESG_MAX_DATA_SIZE + MESG_CHANNEL_NUM_SIZE); // this is the maximum value that the serial message size value is allowed to be
    const byte MESG_BUFFER_SIZE = (MESG_SIZE_SIZE + MESG_ID_SIZE + MESG_CHANNEL_NUM_SIZE + MESG_MAX_DATA_SIZE + MESG_CHECKSUM_SIZE);
    const byte MESG_FRAMED_SIZE = (MESG_ID_SIZE + MESG_CHANNEL_NUM_SIZE + MESG_MAX_DATA_SIZE);
    const byte MESG_HEADER_SIZE = (MESG_SYNC_SIZE + MESG_SIZE_SIZE + MESG_ID_SIZE);
    const byte MESG_FRAME_SIZE = (MESG_HEADER_SIZE + MESG_CHECKSUM_SIZE);
    const byte MESG_MAX_SIZE = (MESG_MAX_DATA_SIZE + MESG_FRAME_SIZE);

    const byte MESG_SIZE_OFFSET = (MESG_SYNC_SIZE);
    const byte MESG_ID_OFFSET = (MESG_SYNC_SIZE + MESG_SIZE_SIZE);
    const byte MESG_DATA_OFFSET = (MESG_HEADER_SIZE);
    const byte MESG_RECOMMENDED_BUFFER_SIZE = 64; // This is the recommended size for serial message buffers if there are no RAM restrictions on the system

    //////////////////////////////////////////////
    // Message ID's
    //////////////////////////////////////////////
    const byte MESG_INVALID_ID = 0x00;
    const byte MESG_EVENT_ID = 0x01;

    const byte MESG_VERSION_ID = 0x3E;
    const byte MESG_RESPONSE_EVENT_ID = 0x40;

    const byte MESG_UNASSIGN_CHANNEL_ID = 0x41;
    const byte MESG_ASSIGN_CHANNEL_ID = 0x42;
    const byte MESG_CHANNEL_MESG_PERIOD_ID = 0x43;
    const byte MESG_CHANNEL_SEARCH_TIMEOUT_ID = 0x44;
    const byte MESG_CHANNEL_RADIO_FREQ_ID = 0x45;
    const byte MESG_NETWORK_KEY_ID = 0x46;
    const byte MESG_RADIO_TX_POWER_ID = 0x47;
    const byte MESG_RADIO_CW_MODE_ID = 0x48;
    const byte MESG_SYSTEM_RESET_ID = 0x4A;
    const byte MESG_OPEN_CHANNEL_ID = 0x4B;
    const byte MESG_CLOSE_CHANNEL_ID = 0x4C;
    const byte MESG_REQUEST_ID = 0x4D;

    const byte MESG_BROADCAST_DATA_ID = 0x4E;
    const byte MESG_ACKNOWLEDGED_DATA_ID = 0x4F;
    const byte MESG_BURST_DATA_ID = 0x50;

    const byte MESG_CHANNEL_ID_ID = 0x51;
    const byte MESG_CHANNEL_STATUS_ID = 0x52;
    const byte MESG_RADIO_CW_INIT_ID = 0x53;
    const byte MESG_CAPABILITIES_ID = 0x54;

    const byte MESG_STACKLIMIT_ID = 0x55;

    const byte MESG_SCRIPT_DATA_ID = 0x56;
    const byte MESG_SCRIPT_CMD_ID = 0x57;

    const byte MESG_ID_LIST_ADD_ID = 0x59;
    const byte MESG_ID_LIST_CONFIG_ID = 0x5A;
    const byte MESG_OPEN_RX_SCAN_ID = 0x5B;

    const byte MESG_EXT_CHANNEL_RADIO_FREQ_ID = 0x5C; // OBSOLETE: (for 905 radio;
    const byte MESG_EXT_BROADCAST_DATA_ID = 0x5D;
    const byte MESG_EXT_ACKNOWLEDGED_DATA_ID = 0x5E;
    const byte MESG_EXT_BURST_DATA_ID = 0x5F;

    const byte MESG_CHANNEL_RADIO_TX_POWER_ID = 0x60;
    const byte MESG_GET_SERIAL_NUM_ID = 0x61;
    const byte MESG_GET_TEMP_CAL_ID = 0x62;
    const byte MESG_SET_LP_SEARCH_TIMEOUT_ID = 0x63;
    const byte MESG_SET_TX_SEARCH_ON_NEXT_ID = 0x64;
    const byte MESG_SERIAL_NUM_SET_CHANNEL_ID_ID = 0x65;
    const byte MESG_RX_EXT_MESGS_ENABLE_ID = 0x66;
    const byte MESG_RADIO_CONFIG_ALWAYS_ID = 0x67;
    const byte MESG_ENABLE_LED_FLASH_ID = 0x68;
    const byte MESG_XTAL_ENABLE_ID = 0x6D;
    const byte MESG_STARTUP_MESG_ID = 0x6F;
    const byte MESG_AUTO_FREQ_CONFIG_ID = 0x70;
    const byte MESG_PROX_SEARCH_CONFIG_ID = 0x71;

    const byte MESG_CUBE_CMD_ID = 0x80;

    const byte MESG_GET_PIN_DIODE_CONTROL_ID = 0x8D;
    const byte MESG_PIN_DIODE_CONTROL_ID = 0x8E;
    const byte MESG_FIT1_SET_AGC_ID = 0x8F;

    const byte MESG_FIT1_SET_EQUIP_STATE_ID = 0x91; // *** CONFLICT: w/ Sensrcore, Fit1 will never have sensrcore enabled

    // Sensrcore Messages
    const byte MESG_SET_CHANNEL_INPUT_MASK_ID = 0x90;
    const byte MESG_SET_CHANNEL_DATA_TYPE_ID = 0x91;
    const byte MESG_READ_PINS_FOR_SECT_ID = 0x92;
    const byte MESG_TIMER_SELECT_ID = 0x93;
    const byte MESG_ATOD_SETTINGS_ID = 0x94;
    const byte MESG_SET_SHARED_ADDRESS_ID = 0x95;
    const byte MESG_ATOD_EXTERNAL_ENABLE_ID = 0x96;
    const byte MESG_ATOD_PIN_SETUP_ID = 0x97;
    const byte MESG_SETUP_ALARM_ID = 0x98;
    const byte MESG_ALARM_VARIABLE_MODIFY_TEST_ID = 0x99;
    const byte MESG_PARTIAL_RESET_ID = 0x9A;
    const byte MESG_OVERWRITE_TEMP_CAL_ID = 0x9B;
    const byte MESG_SERIAL_PASSTHRU_SETTINGS_ID = 0x9C;

    const byte MESG_READ_SEGA_ID = 0xA0;
    const byte MESG_SEGA_CMD_ID = 0xA1;
    const byte MESG_SEGA_DATA_ID = 0xA2;
    const byte MESG_SEGA_ERASE_ID = 0xA3;
    const byte MESG_SEGA_WRITE_ID = 0xA4;
    const byte AVOID_USING_SYNC_BYTES_FOR_MESG_IDS = 0xA5;

    const byte MESG_SEGA_LOCK_ID = 0xA6;
    const byte MESG_FLASH_PROTECTION_CHECK_ID = 0xA7;
    const byte MESG_UARTREG_ID = 0xA8;
    const byte MESG_MAN_TEMP_ID = 0xA9;
    const byte MESG_BIST_ID = 0xAA;
    const byte MESG_SELFERASE_ID = 0xAB;
    const byte MESG_SET_MFG_BITS_ID = 0xAC;
    const byte MESG_UNLOCK_INTERFACE_ID = 0xAD;
    const byte MESG_SERIAL_ERROR_ID = 0xAE;
    const byte MESG_SET_ID_STRING_ID = 0xAF;

    const byte MESG_IO_STATE_ID = 0xB0;
    const byte MESG_CFG_STATE_ID = 0xB1;
    const byte MESG_BLOWFUSE_ID = 0xB2;
    const byte MESG_MASTERIOCTRL_ID = 0xB3;
    const byte MESG_PORT_GET_IO_STATE_ID = 0xB4;
    const byte MESG_PORT_SET_IO_STATE_ID = 0xB5;



    const byte MESG_SLEEP_ID = 0xC5;
    const byte MESG_GET_GRMN_ESN_ID = 0xC6;

    const byte MESG_DEBUG_ID = 0xF0; // use 2 byte sub-index identifier

    //////////////////////////////////////////////
    // Message Sizes
    //////////////////////////////////////////////
    const byte MESG_INVALID_SIZE = 0;

    const byte MESG_VERSION_SIZE = 13;
    const byte MESG_RESPONSE_EVENT_SIZE = 3;
    const byte MESG_CHANNEL_STATUS_SIZE = 2;

    const byte MESG_UNASSIGN_CHANNEL_SIZE = 1;
    const byte MESG_ASSIGN_CHANNEL_SIZE = 3;
    const byte MESG_CHANNEL_ID_SIZE = 5;
    const byte MESG_CHANNEL_MESG_PERIOD_SIZE = 3;
    const byte MESG_CHANNEL_SEARCH_TIMEOUT_SIZE = 2;
    const byte MESG_CHANNEL_RADIO_FREQ_SIZE = 2;
    const byte MESG_CHANNEL_RADIO_TX_POWER_SIZE = 2;
    const byte MESG_NETWORK_KEY_SIZE = 9;
    const byte MESG_RADIO_TX_POWER_SIZE = 2;
    const byte MESG_RADIO_CW_MODE_SIZE = 3;
    const byte MESG_RADIO_CW_INIT_SIZE = 1;
    const byte MESG_SYSTEM_RESET_SIZE = 1;
    const byte MESG_OPEN_CHANNEL_SIZE = 1;
    const byte MESG_CLOSE_CHANNEL_SIZE = 1;
    const byte MESG_REQUEST_SIZE = 2;

    const byte MESG_CAPABILITIES_SIZE = 6;
    const byte MESG_STACKLIMIT_SIZE = 2;

    const byte MESG_SCRIPT_DATA_SIZE = 10;
    const byte MESG_SCRIPT_CMD_SIZE = 3;

    const byte MESG_ID_LIST_ADD_SIZE = 6;
    const byte MESG_ID_LIST_CONFIG_SIZE = 3;
    const byte MESG_OPEN_RX_SCAN_SIZE = 1;
    const byte MESG_EXT_CHANNEL_RADIO_FREQ_SIZE = 3;

    const byte MESG_RADIO_CONFIG_ALWAYS_SIZE = 2;
    const byte MESG_RX_EXT_MESGS_ENABLE_SIZE = 2;
    const byte MESG_SET_TX_SEARCH_ON_NEXT_SIZE = 2;
    const byte MESG_SET_LP_SEARCH_TIMEOUT_SIZE = 2;

    const byte MESG_SERIAL_NUM_SET_CHANNEL_ID_SIZE = 3;
    const byte MESG_ENABLE_LED_FLASH_SIZE = 2;
    const byte MESG_GET_SERIAL_NUM_SIZE = 4;
    const byte MESG_GET_TEMP_CAL_SIZE = 4;
    const byte MESG_CLOCK_DRIFT_DATA_SIZE = 9;

    const byte MESG_AGC_CONFIG_SIZE = 2;
    const byte MESG_RUN_SCRIPT_SIZE = 2;
    const byte MESG_ANTLIB_CONFIG_SIZE = 2;
    const byte MESG_XTAL_ENABLE_SIZE = 1;
    const byte MESG_STARTUP_MESG_SIZE = 1;
    const byte MESG_AUTO_FREQ_CONFIG_SIZE = 4;
    const byte MESG_PROX_SEARCH_CONFIG_SIZE = 2;

    const byte MESG_GET_PIN_DIODE_CONTROL_SIZE = 1;
    const byte MESG_PIN_DIODE_CONTROL_ID_SIZE = 2;
    const byte MESG_FIT1_SET_EQUIP_STATE_SIZE = 2;
    const byte MESG_FIT1_SET_AGC_SIZE = 3;

    const byte MESG_READ_SEGA_SIZE = 2;
    const byte MESG_SEGA_CMD_SIZE = 3;
    const byte MESG_SEGA_DATA_SIZE = 10;
    const byte MESG_SEGA_ERASE_SIZE = 0;
    const byte MESG_SEGA_WRITE_SIZE = 3;
    const byte MESG_SEGA_LOCK_SIZE = 1;
    const byte MESG_FLASH_PROTECTION_CHECK_SIZE = 1;
    const byte MESG_UARTREG_SIZE = 2;
    const byte MESG_MAN_TEMP_SIZE = 2;
    const byte MESG_BIST_SIZE = 6;
    const byte MESG_SELFERASE_SIZE = 2;
    const byte MESG_SET_MFG_BITS_SIZE = 2;
    const byte MESG_UNLOCK_INTERFACE_SIZE = 1;
    const byte MESG_SET_SHARED_ADDRESS_SIZE = 3;

    const byte MESG_GET_GRMN_ESN_SIZE = 5;

    const byte MESG_IO_STATE_SIZE = 2;
    const byte MESG_CFG_STATE_SIZE = 2;
    const byte MESG_BLOWFUSE_SIZE = 1;
    const byte MESG_MASTERIOCTRL_SIZE = 1;
    const byte MESG_PORT_SET_IO_STATE_SIZE = 5;


    const byte MESG_SLEEP_SIZE = 1;


    const byte MESG_EXT_DATA_SIZE = 13;
    // ReSharper restore InconsistentNaming
#pragma warning restore 169 
    #endregion

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
        m_suunto.ReadTimeout = 5000;
        byte[] antPort = { MESG_TX_SYNC, 0x02, MESG_REQUEST_ID, 0x00, 0x3D, 0xD6 };
        byte[] antReset = { MESG_TX_SYNC, 0x01, MESG_SYSTEM_RESET_ID, 0x00, 0xEF };
        byte[] antCapabilities = { MESG_TX_SYNC, 0x02, MESG_REQUEST_ID, 0x00, 0x54, 0xbf };
        // byte[] antHostAssignChannel = { MESG_TX_SYNC, 0x03, MESG_ASSIGN_CHANNEL_ID, 0x00, 0x00, 0x01, 0xE4 };
        byte[] antHostAssignChannel = { MESG_TX_SYNC, 0x03, MESG_ASSIGN_CHANNEL_ID, 0x00, 0x10, 0x01, 0xF4 };
        // byte[] antHostChannelID = { MESG_TX_SYNC, 0x05, MESG_CHANNEL_ID_ID, 0x00, 0x00, 0x00, 0x00, 0x02, 0xF2 };
        byte[] antHostChannelID = { MESG_TX_SYNC, 0x05, MESG_CHANNEL_ID_ID, 0x00, 0x01, 0x00, 0x0A, 0x02, 0xF9 };
        byte[] antHostChannelSearchTimeout = { MESG_TX_SYNC, 0x02, MESG_CHANNEL_SEARCH_TIMEOUT_ID, 0x00, 0x0C, 0xEE };
        byte[] antHostChannelRfFreq = { MESG_TX_SYNC, 0x02, MESG_CHANNEL_RADIO_FREQ_ID, 0x00, 0x41, 0xA2 };
        byte[] antBeginTransmission = { MESG_TX_SYNC, 0x01, MESG_STACKLIMIT_ID, 0x00, 0xF0 };
        byte[] antMessage1 = {MESG_TX_SYNC, 0xA4, 0x01, 0x4B, 0x00, 0xEE};
        byte[] antMessage2 = { MESG_TX_SYNC, 0x09, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE3 };
        byte[] antMessage3 = { MESG_TX_SYNC, 0x09, 0x4F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE2 };

        //m_suunto.DataReceived += m_suunto_DataReceived;

        Send(antPort);
        Send(antReset);
        Send(antCapabilities);
        Send(antHostAssignChannel);
        Send(antHostChannelID);
        Send(antHostChannelSearchTimeout);
        Send(antHostChannelRfFreq);
        Send(antMessage1);
        Send(antMessage2);
        Send(antMessage3);
      }
    }

    private void Send(byte[] data)
    {
      m_suunto.Write(data, 0, data.Length);
      byte[] buffer = new byte[100];
      int cnt = 0;
      try
      {
        cnt = m_suunto.Read(buffer, 0, buffer.Length);
      }
      catch (Exception)
      {
      }
      Trace.Write("Write ");
      foreach (byte d in data)
        Trace.Write(d.ToString("X2") + " ");
      Trace.WriteLine("");
      Trace.Write("Read ");
      for (int index = 0; index < cnt; ++index)
        Trace.Write(buffer[index].ToString("X2") + " ");
      Trace.WriteLine("");
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
