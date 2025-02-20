using static System.Console;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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
      { "pwd", Pwd },
      { "cd", Cd }
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
    static string[] ParseInput_(string input)
    {
      ArgumentNullException.ThrowIfNull(input);
      var regex = new Regex(@"'([^']*(?:''[^']*)*)'|(\S+)");
      var matches = regex.Matches(input);
      var arguments = matches.Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).ToArray();
      return arguments.Select(word => word.Replace("''", "")).ToArray();
    }
    static bool ParseInput(string input, out string command, out List<string> arguments)
    {
      string pattern = @"(\S+)";
      var regex = new Regex(pattern);
      Match command_result = regex.Match(input);
      command = command_result.Value;
      arguments = new List<string>();

      if (command_result.Success)
      {
        var command_length = command_result.Length + command_result.Index;
        var remaining = input.Remove(0, command_length).TrimStart();

        while (!string.IsNullOrEmpty(remaining))
        {
          if ((remaining[0] == '\'') && (remaining.AsSpan().Count('\'') > 1))
          {
            string single_pattern = @"'((?:[^']| '')*)'";
            var single_regex = new Regex(single_pattern);
            var argument = single_regex.Match(remaining);
            if (argument.Success)
            {
              arguments.Add(argument.Groups[1].Value.Replace("''", ""));
              var argument_length = argument.Length + argument.Index;
              remaining = remaining.Remove(0, argument_length).TrimStart();
            }
            else
            {
              break;
            }
          }
          else if ((remaining[0] == '\"') && (remaining.AsSpan().Count('\"') > 1))
          {
            string single_pattern = @"""((?:[^""]|"""")*)""";
            var single_regex = new Regex(single_pattern);
            var argument = single_regex.Match(remaining);
            if (argument.Success)
            {
              arguments.Add(argument.Groups[1].Value.Replace("\"\"", ""));
              var argument_length = argument.Length + argument.Index;
              remaining = remaining.Remove(0, argument_length).TrimStart();
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
              arguments.Add(argument.Value);
              var argument_length = argument.Length + argument.Index;
              remaining = remaining.Remove(0, argument_length).TrimStart();
            }
            else
            {
              break;
            }
          }
        }
      }
      return command_result.Success;
    }
  }
}
