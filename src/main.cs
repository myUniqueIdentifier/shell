using System.Net;
using System.Net.Sockets;

Console.Write("$ ");

while (true)
{
  // Wait for user input
  var line = Console.ReadLine();

  if (string.IsNullOrWhiteSpace(line))
  {
    Console.Write("$ ");
  }
  else
  {
    Console.Write($"{line}: command not found");
  }
}
