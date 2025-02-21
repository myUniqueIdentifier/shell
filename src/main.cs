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
      string pattern = @"(?:'((?:[^']|'')*)'|""((?:[^""]| "")*)"" | (\S +))";
    var regex = new Regex(pattern);
      arguments = new List<string>();

      var matches = regex.Matches(input);
      if (matches.Count == 0)
      {
        command = string.Empty;
        return false;
      }

      command = matches[0].Groups[0].Value;
      for (int i = 1; i < matches.Count; i++)
      {
        string arg = matches[i].Groups[1].Success ? matches[i].Groups[1].Value.Replace("''", "'") :
                     matches[i].Groups[2].Success ? matches[i].Groups[2].Value.Replace("\"\"", "\"") :
                     matches[i].Groups[3].Value;
        arguments.Add(arg);
      }

      return true;
    }

    static bool _ParseInput(string input, out string command, out List<string> arguments)
    {
      string pattern = @"(\S+)";
      var regex = new Regex(pattern);
      string single_pattern = @"'((?:[^']| '')*)'";
      var single_regex = new Regex(single_pattern);
      string double_pattern = @"""((?:[^""]|"""")*)""";
      var double_regex = new Regex(double_pattern);


      Match command_result = regex.Match(input);
      command = command_result.Value;
      arguments = new List<string>();

      if (!command_result.Success)
      {
        return false;
      }

      string remaining = string.Empty;

      var command_length = command_result.Length + command_result.Index;
      remaining = input.Remove(0, command_length).TrimStart();

      while (!string.IsNullOrEmpty(remaining))
      {
        if (parse_as(remaining, '\''))
        {
          var argument = single_regex.Match(remaining);
          if (argument.Success)
          {
            extract(argument, ref arguments, "\'");
          }
          else
          {
            break;
          }
        }
        else if (parse_as(remaining, '\"'))
        {
          var argument = double_regex.Match(remaining);
          if (argument.Success)
          {
            extract(argument, ref arguments, "\"");
          }
          else
          {
            break;
          }
        }
        else
        {
          var argument = regex.Match(remaining);
          if (argument.Success)
          {
            extract(argument, ref arguments, "");
          }
          else
          {
            break;
          }
        }
      }

      return command_result.Success;

      void extract(Match argument, ref List<string> _arguments, string remove)
      {
        _arguments.Add(argument.Groups[1].Value.Replace(remove, ""));
        var argument_length = argument.Length + argument.Index;
        remaining = remaining.Remove(0, argument_length).TrimStart();
      }

      bool parse_as(string content, char character)
      {
        return (remaining[0] == character) && (remaining.AsSpan().Count(character) > 1);
      }
    }
  }
}
