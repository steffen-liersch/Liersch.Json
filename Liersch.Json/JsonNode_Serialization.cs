/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Text;

namespace Liersch.Json
{
  partial class JsonNode
  {
    public string AsJson { get { return Serialize(true); } }
    public string AsJsonCompact { get { return Serialize(false); } }


    public string Serialize(bool indented)
    {
      var wr=new JsonWriter(new StringBuilder(), indented);
      Serialize(wr);
      return wr.ToString();
    }

    public void Serialize(JsonWriter writer)
    {
      if(writer==null)
        throw new ArgumentNullException("writer");

      switch(NodeType)
      {
        case JsonNodeType.Missing:
        case JsonNodeType.Null: writer.WriteValueNull(); break;
        case JsonNodeType.Array: SerializeArray(writer); break;
        case JsonNodeType.Object: SerializeObject(writer); break;
        case JsonNodeType.Boolean: writer.WriteValue(AsBoolean); break;
        case JsonNodeType.Number: writer.WriteValue(AsDouble); break;
        case JsonNodeType.String: writer.WriteValue(AsString); break;
        default: throw new NotImplementedException();
      }
    }


    void SerializeArray(JsonWriter writer)
    {
      writer.BeginArray();
      foreach(JsonNode n in this)
        n.Serialize(writer);
      writer.EndArray();
    }

    void SerializeObject(JsonWriter writer)
    {
      writer.BeginObject();

      foreach(string n in Names)
      {
        writer.BeginField(n);
        this[n].Serialize(writer);
      }

      writer.EndObject();
    }
  }
}