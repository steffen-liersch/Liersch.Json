/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;

namespace Liersch.Json
{
  partial class JsonNode
  {
    public bool IsReadOnly { get { return RetrieveMonitor().IsReadOnly; } }


    public JsonMonitor CreateMonitor()
    {
      if(m_Parent!=null)
        throw new InvalidOperationException("CreateMonitor is only allowed for the root node");

      if(m_Monitor!=null)
        throw new InvalidOperationException("CreateMonitor must not be called several times");

      m_Monitor=new JsonMonitor();
      return m_Monitor;
    }


    JsonMonitor RetrieveMonitor()
    {
      JsonNode n=this;
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
      JsonMonitor m=RetrieveMonitor();
      if(m!=null)
      {
        if(m.IsReadOnly)
          throw new InvalidOperationException("Marked as read-only");

        m.IsModified=true;
      }
    }


    static readonly JsonMonitor m_DefaultMonitor=new JsonMonitor();
    JsonMonitor m_Monitor; // Must not be cloned!
  }
}