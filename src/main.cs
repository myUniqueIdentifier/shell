using System.Net;
using System.Net.Sockets;

Console.Write("$ ");

// Wait for user input
var x = Console.ReadLine();

if (string.IsNullOrWhiteSpace(x))
{
  Console.Write("$ ");
}
else
{
  Console.Write($"{x}: command not found");
}
