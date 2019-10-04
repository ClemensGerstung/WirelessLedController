using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace WirelessLedController.Lib
{
  internal class BitConverter
  {
    public static short ToInt16(byte[] value, int startIndex)
    {
      if (System.BitConverter.IsLittleEndian)
      {
        Array.Reverse(value);
      }

      return System.BitConverter.ToInt16(value, startIndex);
    }

    public static Array GetBytes(short value)
    {
      var array = System.BitConverter.GetBytes(value);
      if (System.BitConverter.IsLittleEndian)
      {
        Array.Reverse(array);
      }

      return array;
    }

    public static Array GetBytes(uint value)
    {
      var array = System.BitConverter.GetBytes(value);
      if (System.BitConverter.IsLittleEndian)
      {
        Array.Reverse(array);
      }

      return array;
    }

    public static uint ToUInt32(byte[] value, int startIndex)
    {
      if (System.BitConverter.IsLittleEndian)
      {
        Array.Reverse(value);
      }

      return System.BitConverter.ToUInt32(value, startIndex);
    }
  }

  internal enum Code : byte
  {
    Count = 0x01,
    SetLed = 0x02,
    GetLed = 0x03,
    SetLeds = 0x04,
    GetLeds = 0x05,
    SetBrightness = 0x06,
    GetBrightness = 0x07
  }

  public class Controller : IDisposable
  {
    private readonly TcpClient _client;
    private short? _ledCount;

    /// <summary>
    /// Gets the count of LEDs programmed onto the ESP
    /// </summary>
    public short LedCount
    {
      get
      {
        if (!_ledCount.HasValue)
        {
          Stream stream = _client.GetStream();
          stream.WriteByte((byte) Code.Count);

          byte[] data = new byte[2];
          int read = stream.Read(data, 0, 2);
          if (read == 2)
          {
            _ledCount = BitConverter.ToInt16(data, 0);
          }
        }

        return _ledCount.Value;
      }
    }

    /// <summary>
    /// Gets if the controller is connected to the ESP
    /// </summary>
    public bool Connected => _client.Connected;

    public string Host { get; }

    public int Port { get; }

    /// <summary>
    /// Initializes the controller by connecting to the ESP
    /// </summary>
    /// <param name="host">Name or EndPoint-Name of the ESP</param>
    /// <param name="port">Port on the ESP</param>
    public Controller(string host, int port)
    {
      this.Host = host;
      this.Port = port;
      _client = new TcpClient(host, port);
    }

    /// <summary>
    /// Sets the color of an single LED
    /// </summary>
    /// <param name="index">0 based index</param>
    /// <param name="color">32bit color in RGBW</param>
    public void SetLedColor(short index, uint color)
    {
      byte[] data = new byte[7];
      data[0] = (byte) Code.SetLed;

      Array.Copy(BitConverter.GetBytes(index), 0, data, 1, 2);
      Array.Copy(BitConverter.GetBytes(color), 0, data, 3, 4);

      Stream stream = _client.GetStream();
      stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Gets the color of an LED
    /// </summary>
    /// <param name="index">0-based index of the LED</param>
    /// <returns>The color in RGBW</returns>
    public uint GetLedColor(short index)
    {
      byte[] data = new byte[4];
      data[0] = (byte) Code.GetLed;
      Array.Copy(BitConverter.GetBytes(index), 0, data, 1, 2);

      Stream stream = _client.GetStream();
      stream.Write(data, 0, 3);
      int read = stream.Read(data, 0, 4);
      if (read == 4)
      {
        return BitConverter.ToUInt32(data, 0);
      }

      return 0;
    }

    /// <summary>
    /// Sets the colors of all LEDs in the given range
    /// </summary>
    /// <param name="from">First LED index (0-based)</param>
    /// <param name="to">Last LED index (0-based, including)</param>
    /// <param name="colors">Colors for the LEDs</param>
    public void SetLedColor(short from, short to, uint[] colors)
    {
      if (colors.Length != (to - from))
      {
        throw new ArgumentException("Amount of colors must be the same as the range (to - from)");
      }

      byte[] data = new byte[1 + 2 + 2 + colors.Length * 4];
      data[0] = (byte) Code.SetLeds;
      Array.Copy(BitConverter.GetBytes(from), 0, data, 1, 2);
      Array.Copy(BitConverter.GetBytes(to), 0, data, 3, 2);

      int offset = 5;
      foreach (uint color in colors)
      {
        Array.Copy(BitConverter.GetBytes(color), 0, data, offset, 4);

        offset += 4;
      }

      Stream stream = _client.GetStream();
      stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Gets the colors of a range of LEDs
    /// </summary>
    /// <param name="from">First LED index (0-based)</param>
    /// <param name="to">Last LED index (0-based, including)</param>
    /// <returns>Array of all colors in RGBW</returns>
    public uint[] GetLedColor(short from, short to)
    {
      byte[] data = new byte[5];
      data[0] = (byte) Code.GetLeds;
      Array.Copy(BitConverter.GetBytes(from), 0, data, 1, 2);
      Array.Copy(BitConverter.GetBytes(to), 0, data, 3, 2);

      Stream stream = _client.GetStream();
      stream.Write(data, 0, data.Length);

      uint[] colors = new uint[to - from];

      byte[] color = new byte[4];
      for (int index = 0; index < colors.Length; index++)
      {
        if (stream.Read(color, 0, 4) == 4)
        {
          colors[index] = BitConverter.ToUInt32(color, 0);
        }
      }

      return colors;
    }

    /// <summary>
    /// Sets the color of all LEDs at the same time
    /// </summary>
    /// <param name="color">Color to use</param>
    public void SetLedColor(uint color)
    {
      uint[] colors = new uint[LedCount];
      for (int i = 0; i < colors.Length; i++)
      {
        colors[i] = color;
      }

      SetLedColor(0, LedCount, colors);
    }

    /// <summary>
    /// Sets the brightness of the leds
    /// </summary>
    /// <param name="b">Brightness to use</param>
    public void SetBrightness(byte b)
    {
      byte[] data = {(byte) Code.SetBrightness, b};
      Stream stream = _client.GetStream();
      stream.Write(data, 0, 2);
    }

    /// <summary>
    /// Gets the current brightness of the LEDs
    /// </summary>
    /// <returns>The brightness</returns>
    public byte GetBrightness()
    {
      Stream stream = _client.GetStream();
      stream.WriteByte((byte) Code.GetBrightness);

      int r = stream.ReadByte();
      if (r == -1)
      {
        return 0;
      }

      return (byte) r;
    }

    /// <summary>
    /// Disposes the object
    /// </summary>
    public void Dispose()
    {
      _client?.Close();
      _client?.Dispose();
    }
  }
}