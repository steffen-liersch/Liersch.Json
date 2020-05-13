[![NuGet](https://img.shields.io/nuget/v/Liersch.Json.svg)](https://www.nuget.org/packages/Liersch.Json)


Liersch.Json - JSON Support for .NET
====================================

Liersch.Json provides support for parsing and generating JSON expressions. The library is written in C# 3.0 targeting many .NET platforms. The following platforms are explicitly supported:

- .NET Framework 2.0, 3.0, 3.5, 4.0, 4.5 and Mono
- .NET Core 1.0
- .NET Standard 1.0
- .NET Micro Framework 4.4 (excluding reflection-based features)

Changes are logged in file [CHANGELOG.md](https://github.com/steffen-liersch/Liersch.Json/blob/master/CHANGELOG.md).

[You can support maintenance and further development with a voluntary donation.](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=NVXEQCNGJFK92)


Getting Started
---------------

The following solutions can be used to try the library:

- "Liersch.Json_VS2017.sln" - demo for multiple target frameworks (VS 2017 required)
- "Liersch.Json_VS2013.sln" - demo for .NET Framework 3.5 and Mono (VS 2013 required)
- "Liersch.Json_netmf.sln" - demo for .NET Micro Framework 4.4 (VS 2015 required)

To use the library in a project one of the following library project files can be included into the solution, depending on the used IDE and the target framework:

- "Liersch.Json_sdk.csproj" - library project for multiple target frameworks (VS 2017 required)
- "Liersch.Json_net20.csproj" - library project for .NET Framework 2.0 and Mono (VS 2013 required)
- "Liersch.Json_net35.csproj" - library project for .NET Framework 3.5 and Mono (VS 2013 required)
- "Liersch.Json_net45.csproj" - library project for .NET Framework 4.5 and Mono (VS 2013 required)
- "Liersch.Json_netmf.csproj" - library project for .NET Micro Framework 4.4 (VS 2015 required)

The easiest and the fastest way to integrate the library into a project is to use the [Liersch.Json package published on NuGet](https://www.nuget.org/packages/Liersch.Json).


Parsing
-------

The static function SLJsonNode.Parse should be used to parse JSON expressions in restrictive mode. The input JSON expression should be formatted correctly. Otherwise a SLJsonException is thrown.

For less restrictive parsing an instance of SLJsonParser must be created. There are different properties to configure the parsing mode. In JSON strings must be delimited by double-quotation marks. The parser also accepts single-quotation marks if property AreSingleQuotesAllowed is true.


```cs
public static void RunExample1()
{
  string jsonExpression=@"
  {
    ""addressBook"": [
      {""lastName"": ""Average"", ""firstName"": ""Joe""},
      {""lastName"": ""Doe"", ""firstName"": ""Jane""},
      {""lastName"": ""Smith"", ""firstName"": ""John""}
    ]
  }";

  var root=SLJsonNode.Parse(jsonExpression);
  SLJsonNode book=root["addressBook"];
  if(book.IsArray)
  {
    int c=book.Count;
    for(int i=0; i<c; i++)
    {
      SLJsonNode entry=book[i];
      string ln=entry["lastName"];
      string fn=entry["firstName"];
      Console.WriteLine(fn+" "+ln);
    }
  }
}
```


SLJsonNode
----------

The parser result is an instance of SLJsonNode. It can be used to analyze the parsed JSON expression. SLJsonNode implements IEnumerable. For arrays and objects all sub nodes are enumerated. In addition there is a property Names. It can be used for objects to enumerate the property names.

```cs
public static void RunExample2()
{
  string jsonExpression=RetrieveJsonExample();
  PrintNode(SLJsonNode.Parse(jsonExpression), "demo = ", 0);
}

static void PrintNode(SLJsonNode node, string prefix, int level)
{
  Console.Write(new String(' ', level*2));
  Console.Write(prefix);

  switch(node.NodeType)
  {
    case SLJsonNodeType.Array:
      Console.WriteLine("(Array)");
      int c=node.Count;
      for(int i=0; i<c; i++)
        PrintNode(node[i], "["+i.ToString(CultureInfo.InvariantCulture)+"] = ", level+1);
      break;

    case SLJsonNodeType.Object:
      Console.WriteLine("(Object)");
      foreach(string name in node.Names)
        PrintNode(node[name], name+" = ", level+1);
      break;

    case SLJsonNodeType.Boolean:
    case SLJsonNodeType.Number:
    case SLJsonNodeType.String:
      Console.WriteLine(node.AsString+" ("+node.NodeType.ToString()+")");
      break;

    default:
      Console.WriteLine("("+node.NodeType.ToString()+")");
      break;
  }
}
```

The following properties can be used to read and write values: AsBoolean, AsInt32, AsInt64, AsDouble and AsString. If on reading a property, the value cannot be converted to the corresponding data type, the default value of the data type is returned instead.

If accessing a missing object or if using an invalid array index, no exception is thrown. Instead an empty node is returned. If a value setter is used for a not existing value, the value is created. SLJsonNode can also be used to create new JSON expressions.

```cs
public static void RunExample3()
{
  var root=new SLJsonNode();
  root["addressBook"]=CreateAddressBook();
  Console.WriteLine(root.AsJson);
}

static SLJsonNode CreateAddressBook()
{
  var book=new SLJsonNode();

  book[0]["LastName"]="Average";
  book[0]["firstName"]="Joe";

  book[1]["LastName"]="Doe";
  book[1]["firstName"]="Jane";

  book[2]["LastName"]="Smith";
  book[2]["firstName"]="John";

  return book;
}
```


SLJsonMonitor
-------------

The function SLJsonNode.CreateMonitor can be used to create an instance of class SLJsonMonitor. It's only allowed for root nodes and it must not be called several times.

Property SLJsonMonitor.IsModified is set to true on any change.

Property SLJsonMonitor.IsReadOnly can be used to disallow changing any node.

If passing root nodes to external code, CreateMonitor should always be used before. Otherwise the external code could cause unexpected side effects.


SLJsonWriter
------------

The writer class SLJsonWriter has a very small footprint. It has a good performance, but no checks for incorrect use.


SLJsonSerializer and SLJsonDeserializer
---------------------------------------

The classes SLJsonSerializer and SLJsonDeserializer are based on reflection. Fields and properties to be processed by serialization and deserialization must be marked with the attribute SLJsonMemberAttribute. Only public fields and properties should be marked with this attribute. For deserialization a public standard constructor is required.

```cs
class Example
{
  [SLJsonMember("IntegerArray", SLJsonMemberType.ValueArray)]
  public int[] IntegerArray;

  [SLJsonMember("StringValue")]
  public string StringValue;

  public string NotSerializedString;
}
```

In the following example an instance of a serializable class is created, serialized and deserialized again.

```cs
var e1=new Example();
e1.IntegerArray=new int[] { 10, 20, 30, 700, 800 };
e1.StringValue="Example Text";
e1.NotSerializedString="Other Text";

string json=new SLJsonSerializer().Serialize(e1);
Example e2=new SLJsonDeserializer().Deserialize<Example>(json);
```

The reflection-based serialization and deserialization are unavailable for the .NET Micro Framework.


License
-------

Consider the license terms for the use of this software in whole or in part. The license terms are in file
[Liersch.Json_License.txt](https://github.com/steffen-liersch/Liersch.Json/blob/master/Liersch.Json/Liersch.Json_License.txt).


Copyright
---------

Copyright © 2013-2020 Dipl.-Ing. (BA) Steffen Liersch  
http://www.steffen-liersch.de/

The software is maintained and published here:  
https://github.com/steffen-liersch/Liersch.Json
