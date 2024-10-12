// See https://aka.ms/new-console-template for more information

using Context.Models;
using Pooshit.Json;


SnakeData data = Json.Read<SnakeData>("{\"over_the_top\":7}");
Console.WriteLine(Json.WriteString(data));