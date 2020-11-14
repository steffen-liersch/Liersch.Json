/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

#if !NETMF

using System.Collections;
using System.Collections.Generic;

namespace Liersch.Json
{
  partial class JsonNode : IEnumerable<JsonNode>
  {
    public IEnumerable<string> Names
    {
      get
      {
        if(m_Object!=null)
          return m_Object.Keys;

        return m_EmptyNames;
      }
    }

    public IEnumerator<JsonNode> GetEnumerator()
    {
      if(m_Array!=null)
        return m_Array.GetEnumerator();

      if(m_Object!=null)
        return m_Object.Values.GetEnumerator();

      return m_EmptyNodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }


    JsonNode GetArrayItem(int index) { return m_Array[index]; }

    JsonNode TryGetObjectProperty(string name)
    {
      JsonNode res;
      m_Object.TryGetValue(name, out res);
      return res;
    }


    static List<JsonNode> CreateArray() { return new List<JsonNode>(); }

    static SortedDictionary<string, JsonNode> CreateObject() { return new SortedDictionary<string, JsonNode>(); }


    static readonly IList<string> m_EmptyNames=new string[0];
    static readonly IList<JsonNode> m_EmptyNodes=new JsonNode[0];

    internal List<JsonNode> m_Array;
    internal SortedDictionary<string, JsonNode> m_Object;
  }
}

#endif