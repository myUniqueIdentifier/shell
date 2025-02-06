using System.Net;
using System.Net.Sockets;


while (true)
{
  Console.Write("$ ");

  // Wait for user input
  var line = Console.ReadLine();

  if (line == null)
  {
    Environment.Exit(0);
  }
  else if (line.StartsWith("exit"))
  {
    var arguments = line.Split(' ');
    Environment.Exit(int.Parse(arguments[1]));
  }
  else if (line.StartsWith("echo"))
  {
    var arguments = line.Split(' ').Skip(1).ToArray();
    Console.WriteLine(string.Join(" ", arguments));
  }
  else
  {
    Console.WriteLine($"{line}: command not found");
  }
}
