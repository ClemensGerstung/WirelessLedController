using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirelessLedController.Lib;

namespace WirelessLedController.CLI
{
  class Program
  {
    static void Main(string[] args)
    {
      Controller controller = null;

      try
      {
        if (args.Length == 2)
        {
          controller = new Controller(args[0], int.Parse(args[1]));
        }
        else
        {
          Console.WriteLine("Please enter host and port to connect");
          Console.Write("Host: ");
          string host = Console.ReadLine();
          Console.Write("Port: ");
          int port = int.Parse(Console.ReadLine() ?? "0");

          controller = new Controller(host, port);
        }

        if (controller.Connected)
        {
          Console.WriteLine("Connected to {0}:{1} ({2} LEDs)", controller.Host, controller.Port, controller.LedCount);
          Console.WriteLine("Type \"set <index> <color>\" to set the color of an LED");
          Console.WriteLine("Type \"brightness <value>\" to set the overall brightness");
          Console.WriteLine("Type \"all <color>\" to set the color of all LEDs");
          Console.WriteLine("Type \"exit\" to leave");
          Console.WriteLine("Color is in the format RGBW, and must be provided in HEX");

          string cmd = "";
          do
          {
            Console.Write(">>> ");
            string[] cmds = Console.ReadLine()?
                                   .Split(new[] {' '}, 
                                          StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.ToLowerInvariant())
                                   .ToArray();

            if (cmds == null || cmds.Length == 0)
            {
              Console.WriteLine("Wrong input");
              continue;
            }

            cmd = cmds[0];

            if (cmd == "set" && cmds.Length == 3)
            {
              short index = Int16.Parse(cmds[1]);
              UInt32 color = UInt32.Parse(cmds[2], NumberStyles.HexNumber);
              controller.SetLedColor(index, color);
            }
            else if (cmd == "brightness" && cmds.Length == 2)
            {
              byte b = Byte.Parse(cmds[1], NumberStyles.HexNumber);
              controller.SetBrightness(b);
            }
            else if (cmd == "all" && cmds.Length == 2)
            {
              UInt32 color = UInt32.Parse(cmds[1], NumberStyles.HexNumber);
              controller.SetLedColor(color);
            }

          } while (cmd != "exit");

          Console.WriteLine("bye");
        }
      }
      catch
      {
        Console.WriteLine("Unexpected Exception.\nApplication will be closed");
      }
    }
  }
}
