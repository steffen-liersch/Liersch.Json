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
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  public sealed class SLJsonSerializer
  {
    public bool ThrowOnUnknownValueType { get { return m_ThrowOnUnknownValueType; } set { m_ThrowOnUnknownValueType=value; } }

    //------------------------------------------------------------------------

    public SLJsonSerializer() { m_Writer=new SLJsonWriter(); }

    public SLJsonSerializer(SLJsonWriter writer) { m_Writer=writer ?? new SLJsonWriter(); }

    public override string ToString() { return m_Writer.ToString(); }

    public SLJsonSerializer SerializeObject(object instance)
    {
      if(instance==null)
        m_Writer.WriteValueNull();
      else
      {
        m_Writer.BeginObject();

        Type t=instance.GetType();

        foreach(FieldInfo fi in t.GetRuntimeFields())
          if(fi.IsPublic)
            foreach(SLJsonMemberAttribute attr in fi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
              SerializeProperty(attr, fi.FieldType, fi.GetValue(instance));

        foreach(PropertyInfo pi in t.GetRuntimeProperties())
          if(pi.CanRead && pi.CanWrite /*&& pi.GetMethod.IsPublic && pi.SetMethod.IsPublic*/)
            foreach(SLJsonMemberAttribute attr in pi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
              SerializeProperty(attr, pi.PropertyType, pi.GetValue(instance, null));

        m_Writer.EndObject();
      }

      return this;
    }

    //------------------------------------------------------------------------

    void SerializeProperty(SLJsonMemberAttribute attribute, Type type, object value)
    {
      m_Writer.BeginField(attribute.MemberName);
      switch(attribute.MemberType)
      {
        case SLJsonMemberType.Value: SerializeValue(value); break;
        case SLJsonMemberType.Object: SerializeObject(value); break;
        case SLJsonMemberType.ValueArray: SerializeArray(type, value, false); break;
        case SLJsonMemberType.ObjectArray: SerializeArray(type, value, true); break;
        default: throw new NotImplementedException();
      }
    }

    void SerializeArray(Type type, object array, bool asObject)
    {
      if(!type.IsArray)
        throw new NotSupportedException("Type "+type.FullName+" is not an array");

      if(array==null)
      {
        m_Writer.WriteValue(null);
        return;
      }

      m_Writer.BeginArray();

      var helper=(IEnumerable)array;
      foreach(object value in helper)
      {
        if(asObject)
          SerializeObject(value);
        else SerializeValue(value);
      }

      m_Writer.EndArray();
    }

    void SerializeValue(object value)
    {
      if(value==null) m_Writer.WriteValueNull();
      else if(value is bool) m_Writer.WriteValue((bool)value);
      else if(value is sbyte) m_Writer.WriteValue((sbyte)value);
      else if(value is byte) m_Writer.WriteValue((byte)value);
      else if(value is short) m_Writer.WriteValue((short)value);
      else if(value is ushort) m_Writer.WriteValue((ushort)value);
      else if(value is int) m_Writer.WriteValue((int)value);
      else if(value is uint) m_Writer.WriteValue((uint)value);
      else if(value is long) m_Writer.WriteValue((long)value);
      else if(value is ulong) m_Writer.WriteValue((ulong)value);
      else if(value is float) m_Writer.WriteValue((float)value);
      else if(value is double) m_Writer.WriteValue((double)value);
      else if(value is DateTime) m_Writer.WriteValue((DateTime)value);
      else if(value is TimeSpan) m_Writer.WriteValue((TimeSpan)value);
      else
      {
        string s=value as string;
        if(s==null)
        {
          if(m_ThrowOnUnknownValueType)
            throw new NotSupportedException("Unknown data type "+value.GetType().FullName+" is not supported");

          s=Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        m_Writer.WriteValue(s);
      }
    }

    //------------------------------------------------------------------------

    SLJsonWriter m_Writer;
    bool m_ThrowOnUnknownValueType;
  }

  //--------------------------------------------------------------------------
}