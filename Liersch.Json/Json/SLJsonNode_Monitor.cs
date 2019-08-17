//----------------------------------------------------------------------------
//
// Copyright © 2013-2019 Dipl.-Ing. (BA) Steffen Liersch
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

using System;

namespace Liersch.Json
{
  partial class SLJsonNode
  {
    public bool IsReadOnly { get { return RetrieveMonitor().IsReadOnly; } }


    public SLJsonMonitor CreateMonitor()
    {
      if(m_Parent!=null)
        throw new InvalidOperationException("CreateMonitor is only allowed for the root node");

      if(m_Monitor!=null)
        throw new InvalidOperationException("CreateMonitor must not be called several times");

      m_Monitor=new SLJsonMonitor();
      return m_Monitor;
    }


    SLJsonMonitor RetrieveMonitor()
    {
      SLJsonNode n=this;
      do
      {
        if(n.m_Parent==null)
          return n.m_Monitor ?? m_DefaultMonitor;

#if DEBUG
        if(n.m_Monitor!=null)
          throw new InvalidOperationException();
#endif

        n=n.m_Parent;
      }
      while(n!=null);

      return m_DefaultMonitor;
    }

    void BeforeChange()
    {
      SLJsonMonitor m=RetrieveMonitor();
      if(m!=null)
      {
        if(m.IsReadOnly)
          throw new InvalidOperationException("Marked as read-only");

        m.IsModified=true;
      }
    }


    static readonly SLJsonMonitor m_DefaultMonitor=new SLJsonMonitor();
    SLJsonMonitor m_Monitor; // Must not be cloned!
  }
}