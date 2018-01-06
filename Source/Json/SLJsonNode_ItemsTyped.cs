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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  partial class SLJsonNode : IEnumerable<SLJsonNode>
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

    public IEnumerator<SLJsonNode> GetEnumerator()
    {
      if(m_Array!=null)
        return m_Array.GetEnumerator();

      if(m_Object!=null)
        return m_Object.Values.GetEnumerator();

      return m_EmptyNodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    //------------------------------------------------------------------------

    SLJsonNode GetArrayItem(int index) { return m_Array[index]; }

    SLJsonNode TryGetObjectProperty(string name)
    {
      SLJsonNode res;
      m_Object.TryGetValue(name, out res);
      return res;
    }

    //------------------------------------------------------------------------

    static List<SLJsonNode> CreateArray() { return new List<SLJsonNode>(); }

    static SortedDictionary<string, SLJsonNode> CreateObject() { return new SortedDictionary<string, SLJsonNode>(); }

    //------------------------------------------------------------------------

    static readonly IList<string> m_EmptyNames=new ReadOnlyCollection<string>(new string[0]);
    static readonly IList<SLJsonNode> m_EmptyNodes=new ReadOnlyCollection<SLJsonNode>(new SLJsonNode[0]);

    internal List<SLJsonNode> m_Array;
    internal SortedDictionary<string, SLJsonNode> m_Object;
  }

  //--------------------------------------------------------------------------
}