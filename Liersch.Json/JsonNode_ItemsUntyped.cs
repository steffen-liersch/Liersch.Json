/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

#if NETMF

using System.Collections;

namespace Liersch.Json
{
  partial class JsonNode : IEnumerable
  {
    public IEnumerable Names
    {
      get
      {
        if(m_Object!=null)
          return m_Object.Keys;

        return m_UntypedEmptyList;
      }
    }

    public IEnumerator GetEnumerator()
    {
      if(m_Array!=null)
        return m_Array.GetEnumerator();

      if(m_Object!=null)
        return m_Object.Values.GetEnumerator();

      return m_UntypedEmptyList.GetEnumerator();
    }


    JsonNode GetArrayItem(int index) { return (JsonNode)m_Array[index]; }

    JsonNode TryGetObjectProperty(string name) { return (JsonNode)m_Object[name]; }


    static ArrayList CreateArray() { return new ArrayList(); }

    static IDictionary CreateObject()
    {
#if NETMF
      return new Hashtable();
#else
      return new SortedList();
#endif
    }


    static readonly IList m_UntypedEmptyList=new ArrayList();

    internal ArrayList m_Array; // List<JsonNode>
    internal IDictionary m_Object; // Dictionary<string, JsonNode>
  }
}

#endif