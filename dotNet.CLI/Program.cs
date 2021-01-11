using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirelessLedController.Lib;
using CommandLine;
using System.Threading;

namespace WirelessLedController.CLI
{
  class Options
  {
    [Option('s', "server", Required = true)]
    public string Server { get; set; }

    [Option('p', "port", Required = true)]
    public int Port { get; set; }

    [Option('c', "command")]
    public IEnumerable<string> Command { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      Controller controller = null;

      try
      {
        var parsed = Parser.Default.ParseArguments<Options>(args);

        if (parsed.Tag == ParserResultType.Parsed)
        {
          Options options = ((Parsed<Options>)parsed).Value;

          controller = new Controller(options.Server, options.Port);

          if(options.Command.Any())
          {
            while (!controller.Connected) ;

            HandleCommands(options.Command.ToArray(),
                           controller);

            // todo: find better way to see if data are sent
            Thread.Sleep(100);

            controller.Dispose();
            return;
          }
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
            string[] cmds = Console.ReadLine()
                                   .Split(new[] {' '}, 
                                          StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.ToLowerInvariant())
                                   .ToArray();

            if (cmds == null || cmds.Length == 0)
            {
              Console.WriteLine("Wrong input");
              continue;
            }

            HandleCommands(cmds, controller);

          } while (cmd != "exit");

          Console.WriteLine("bye");
        }
      }
      catch
      {
        Console.WriteLine("Unexpected Exception.\nApplication will be closed");
      }
      finally
      {
        controller.Dispose();
      }
    }

    static void HandleCommands(string[] cmds, Controller controller)
    {
      string cmd = cmds[0];

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
    }
  }
}
