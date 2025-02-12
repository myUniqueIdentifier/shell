using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Namespace
{
  static class Program
  {
    public delegate void Command(string [] arguments);

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
          var temp = ParseInput(line);
          var arguments = temp.Skip(1).ToArray();
          var command = temp[0];

          if (commands.ContainsKey(command)) // internal commands
          {
            var handler = commands[command];
            handler(arguments);
          }
          else if (Exists(command))
          {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = command;
            startInfo.Arguments = string.Join(" ", arguments);
            Process.Start(startInfo);
          }
          else
          {
            Console.WriteLine($"{command}: command not found");
          }
        }
      }
    }
    // ----------------------------------------------------------------------------------------
    static void Exit(string [] arguments)
    {
      try
      {
        Environment.Exit(int.Parse(arguments.First()));
      }
      catch
      {
        Console.WriteLine($"exit argument not integer.");
      }
    }
    // ----------------------------------------------------------------------------------------
    static void Echo(string[] arguments)
    {
      Console.WriteLine(string.Join(" ", arguments));
    }
    // ----------------------------------------------------------------------------------------
    static void Type(string[] arguments)
    {
      if (arguments.Count() < 1)
      {
        Console.WriteLine($"type argument missing.");
        return;
      }
      var argument = arguments.First();
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
    static void Pwd(string[] arguments)
    {
      Console.WriteLine (Directory.GetCurrentDirectory());
    }

    // ----------------------------------------------------------------------------------------
    // Change Directory
    static void Cd(string[] arguments)
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
          Console.WriteLine($"cd: {string.Join(" ", arguments)}: No such file or directory");
        }
      }
    }
    static string[] ParseInput(string input)
    {
      ArgumentNullException.ThrowIfNull(input);
      var regex = new Regex(@"'([^']*)'|(\S+)");
      var matches = regex.Matches(input);
      return matches.Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).ToArray();
    }
  }
}
