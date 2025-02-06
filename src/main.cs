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
  else if (!string.IsNullOrWhiteSpace(line))
  {
    Console.WriteLine($"{line}: command not found");
  }
  else if (line.Contains("exit 0"))
  {
    Environment.Exit(0);
  }
}
