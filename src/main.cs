using System.Collections.Generic;

namespace Namespace
{
  class Program
  {
    public delegate void Command(string line);

    static readonly Dictionary<string, Command> commands = new Dictionary<string, Command>()
    {
      { "exit", Exit },
      { "echo", Echo },
      { "type", Type }
    };

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
          try
          {
            var current = line.Split(' ')[0];
            var handler = commands[current];
            handler(line);
          }
          catch
          {
            Console.WriteLine($"{line}: command not found");
          }
        }
      }
    }
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
    static void Echo(string line)
    {
      var arguments = line.Split(' ').Skip(1).ToArray();
      Console.WriteLine(string.Join(" ", arguments));
    }
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
      if(location is null)
      {
        Console.WriteLine($"{argument} is {location}");
      }
      else
      {
        Console.WriteLine($"{argument}: not found");
      }
    }
    static string GetFullPath(string fileName)
    {
      if (File.Exists(fileName))
      {
        return Path.GetFullPath(fileName);
      }

      var values = Environment.GetEnvironmentVariable("PATH");
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
  }
}
