using static System.Console;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;

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
          var commandline = ParseInput (line);
          if (commandline.Count == 0)
          {
            continue;
          }

          var command = commandline[0];
          var arguments = commandline.Skip(1).ToList();

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
              Arguments = string.Join(" ", arguments.Select(arg => "'" + arg + "'"))
            };
            WriteLine($"DEBUG: {startInfo.FileName} {startInfo.Arguments}");
            Process.Start(startInfo)?.WaitForExit();
            WriteLine("");
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
    static List<string> ParseInput(string input)
    {
      var result = new List<string>();
      bool inDoubleQuotes = false;
      bool inSingleQuotes = false;
      var currentWord = new List<char>();

      for (int i = 0; i < input.Length; i++)
      {
        char c = input[i];

        if (c == '\\' && i + 1 < input.Length)
        {
          char nextChar = input[i + 1];
          if (inDoubleQuotes && (nextChar == '\\' || nextChar == '$' || nextChar == '"'))
          {
            currentWord.Add(nextChar); // Preserve escaped \ or $ or " inside double quotes
            i++;
          }
          else if (!inDoubleQuotes && !inSingleQuotes)
          {
            currentWord.Add(nextChar); // Preserve next character as is
            i++;
          }
          else
          {
            currentWord.Add(c); // Add backslash as is if not followed by escape-worthy character
          }
        }
        else if (c == '"' && !inSingleQuotes)
        {
          if (inDoubleQuotes && i + 1 < input.Length && input[i + 1] == '"')
          {
            currentWord.Add('"'); // Convert double " inside to a single "
            i++;
          }
          else
          {
            inDoubleQuotes = !inDoubleQuotes;
          }
        }
        else if (c == '\'' && !inDoubleQuotes)
        {
          inSingleQuotes = !inSingleQuotes;
        }
        else if (c == ' ' && !inDoubleQuotes && !inSingleQuotes)
        {
          if (currentWord.Count > 0)
          {
            result.Add(new string(currentWord.ToArray()));
            currentWord.Clear();
          }
        }
        else
        {
          currentWord.Add(c);
        }
      }

      if (currentWord.Count > 0)
      {
        string word = new string(currentWord.ToArray());

        result.Add(word);
      }

      return result;
    }
  }
}
