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

  public sealed class SLJsonWriter
  {
    public int Level { get { return m_Level; } set { m_Level=value; } }

    public SLJsonWriter() : this(new StringBuilder(), true) { }
    public SLJsonWriter(StringBuilder target) : this(target, true) { }
    public SLJsonWriter(StringBuilder target, bool indented) { m_Target=target ?? new StringBuilder(); m_Indented=indented; }
    public override string ToString() { return m_Target.ToString(); }

    public void BeginObject() { BeginRegion('{'); }
    public void EndObject() { EndRegion('}'); }
    public void BeginArray() { BeginRegion('['); }
    public void EndArray() { EndRegion(']'); }
    void BeginRegion(char token) { CheckNL(); CheckVS(); m_Target.Append(token); m_NeedFS=false; m_NeedVS=false; m_NeedLB=true; m_Level++; }
    void EndRegion(char token) { m_Level--; WriteLineBreak(); m_Target.Append(token); m_NeedFS=true; m_NeedVS=true; m_NeedLB=false; }

    public void SetFieldNull(string name) { BeginField(name); WriteValueNull(); }
    public void SetField(string name, bool value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, int value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, long value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, double value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, DateTime value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, TimeSpan value) { BeginField(name); WriteValue(value); }
    public void SetField(string name, string value) { BeginField(name); WriteValue(value); }

    public void BeginField(string name) { CheckNL(); CheckFS(); WriteQuoted(name); m_Target.Append(':'); WriteSpace(); m_NeedVS=false; }
    public void WriteValueNull() { CheckVS(); m_Target.Append("null"); }
    public void WriteValue(bool value) { CheckVS(); m_Target.Append(value ? "true" : "false"); }
    public void WriteValue(int value) { CheckVS(); m_Target.Append(value); }
    public void WriteValue(long value) { CheckVS(); m_Target.Append(value); }
    public void WriteValue(double value) { CheckVS(); m_Target.Append(SLJsonConvert.ToString(value)); }
    public void WriteValue(DateTime value) { CheckVS(); WriteDateTime(value); }
    public void WriteValue(TimeSpan value) { CheckVS(); m_Target.Append('"').Append(value.ToString()).Append('"'); }
    public void WriteValue(string value) { CheckVS(); WriteQuoted(value); }

    public void WriteLineBreak() { if(m_Indented) { m_Target.Append("\r\n"); WriteIndentation(); } m_NeedLB=false; }
    void WriteIndentation() { if(m_Indented) { m_Target.Append('\t', m_Level); } }
    void WriteSpace() { if(m_Indented) { m_Target.Append(' '); } }

    void CheckFS() { if(m_NeedFS) { m_Target.Append(','); WriteLineBreak(); } m_NeedFS=true; }
    void CheckVS() { CheckNL(); if(m_NeedVS) { m_Target.Append(','); WriteLineBreak(); } m_NeedVS=true; }
    void CheckNL() { if(m_NeedLB) { WriteLineBreak(); } }

    void WriteDateTime(DateTime value)
    {
      m_Target.Append('"');
      m_Target.Append(SLJsonConvert.ToString(value.Year, "0000")).Append('-');
      m_Target.Append(SLJsonConvert.ToString(value.Month, "00")).Append('-');
      m_Target.Append(SLJsonConvert.ToString(value.Day, "00")).Append(' ');
      m_Target.Append(SLJsonConvert.ToString(value.Hour, "00")).Append(':');
      m_Target.Append(SLJsonConvert.ToString(value.Minute, "00")).Append(':');
      m_Target.Append(SLJsonConvert.ToString(value.Second, "00"));
      m_Target.Append('"');

      /*
      sb.Append('"');
      sb.Append(value.ToString(@"yyyy\-MM\-dd HH\:mm\:ss", CultureInfo.InvariantCulture));
      sb.Append('"');
      */
    }

    void WriteQuoted(string value)
    {
      if(value==null)
      {
        m_Target.Append("null"); // See also WriteValueNull
        return;
      }

      m_Target.Append('"');
      int len=value.Length;
      for(int i=0; i<len; i++)
      {
        char c=value[i];
        switch(c)
        {
          case '\b': m_Target.Append("\\b"); break; // Backspace      : \u0008
          case '\t': m_Target.Append("\\t"); break; // Horizontal tab : \u0009
          case '\n': m_Target.Append("\\n"); break; // Line feed      : \u000A
          //case '\v': m_Target.Append("\\v"); break; // Vertical tab   : \u000B
          case '\f': m_Target.Append("\\f"); break; // Form feed      : \u000C
          case '\r': m_Target.Append("\\r"); break; // Carriage return: \u000D
          case '"': m_Target.Append("\\\""); break;
          case '\\': m_Target.Append("\\\\"); break;
          default:
            if(!IsControl(c))
              m_Target.Append(c);
            else
            {
              m_Target.Append("\\u");
              m_Target.Append(SLJsonConvert.ToString((int)c, "X4"));
            }
            break;
        }
      }
      m_Target.Append('"');
    }

    static bool IsControl(char c) { return c>='\0' && c<' '; }

    readonly bool m_Indented;
    StringBuilder m_Target;
    int m_Level;
    bool m_NeedFS; // Field separator required
    bool m_NeedVS; // Value separator required
    bool m_NeedLB; // Line-break required
  }

  //--------------------------------------------------------------------------
}