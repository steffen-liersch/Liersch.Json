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
  public sealed partial class JsonNode
  {
    public static JsonNode Parse(string jsonExpression) { return JsonParser.Parse(jsonExpression, true); }


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

    public JsonNode this[int index]
    {
      get
      {
        if(index<0)
          throw new ArgumentOutOfRangeException("index");

        if(m_Array!=null && index<m_Array.Count)
          return GetArrayItem(index);

        return new JsonNode(this, index);
      }
      set
      {
        if(value==null)
          value=new JsonNode();
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

    public JsonNode this[string name]
    {
      get
      {
        if(name==null)
          throw new ArgumentNullException("name");

        if(m_Object!=null)
        {
          JsonNode n=TryGetObjectProperty(name);
          if(n!=null)
            return n;
        }

        return new JsonNode(this, name);
      }
      set
      {
        if(name==null)
          throw new ArgumentNullException("name");

        if(value==null)
          value=new JsonNode();
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


    public JsonNodeType NodeType { get { return m_NodeType; } }
    public bool IsMissing { get { return m_NodeType==JsonNodeType.Missing; } }
    public bool IsNull { get { return m_NodeType==JsonNodeType.Null; } }
    public bool IsArray { get { return m_NodeType==JsonNodeType.Array; } }
    public bool IsObject { get { return m_NodeType==JsonNodeType.Object; } }
    public bool IsValue { get { return m_NodeType>=JsonNodeType.Boolean; } }
    public bool IsBoolean { get { return m_NodeType==JsonNodeType.Boolean; } }
    public bool IsNumber { get { return m_NodeType==JsonNodeType.Number; } }
    public bool IsString { get { return m_NodeType==JsonNodeType.String; } }


    public bool AsBoolean { get { return GetValue(false); } set { ChangeValue(JsonNodeType.Boolean, value ? "true" : "false"); } }
    public int AsInt32 { get { return GetValue(0); } set { ChangeValue(JsonNodeType.Number, JsonConvert.ToString(value)); } }
    public long AsInt64 { get { return GetValue(0L); } set { ChangeValue(JsonNodeType.Number, JsonConvert.ToString(value)); } }
    public double AsDouble { get { return GetValue(0.0); } set { ChangeValue(JsonNodeType.Number, JsonConvert.ToString(value)); } }
    public string AsString { get { return m_Value; } set { ChangeValue(value!=null ? JsonNodeType.String : JsonNodeType.Null, value); } }


    public bool GetValue(bool defaultValue) { bool res; return TryGetValue(out res) ? res : defaultValue; }
    public int GetValue(int defaultValue) { int res; return TryGetValue(out res) ? res : defaultValue; }
    public long GetValue(long defaultValue) { long res; return TryGetValue(out res) ? res : defaultValue; }
    public double GetValue(double defaultValue) { double res; return TryGetValue(out res) ? res : defaultValue; }


    public bool TryGetValue(out bool value)
    {
      double v;
      bool res=TryGetValue(out v);
      value=res && Math.Abs(v)>=1-1e-7;
      return res;
    }

    public bool TryGetValue(out int value)
    {
      double v;
      if(TryGetValue(out v))
      {
        v=Math.Round(v);
        int i=unchecked((int)v);
        bool res=v-i==0;
        value=res ? i : 0;
        return res;
      }
      else
      {
        value=0;
        return false;
      }
    }

    public bool TryGetValue(out long value)
    {
      double v;
      if(TryGetValue(out v))
      {
        v=Math.Round(v);
        long i=unchecked((long)v);
        bool res=v-i==0;
        value=res ? i : 0;
        return res;
      }
      else
      {
        value=0;
        return false;
      }
    }

    public bool TryGetValue(out double value)
    {
      switch(m_Value)
      {
        case "false": value=0; return true;
        case "true": value=1; return true;
        default: return JsonConvert.TryParse(m_Value, out value);
      }
    }


    public void CreateEmptyArray()
    {
      ChangeValue(JsonNodeType.Null, null);
      MakeArray();
    }

    public void CreateEmptyObject()
    {
      ChangeValue(JsonNodeType.Null, null);
      MakeObject();
    }

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

    public JsonNode Clone() { return Clone(null); }

    JsonNode Clone(JsonNode parent)
    {
      var res=new JsonNode(parent);

      res.m_NodeType=m_NodeType;
      res.m_Value=m_Value;

      res.m_Index=m_Index;
      res.m_Name=m_Name;

      // Do not copy m_Monitor here!

      if(m_Array!=null)
      {
        res.m_Array=CreateArray();
        foreach(JsonNode n in m_Array)
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
          JsonNode n=TryGetObjectProperty(k);

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
        return "Array: ["+JsonConvert.ToString(m_Array.Count)+"]";

      if(m_Object!=null)
      {
        var sb=new StringBuilder();
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
        case JsonNodeType.Missing: return "Missing";
        case JsonNodeType.Null: return "Null";
        default: return "???";
      }
    }


    internal void MakeArray()
    {
      if(m_NodeType!=JsonNodeType.Array)
      {
        m_NodeType=JsonNodeType.Array;
        m_Value=null;

        m_Array=CreateArray();
        ReleaseObjects();
      }
    }

    internal void MakeObject()
    {
      if(m_NodeType!=JsonNodeType.Object)
      {
        m_NodeType=JsonNodeType.Object;
        m_Value=null;

        ReleaseArray();
        m_Object=CreateObject();
      }
    }

    void MakeArraySetItem(int index, JsonNode value)
    {
      MakeArray();

      while(m_Array.Count<index)
        m_Array.Add(new JsonNode(this));

      if(index==m_Array.Count)
        m_Array.Add(value);
      else
      {
        GetArrayItem(index).ClearParent();
        m_Array[index]=value;
      }
    }

    void MakeObjectSetItem(string name, JsonNode value)
    {
      MakeObject();

      JsonNode n=TryGetObjectProperty(name);
      if(n!=null)
        n.ClearParent();

      m_Object[name]=value;
    }

    void ReleaseArray()
    {
      if(m_Array!=null)
      {
        foreach(JsonNode n in m_Array)
          n.ClearParent();
        m_Array=null;
      }
    }

    void ReleaseObjects()
    {
      if(m_Object!=null)
      {
        foreach(JsonNode n in m_Object.Values)
          n.ClearParent();
        m_Object=null;
      }
    }

    void ChangeValue(JsonNodeType nodeType, string value)
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
      JsonNode n=this;
      JsonNode p=n.m_Parent;
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


    internal void AssignParent(JsonNode parent) { m_Parent=parent; } // Used by the parser and by the setters of the indexer properties
    void ClearParent() { m_Parent=null; }


    public JsonNode() { m_NodeType=JsonNodeType.Null; } // Used to create a new JSON expression; Also used by the parser
    internal JsonNode(JsonNodeType nodeType, string value) { m_NodeType=nodeType; m_Value=value; } // Used by the parser
    JsonNode(JsonNode parent, int index) { m_NodeType=JsonNodeType.Missing; m_Parent=parent; m_Index=index; }
    JsonNode(JsonNode parent, string name) { m_NodeType=JsonNodeType.Missing; m_Parent=parent; m_Name=name; }
    JsonNode(JsonNode parent) { m_NodeType=JsonNodeType.Null; m_Parent=parent; }


    JsonNodeType m_NodeType;
    string m_Value;

    JsonNode m_Parent;
    int m_Index=-1;
    string m_Name;
  }
}