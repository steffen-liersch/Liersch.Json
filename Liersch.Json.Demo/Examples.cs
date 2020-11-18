/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;

namespace Liersch.Json
{
  static class Examples
  {
    public static void RunExample1()
    {
      Console.WriteLine("Example 1");
      Console.WriteLine("=========");
      Console.WriteLine();

      string json=@"
      {
        ""addressBook"": [
          {""lastName"": ""Average"", ""firstName"": ""Joe""},
          {""lastName"": ""Doe"", ""firstName"": ""Jane""},
          {""lastName"": ""Smith"", ""firstName"": ""John""}
        ]
      }";

      var root=JsonNode.Parse(json);
      JsonNode book=root["addressBook"];
      if(book.IsArray)
      {
        int c=book.Count;
        for(int i=0; i<c; i++)
        {
          JsonNode entry=book[i];
          string ln=entry["lastName"];
          string fn=entry["firstName"];
          Console.WriteLine(fn+" "+ln);
        }
      }

      Console.WriteLine();
    }


    public static void RunExample2()
    {
      Console.WriteLine("Example 2");
      Console.WriteLine("=========");
      Console.WriteLine();

      string json=RetrieveJsonExample();
      PrintNode(JsonNode.Parse(json), "demo = ", 0);
      Console.WriteLine();
    }

    static void PrintNode(JsonNode node, string prefix, int level)
    {
      Console.Write(new string(' ', level*2));
      Console.Write(prefix);

      switch(node.NodeType)
      {
        case JsonNodeType.Array:
          Console.WriteLine("(Array)");
          int c=node.Count;
          for(int i=0; i<c; i++)
            PrintNode(node[i], "["+i.ToString(/*CultureInfo.InvariantCulture*/)+"] = ", level+1);
          break;

        case JsonNodeType.Object:
          Console.WriteLine("(Object)");
          foreach(string name in node.Names)
            PrintNode(node[name], name+" = ", level+1);
          break;

        case JsonNodeType.Boolean:
        case JsonNodeType.Number:
        case JsonNodeType.String:
          Console.WriteLine(node.AsString+" ("+node.NodeType.ToString()+")");
          break;

        default:
          Console.WriteLine("("+node.NodeType.ToString()+")");
          break;
      }
    }

    static string RetrieveJsonExample()
    {
      return @"
      {
        ""sensors"": [
          { ""name"": ""Button"", ""value"": 1 },
          { ""name"": ""Temperature"", ""value"": 17.5 }
        ],

        ""actors"": [
          { ""name"": ""Lamp"", ""value"": 0 }
        ],

        ""debug"": true,
        ""timestamp"": ""2017-12-30 18:10:35"",
        ""unassigned"": null
      }";
    }


    public static void RunExample3()
    {
      Console.WriteLine("Example 3");
      Console.WriteLine("=========");
      Console.WriteLine();

      var root=new JsonNode();
      root["addressBook"]=CreateAddressBook();
      Console.WriteLine(root.AsJson);
      Console.WriteLine();
    }

    static JsonNode CreateAddressBook()
    {
      var book=new JsonNode();

      book[0]["LastName"]="Average";
      book[0]["firstName"]="Joe";

      book[1]["LastName"]="Doe";
      book[1]["firstName"]="Jane";

      book[2]["LastName"]="Smith";
      book[2]["firstName"]="John";

      return book;
    }


    public static void RunExample4()
    {
      Console.WriteLine("Example 4");
      Console.WriteLine("=========");
      Console.WriteLine();

      string s=@"
      {
        ""options"": { ""logging"": true },

        ""sensors"": [
          { ""name"": ""Button"", ""value"": 1 },
          { ""name"": ""Temperature"", ""value"": 17.5 }
        ],

        ""actors"": [
          { ""name"": ""Lamp"", ""value"": 0 }
        ]
      }";

      /*
      Console.WriteLine(s.Trim());
      Console.WriteLine();
      //*/

      var n=JsonNode.Parse(s);
      Console.WriteLine("n.AsJsonCompact          => "+n.AsJsonCompact);
      Console.WriteLine();

      Console.WriteLine("n[\"options\"].NodeType    => "   + n["options"].NodeType);
      Console.WriteLine("n[\"options\"][\"logging\"]  => " + n["options"]["logging"]);
      Console.WriteLine("n[\"sensors\"].NodeType    => "   + n["sensors"].NodeType);
      Console.WriteLine("n[\"sensors\"].Count       => "   + n["sensors"].Count);
      Console.WriteLine("n[\"sensors\"][0][\"name\"]  => " + n["sensors"][0]["name"]);
      Console.WriteLine("n[\"sensors\"][1][\"value\"] => " + n["sensors"][1]["value"]);
      Console.WriteLine("n[\"missing\"][33][\"foo\"]  => " + n["missing"][33]["foo"]);
      Console.WriteLine();
    }
  }
}