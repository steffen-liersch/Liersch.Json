[![NuGet](https://img.shields.io/nuget/v/Liersch.Json.svg)](https://www.nuget.org/packages/Liersch.Json)

# Liersch.Json - JSON Support for .NET

**Version 2.x is currently being developed on [this branch](https://github.com/steffen-liersch/Liersch.Json/tree/dev).**

`Liersch.Json` is a small .NET library for parsing and generating JSON documents. The library is written in C# 3.0 supporting the following .NET platforms:

- from .NET Framework 2.0
- from .NET Core 1.0
- from .NET Standard 1.0
- Mono
- .NET Micro Framework 4.4

The file size of the compiled library is only ≈25 kB.  All major changes are logged in the [CHANGELOG.md](https://github.com/steffen-liersch/Liersch.Json/blob/dev/CHANGELOG.md) file.

## Getting Started

The easiest and the fastest way to integrate the library into a project is to use the [Liersch.Json package published on NuGet](https://www.nuget.org/packages/Liersch.Json). For older projects (before .NET Framework 4.0) the library has to be compiled and integrated manually. In order to compile the project for the outdated .NET Micro Framework, the compiler symbol `NETMF` must be defined in addition.

## Migration from Version 1.x

- The type name prefix `SL` used in version 1.x has been removed in version 2.x.
- The reflection-based classes have been moved to a new [Liersch.JsonSerialization](https://github.com/steffen-liersch/Liersch.JsonSerialization) library.

## Parsing JSON Documents

The static function `JsonNode.Parse` should be used to parse JSON document in restrictive mode. The input document should be formatted correctly. Otherwise a `JsonException` is thrown.

For less restrictive parsing an instance of `JsonParser` must be created. There are different properties to configure the parsing mode. In JSON documents, strings must be delimited by double-quotation marks. The parser also accepts single-quotation marks if property `AreSingleQuotesAllowed` is `true`.

```cs
public static void RunExample1()
{
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
}
```

## JsonNode Class

The parser result is an instance of `JsonNode`. It can be used to process the parsed JSON document. `JsonNode` implements `IEnumerable` to enumerate all sub nodes for arrays and objects. In addition there is the property `Names` that can be used for objects to enumerate all property names.

```cs
public static void RunExample2()
{
  string json=RetrieveJsonExample();
  PrintNode(JsonNode.Parse(json), "demo = ", 0);
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
        PrintNode(node[i], "["+i.ToString(CultureInfo.InvariantCulture)+"] = ", level+1);
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
```

The following properties can be used to read and write values: `AsBoolean`, `AsInt32`, `AsInt64`, `AsDouble` and `AsString`. If on reading a property, the value cannot be converted to the corresponding data type, the default value of the data type is returned instead.

If accessing a missing object or if using an invalid array index, no exception is thrown. Instead an empty node is returned. If a value setter is used for a not existing value, the value is created. `JsonNode` can also be used to create new JSON documents.

```cs
public static void RunExample3()
{
  var root=new JsonNode();
  root["addressBook"]=CreateAddressBook();
  Console.WriteLine(root.AsJson);
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
```

## JsonMonitor Class

The `JsonNode.CreateMonitor` function can be used to create an instance of class `JsonMonitor`. It's only allowed for root nodes and it must not be called several times.

`JsonMonitor.IsModified` is set to `true` on any change. `JsonMonitor.IsReadOnly` can be used to disallow changing any node. If passing root nodes to external code, this property can be used to avoid unexpected side effects.

## JsonWriter Class

The writer class `JsonWriter` has a very small footprint. It has a good performance, but no checks for incorrect use.

## License

The software is published under the conditions of an open source license. Alternatively, other terms can be agreed under a commercial license. You can support the maintenance and further development of the software with a [voluntary donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=NVXEQCNGJFK92).

## Copyright

Copyright © 2013-2020 Steffen Liersch  
https://www.steffen-liersch.de/

## Links

The source code is maintained on GitHub:  
https://github.com/steffen-liersch/Liersch.Json

Packages can be downloaded through NuGet:  
https://www.nuget.org/packages/Liersch.Json
