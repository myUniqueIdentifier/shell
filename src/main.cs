using static System.Console;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;

namespace Program
{
  static class Program
  {
    public delegate void Command(List<string> arguments);

    static readonly Dictionary<string, Command> commands = new Dictionary<string, Command>()
    {
      { "exit", Exit },
      { "echo", Echo },
      { "type", Type },
      { "pwd",  Pwd },
      { "cd",   Cd }
    };

    // ----------------------------------------------------------------------------------------
    static void Main(string[] args)
    {
      while (true)
      {
        Console.Write("$ ");

        // Wait for user input
        var line = Console.ReadLine();

        if (line == null)
        {
          Environment.Exit(0);
        }
        else
        {
          if (!ParseInput(line, out string command, out List<string> arguments))
          {
            continue;
          }

          if (commands.ContainsKey(command)) // internal commands
          {
            var handler = commands[command];
            handler(arguments);
          }
          else if (Exists(command))
          {
            var startInfo = new ProcessStartInfo
            {
              FileName = command,
              Arguments = string.Join(" ", arguments.Select(s => s.Contains(' ') ? $"\"{s}\"" : s))
            };
            Process.Start(startInfo)?.WaitForExit();
          }
          else
          {
            WriteLine($"{command}: command not found");
          }
        }
      }
    }

    // ----------------------------------------------------------------------------------------
    static void Exit(List<string> arguments)
    {
      try
      {
        Environment.Exit(int.Parse(arguments.First()));
      }
      catch
      {
        WriteLine($"exit argument not integer.");
      }
    }
    // ----------------------------------------------------------------------------------------
    static void Echo(List<string> arguments)
    {
      WriteLine(string.Join(" ", arguments));
    }
    // ----------------------------------------------------------------------------------------
    static void Type(List<string> arguments)
    {
      if (arguments.Count() < 1)
      {
        WriteLine($"type argument missing.");
        return;
      }
      var argument = arguments.First();
      if (commands.ContainsKey(argument))
      {
        WriteLine($"{argument} is a shell builtin");
        return;
      }

      var  location = GetFullPath(argument);
      if(location is not null)
      {
        WriteLine($"{argument} is {location}");
      }
      else
      {
        WriteLine($"{argument}: not found");
      }
    }
    // ----------------------------------------------------------------------------------------
    static bool Exists(string fileName)
    {
      return GetFullPath(fileName) != null;
    }
    // ----------------------------------------------------------------------------------------
    static string? GetFullPath(string fileName)
    {
      if (File.Exists(fileName))
      {
        return Path.GetFullPath(fileName);
      }

      var values = Environment.GetEnvironmentVariable("PATH");
      if ( values is null)
      {
        return null;
      }
      foreach (var path in values.Split(Path.PathSeparator))
      {
        var fullPath = Path.Combine(path, fileName);
        var extention = fileName.Substring(Math.Max(0, fileName.Length - 4));
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !extention.Equals(".exe"))
        {
          fullPath += ".exe";
        }
        if (File.Exists(fullPath))
        {
          return fullPath;
        }
      }
      return null;
    }

    // ----------------------------------------------------------------------------------------
    // Print Working Directory
    static void Pwd(List<string> arguments)
    {
      WriteLine (Directory.GetCurrentDirectory());
    }

    // ----------------------------------------------------------------------------------------
    // Change Directory
    static void Cd(List<string> arguments)
    {
      if (arguments[0] == "~")
      {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Directory.SetCurrentDirectory(home);
      }
      else
      {
        try
        {
          Directory.SetCurrentDirectory(string.Join(" ", arguments));
        }
        catch
        {
          WriteLine($"cd: {string.Join(" ", arguments)}: No such file or directory");
        }
      }
    }

    // ----------------------------------------------------------------------------------------
    // Parse command line. Extract command and arguments.
    static bool ParseInput(string input, out string command, out List<string> arguments)
    {
      //string pattern = "\\s*(?:'((?:[^']|\\')*)'|\"((?:[^\"]|\\\")*)\"|(\\S+))";
      string @pattern = @"(?:'((?:[^']|'')*)'|""((?:[^""]|"""")*)""|(\S+))";
      var regex = new Regex(pattern);
      arguments = [];

      var matches = regex.Matches(input);
      if (matches.Count == 0)
      {
        command = string.Empty;
        return false;
      }

      command = matches[0].Groups[0].Value.TrimStart();

      for (int i = 1; i < matches.Count; i++)
      {
        string arg = string.Empty;
        if (matches[i].Groups[1].Success)
        {
          arg = matches[i].Groups[2].Value.Replace("\"\"", "").QouteReplace();
        }
        else if (matches[i].Groups[2].Success)
        {
          arg = matches[i].Groups[1].Value.Replace("''", "");
        }
        else
        {
          matches[i].Groups[3].Value.Replace("\\", "");
        }

        arguments.Add(arg);
      }
      return true;
    }
  }

  static class Extender
  {
    public static string QouteReplace(this string that)
    {
      const string pattern1 = @"\\([\$""\\])";
      var temp =  Regex.Replace(that, pattern1, "$1");
      return temp;
    }

    public static string RemoveUnescapedQuotes(this string that)
    {
      return Regex.Replace(that, @"(?<!\\)\""", "").Trim();
    }
  }
}
