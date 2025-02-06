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
  else
  {
    Console.WriteLine($"{line}: command not found");
  }
}
