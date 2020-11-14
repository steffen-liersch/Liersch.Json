/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

namespace Liersch.Json
{
  partial class JsonNode
  {
    public static implicit operator JsonNode(bool value) { return new JsonNode(JsonNodeType.Boolean, value ? "true" : "false"); }
    public static implicit operator JsonNode(int value) { return new JsonNode(JsonNodeType.Number, JsonConvert.ToString(value)); }
    public static implicit operator JsonNode(long value) { return new JsonNode(JsonNodeType.Number, JsonConvert.ToString(value)); }
    public static implicit operator JsonNode(double value) { return new JsonNode(JsonNodeType.Number, JsonConvert.ToString(value)); }
    public static implicit operator JsonNode(string value) { return new JsonNode(value!=null ? JsonNodeType.String : JsonNodeType.Null, value); }

    public static implicit operator bool(JsonNode node) { return node!=null && node.AsBoolean; }
    public static implicit operator int(JsonNode node) { return node!=null ? node.AsInt32 : 0; }
    public static implicit operator long(JsonNode node) { return node!=null ? node.AsInt64 : 0; }
    public static implicit operator double(JsonNode node) { return node!=null ? node.AsDouble : 0; }
    public static implicit operator string(JsonNode node) { return node!=null ? node.AsString : null; }
  }
}