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
  public sealed class JsonWriter
  {
    public int Level { get; set; }

    public JsonWriter() : this(new StringBuilder(), true) { }
    public JsonWriter(bool indented) : this(new StringBuilder(), indented) { }
    public JsonWriter(StringBuilder target) : this(target, true) { }
    public JsonWriter(StringBuilder target, bool indented) { m_Target=target ?? new StringBuilder(); m_Indented=indented; }
    public override string ToString() { return m_Target.ToString(); }

    public void BeginObject() { BeginRegion('{'); }
    public void EndObject() { EndRegion('}'); }
    public void BeginArray() { BeginRegion('['); }
    public void EndArray() { EndRegion(']'); }

    void BeginRegion(char token)
    {
      CheckNL();
      CheckVS();
      m_Target.Append(token);
      m_NeedFS=false;
      m_NeedVS=false;
      m_NeedLB=true;
      Level++;
    }

    void EndRegion(char token)
    {
      Level--;
      WriteLineBreak();
      m_Target.Append(token);
      m_NeedFS=true;
      m_NeedVS=true;
      m_NeedLB=false;
    }

    public void SetFieldNull(string name) { BeginField(name); WriteValueNull(); }
    public void SetField(string name, bool value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, int value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, long value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, double value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, DateTime value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, TimeSpan value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, string value) { BeginField(name); WriteValue(value); }

    public void BeginField(string name) { BeginField(name, true); }

    public void BeginField(string name, bool isEscapingRequired)
    {
      CheckNL();
      CheckFS();

      if(isEscapingRequired)
        WriteQuoted(name);
      else
      {
        m_Target.Append('"');
        m_Target.Append(name);
        m_Target.Append('"');
      }

      m_Target.Append(':');
      WriteSpace();
      m_NeedVS=false;
    }

    public void WriteValueNull() { CheckVS(); m_Target.Append("null"); }
    public void WriteValue(bool value) { CheckVS(); m_Target.Append(value ? "true" : "false"); }

    public void WriteValue(int value)
    {
      CheckVS();

      if(value<0)
      {
        m_Target.Append('-');
        value=-value;
      }

      if(value>=0 && value<=9)
        m_Target.Append((char)('0'+value));
      else m_Target.Append(value);
    }

    public void WriteValue(long value) { CheckVS(); m_Target.Append(value); }
    public void WriteValue(double value) { CheckVS(); m_Target.Append(JsonConvert.ToString(value)); }
    public void WriteValue(DateTime value) { CheckVS(); WriteDateTime(value); }
    public void WriteValue(TimeSpan value) { CheckVS(); m_Target.Append('"').Append(value.ToString()).Append('"'); }
    public void WriteValue(string value) { CheckVS(); WriteQuoted(value); }

    public void WriteLineBreak()
    {
      if(m_Indented)
      {
        m_Target.Append("\r\n");
        WriteIndentation();
      }
      m_NeedLB=false;
    }

    void WriteIndentation()
    {
      if(m_Indented)
        m_Target.Append('\t', Level);
    }

    void WriteSpace()
    {
      if(m_Indented)
        m_Target.Append(' ');
    }

    void CheckFS()
    {
      if(!m_NeedFS)
        m_NeedFS=true;
      else
      {
        m_Target.Append(',');
        WriteLineBreak();
      }
    }

    void CheckVS()
    {
      CheckNL();
      if(!m_NeedVS)
        m_NeedVS=true;
      else
      {
        m_Target.Append(',');
        WriteLineBreak();
      }
    }

    void CheckNL()
    {
      if(m_NeedLB)
        WriteLineBreak();
    }

    void WriteDateTime(DateTime value)
    {
      m_Target.Append('"');
      m_Target.Append(JsonConvert.ToString(value.Year, "0000")).Append('-');
      m_Target.Append(JsonConvert.ToString(value.Month, "00")).Append('-');
      m_Target.Append(JsonConvert.ToString(value.Day, "00")).Append(' ');
      m_Target.Append(JsonConvert.ToString(value.Hour, "00")).Append(':');
      m_Target.Append(JsonConvert.ToString(value.Minute, "00")).Append(':');
      m_Target.Append(JsonConvert.ToString(value.Second, "00"));
      m_Target.Append('"');
    }

    void WriteQuoted(string value)
    {
      if(value==null)
      {
        m_Target.Append("null"); // See also WriteValueNull
        return;
      }

      int len=value.Length;
      if(len<=0)
      {
        m_Target.Append("\"\"");
        return;
      }

      int start=0;
      int c;

      for(int i = 0; i<len; i++)
      {
        int v=value[i];
        if(v<m_EscapeMap.Length)
        {
          string alt=m_EscapeMap[v];
          if(alt!=null)
          {
            if(start<=0)
              m_Target.Append('"');
            c=i-start;
            if(c>0)
              m_Target.Append(value, start, c);
            m_Target.Append(alt);
            start=i+1;
          }
        }
      }

      if(start>0)
      {
        c=value.Length-start;
        if(c>0)
          m_Target.Append(value, start, c);
        m_Target.Append('"');
      }
      else
      {
        m_Target.Append('"');
        m_Target.Append(value);
        m_Target.Append('"');
      }
    }


    public static bool IsEscapingRequired(string value)
    {
      if(value!=null)
      {
        int c=value.Length;
        for(int i = 0; i<c; i++)
        {
          int v=value[i];
          if(v<m_EscapeMap.Length)
          {
            string alt=m_EscapeMap[v];
            if(alt!=null)
              return true;
          }
        }
      }
      return false;
    }

    static string[] CreateEscapeMap()
    {
      string[] res=new string['\\'+1];

      for(char c = '\0'; c<' '; c++)
        res[c]="\\u"+JsonConvert.ToString(c, "X4");

      res['\b']="\\b"; // Backspace      : \u0008
      res['\t']="\\t"; // Horizontal tab : \u0009
      res['\n']="\\n"; // Line feed      : \u000A
      res['\f']="\\f"; // Form feed      : \u000C
      res['\r']="\\r"; // Carriage return: \u000D
      res['"']="\\\"";
      res['/']="\\/";
      res['\\']="\\\\";

      return res;
    }

    static readonly string[] m_EscapeMap=CreateEscapeMap();


    readonly bool m_Indented;
    readonly StringBuilder m_Target;
    bool m_NeedFS; // Field separator required
    bool m_NeedVS; // Value separator required
    bool m_NeedLB; // Line-break required
  }
}