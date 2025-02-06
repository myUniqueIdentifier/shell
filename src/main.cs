using System.Net;
using System.Net.Sockets;


while (true)
{
  Console.Write("$ ");

  // Wait for user input
  var line = Console.ReadLine();

  if (!string.IsNullOrWhiteSpace(line))
  {
    Console.WriteLine($"{line}: command not found");
  }
}
