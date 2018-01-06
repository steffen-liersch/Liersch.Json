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

using System;
using System.Text;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  public sealed partial class SLJsonNode
  {
    public int Count
    {
      get
      {
        if(m_Array!=null)
          return m_Array.Count;

        if(m_Object!=null)
          return m_Object.Count;

        return 0;
      }
    }

    public SLJsonNode this[int index]
    {
      get
      {
        if(index<0)
          throw new ArgumentOutOfRangeException("index");

        if(m_Array!=null && index<m_Array.Count)
          return GetArrayItem(index);

        return new SLJsonNode(this, index);
      }
      set
      {
        if(value==null)
          value=new SLJsonNode();
        else
        {
          if(value.m_Parent!=null)
            throw new ArgumentException("Node already in use", "value");
        }

        BeforeChange();

        MakeArraySetItem(index, value);
        value.AssignParent(this);

        Activate();
      }
    }

    public SLJsonNode this[string name]
    {
      get
      {
        if(name==null)
          throw new ArgumentNullException("name");

        if(m_Object!=null)
        {
          SLJsonNode n=TryGetObjectProperty(name);
          if(n!=null)
            return n;
        }

        return new SLJsonNode(this, name);
      }
      set
      {
        if(name==null)
          throw new ArgumentNullException("name");

        if(value==null)
          value=new SLJsonNode();
        else
        {
          if(value.m_Parent!=null)
            throw new ArgumentException("Node already in use", "value");
        }

        BeforeChange();

        MakeObjectSetItem(name, value);
        value.AssignParent(this);

        Activate();
      }
    }

    //------------------------------------------------------------------------

    public SLJsonNodeType NodeType { get { return m_NodeType; } }
    public bool IsMissing { get { return m_NodeType==SLJsonNodeType.Missing; } }
    public bool IsNull { get { return m_NodeType==SLJsonNodeType.Null; } }
    public bool IsArray { get { return m_NodeType==SLJsonNodeType.Array; } }
    public bool IsObject { get { return m_NodeType==SLJsonNodeType.Object; } }
    public bool IsValue { get { return m_NodeType>=SLJsonNodeType.Boolean; } }
    public bool IsBoolean { get { return m_NodeType==SLJsonNodeType.Boolean; } }
    public bool IsNumber { get { return m_NodeType==SLJsonNodeType.Number; } }
    public bool IsString { get { return m_NodeType==SLJsonNodeType.String; } }

    //------------------------------------------------------------------------

    public bool AsBoolean { get { return GetValue(false); } set { ChangeValue(SLJsonNodeType.Boolean, value ? "true" : "false"); } }
    public int AsInt32 { get { return GetValue(0); } set { ChangeValue(SLJsonNodeType.Number, SLJsonConvert.ToString(value)); } }
    public long AsInt64 { get { return GetValue(0L); } set { ChangeValue(SLJsonNodeType.Number, SLJsonConvert.ToString(value)); } }
    public double AsDouble { get { return GetValue(0.0); } set { ChangeValue(SLJsonNodeType.Number, SLJsonConvert.ToString(value)); } }
    public string AsString { get { return m_Value; } set { ChangeValue(value!=null ? SLJsonNodeType.String : SLJsonNodeType.Null, value); } }

    //------------------------------------------------------------------------

    public bool GetValue(bool defaultValue) { bool res; return TryGetValue(out res) ? res : defaultValue; }
    public int GetValue(int defaultValue) { int res; return TryGetValue(out res) ? res : defaultValue; }
    public long GetValue(long defaultValue) { long res; return TryGetValue(out res) ? res : defaultValue; }
    public double GetValue(double defaultValue) { double res; return TryGetValue(out res) ? res : defaultValue; }

    //------------------------------------------------------------------------

    public bool TryGetValue(out bool value)
    {
      switch(m_Value)
      {
        case "false": value=false; return true;
        case "true": value=true; return true;
      }

      double v;
      bool res=TryGetValue(out v);
      value=res && Math.Abs(AsDouble)>=1-1e-7;
      return res;
    }

    public bool TryGetValue(out int value)
    {
      double v;
      bool res=TryGetValue(out v);
      if(!res)
        value=0;
      else
      {
        res=v>=int.MinValue && v<=int.MaxValue;
        value=res ? (int)v : 0;
      }
      return res;
    }

    public bool TryGetValue(out long value)
    {
      double v;
      bool res=TryGetValue(out v);
      if(!res)
        value=0;
      else
      {
        res=v>=long.MinValue && v<=long.MaxValue;
        value=res ? (long)v : 0;
      }
      return res;
    }

    public bool TryGetValue(out double value) { return SLJsonConvert.TryParse(m_Value, out value); }

    //------------------------------------------------------------------------

    public bool Remove()
    {
      BeforeChange();

      if(m_Parent==null)
        return false;

      if(m_Index>=0 || m_Name!=null)
      {
        ClearParent();
        m_Index=-1;
        m_Name=null;
        return false; // Return false here!
      }

      if(m_Parent.m_Array!=null)
      {
        int c=m_Parent.m_Array.Count;
        m_Parent.m_Array.Remove(this);
        bool res=m_Parent.m_Array.Count<c;
        ClearParent();
        return res;
      }

      if(m_Parent.m_Object!=null)
      {
        foreach(string k in m_Parent.m_Object.Keys)
        {
          if(m_Parent.m_Object[k]==this)
          {
            m_Parent.m_Object.Remove(k);
            ClearParent();
            return true;
          }
        }
      }

      ClearParent();
      return false;
    }

    public SLJsonNode Clone() { return Clone(null); }

    SLJsonNode Clone(SLJsonNode parent)
    {
      SLJsonNode res=new SLJsonNode(parent);

      res.m_NodeType=m_NodeType;
      res.m_Value=m_Value;

      res.m_Index=m_Index;
      res.m_Name=m_Name;

      // Do not copy m_Monitor here!

      if(m_Array!=null)
      {
        res.m_Array=CreateArray();
        foreach(SLJsonNode n in m_Array)
        {

#if DEBUG
          if(n.m_Parent!=this)
            throw new InvalidOperationException();
#endif

          res.m_Array.Add(n.Clone(res));
        }
      }

      if(m_Object!=null)
      {
        res.m_Object=CreateObject();
        foreach(string k in m_Object.Keys)
        {
          SLJsonNode n=TryGetObjectProperty(k);

#if DEBUG
          if(n.m_Parent!=this)
            throw new InvalidOperationException();
#endif

          res.m_Object.Add(k, n.Clone(res));
        }
      }

      return res;
    }

    public override string ToString()
    {
      if(m_Array!=null)
        return "Array: ["+SLJsonConvert.ToString(m_Array.Count)+"]";

      if(m_Object!=null)
      {
        StringBuilder sb=new StringBuilder();
        sb.Append("Object: {");
        bool needSep=false;
        foreach(string n in m_Object.Keys)
        {
          if(needSep)
            sb.Append(", ");
          else needSep=true;
          sb.Append(n);
        }
        sb.Append('}');
        return sb.ToString();
      }

      if(IsValue)
        return m_Value!=null ? "Value: "+m_Value : "Value: (null)";

      switch(m_NodeType)
      {
        case SLJsonNodeType.Missing: return "Missing";
        case SLJsonNodeType.Null: return "Null";
        default: return "???";
      }
    }

    //------------------------------------------------------------------------

    internal void MakeArray()
    {
      if(m_NodeType!=SLJsonNodeType.Array)
      {
        m_NodeType=SLJsonNodeType.Array;
        m_Value=null;

        m_Array=CreateArray();
        ReleaseObjects();
      }
    }

    internal void MakeObject()
    {
      if(m_NodeType!=SLJsonNodeType.Object)
      {
        m_NodeType=SLJsonNodeType.Object;
        m_Value=null;

        ReleaseArray();
        m_Object=CreateObject();
      }
    }

    void MakeArraySetItem(int index, SLJsonNode value)
    {
      MakeArray();

      while(m_Array.Count<index)
        m_Array.Add(new SLJsonNode(this));

      if(index==m_Array.Count)
        m_Array.Add(value);
      else
      {
        GetArrayItem(index).ClearParent();
        m_Array[index]=value;
      }
    }

    void MakeObjectSetItem(string name, SLJsonNode value)
    {
      MakeObject();

      SLJsonNode n=TryGetObjectProperty(name);
      if(n!=null)
        n.ClearParent();

      m_Object[name]=value;
    }

    void ReleaseArray()
    {
      if(m_Array!=null)
      {
        foreach(SLJsonNode n in m_Array)
          n.ClearParent();
        m_Array=null;
      }
    }

    void ReleaseObjects()
    {
      if(m_Object!=null)
      {
        foreach(SLJsonNode n in m_Object.Values)
          n.ClearParent();
        m_Object=null;
      }
    }

    void ChangeValue(SLJsonNodeType nodeType, string value)
    {
      BeforeChange();

      m_NodeType=nodeType;
      m_Value=value;

      ReleaseArray();
      ReleaseObjects();

      Activate();
    }

    void Activate()
    {
      SLJsonNode n=this;
      SLJsonNode p=n.m_Parent;
      while(p!=null)
      {
        if(n.m_Index>=0)
        {
          p.MakeArraySetItem(n.m_Index, n);
          n.m_Index=-1;
        }
        else if(n.m_Name!=null)
        {
          p.MakeObjectSetItem(n.m_Name, n);
          n.m_Name=null;
        }
        else break;

        n=p;
        p=n.m_Parent;
      }
    }

    //------------------------------------------------------------------------

    internal void AssignParent(SLJsonNode parent) { m_Parent=parent; } // Used by the parser and by the setters of the indexer properties
    void ClearParent() { m_Parent=null; }

    //------------------------------------------------------------------------

    public SLJsonNode() { m_NodeType=SLJsonNodeType.Null; } // Used to create a new JSON expression; Also used by the parser
    internal SLJsonNode(SLJsonNodeType nodeType, string value) { m_NodeType=nodeType; m_Value=value; } // Used by the parser
    SLJsonNode(SLJsonNode parent, int index) { m_NodeType=SLJsonNodeType.Missing; m_Parent=parent; m_Index=index; }
    SLJsonNode(SLJsonNode parent, string name) { m_NodeType=SLJsonNodeType.Missing; m_Parent=parent; m_Name=name; }
    SLJsonNode(SLJsonNode parent) { m_NodeType=SLJsonNodeType.Null; m_Parent=parent; }

    //------------------------------------------------------------------------

    SLJsonNodeType m_NodeType;
    string m_Value;

    SLJsonNode m_Parent;
    int m_Index=-1;
    string m_Name;
  }

  //--------------------------------------------------------------------------
}