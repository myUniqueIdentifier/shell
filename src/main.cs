using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Namespace
{
  static class Program
  {
    public delegate void Command(string line);

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
          var current = line.Split(' ')[0];
          if (commands.ContainsKey(current)) // internal commands
          {
            var handler = commands[current];
            handler(line);
          }
          else if (Exists(current))
          {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = current;
            var arguments = line.Split(' ').Skip(1).ToArray();
            startInfo.Arguments = string.Join(" ", arguments);
            Process.Start(startInfo);
          }
          else
          {
            Console.WriteLine($"{current}: command not found");
          }
        }
      }
    }
    // ----------------------------------------------------------------------------------------
    static void Exit(string line)
    {
      var arguments = line.Split(' ');
      try
      {
        Environment.Exit(int.Parse(arguments[1]));
      }
      catch
      {
        Console.WriteLine($"exit argument not integer.");
      }
    }
    // ----------------------------------------------------------------------------------------
    static void Echo(string line)
    {
      var text = line.Remove(0, 5);
      string pattern = @"'([^']*)'|\S+";

      MatchCollection matches = Regex.Matches(text, pattern);

      string output = "";
      foreach (Match match in matches)
      {
        string value = match.Groups[1].Success ? match.Groups[1].Value : match.Value;

        if (output.Length > 0 && !char.IsWhiteSpace(output[^1]) && !value.StartsWith(" "))
        {
          output += ""; // Voeg niks by as daar geen spasie is nie
        }
        else
        {
          output += " ";
        }

        output += value;
      }

      Console.WriteLine(output.Trim());
    }
    // ----------------------------------------------------------------------------------------
    static void Type(string line)
    {
      var arguments = line.Split(' ');
      if (arguments.Count() < 2)
      {
        Console.WriteLine($"type argument missing.");
        return;
      }
      var argument = arguments[1];
      if (commands.ContainsKey(argument))
      {
        Console.WriteLine($"{argument} is a shell builtin");
        return;
      }

      var  location = GetFullPath(argument);
      if(location is not null)
      {
        Console.WriteLine($"{argument} is {location}");
      }
      else
      {
        Console.WriteLine($"{argument}: not found");
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
        if (File.Exists(fullPath))
        {
          return fullPath;
        }
      }
      return null;
    }

    // ----------------------------------------------------------------------------------------
    // Print Working Directory
    static void Pwd(string line)
    {
      Console.WriteLine (Directory.GetCurrentDirectory());
    }

    // ----------------------------------------------------------------------------------------
    // Change Directory
    static void Cd(string line)
    {
      var argument = line.Split(' ').Skip(1).ToArray();
      if (argument[0] == "~")
      {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Directory.SetCurrentDirectory(home);
      }
      else
      {
        try
        {
          Directory.SetCurrentDirectory(string.Join(" ", argument));
        }
        catch
        {
          Console.WriteLine($"cd: {string.Join(" ", argument)}: No such file or directory");
        }
      }
    }
  }
}
