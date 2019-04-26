//----------------------------------------------------------------------------
//
// Copyright © 2013-2018 Dipl.-Ing. (BA) Steffen Liersch
// All rights reserved.
//
// Steffen Liersch
// Robert-Schumann-Straße 1
// 08289 Schneeberg
// Germany
//
// Phone: +49-3772-38 28 08
// E-Mail: S.Liersch@gmx.de
//
//----------------------------------------------------------------------------

#if NETMF

using System.Collections;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  partial class SLJsonNode : IEnumerable
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

    //------------------------------------------------------------------------

    SLJsonNode GetArrayItem(int index) { return (SLJsonNode)m_Array[index]; }

    SLJsonNode TryGetObjectProperty(string name) { return (SLJsonNode)m_Object[name]; }

    //------------------------------------------------------------------------

    static ArrayList CreateArray() { return new ArrayList(); }

    static IDictionary CreateObject()
    {
#if NETMF
      return new Hashtable();
#else
      return new SortedList();
#endif
    }

    //------------------------------------------------------------------------

    static readonly IList m_UntypedEmptyList=new ArrayList();

    internal ArrayList m_Array; // List<SLJsonNode>
    internal IDictionary m_Object; // Dictionary<string, SLJsonNode>
  }

  //--------------------------------------------------------------------------
}

#endif