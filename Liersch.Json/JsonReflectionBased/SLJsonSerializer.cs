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
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Liersch.Json
{
  public sealed class SLJsonSerializer
  {
    public bool ThrowOnUnknownValueType { get { return m_ThrowOnUnknownValueType; } set { m_ThrowOnUnknownValueType=value; } }


    public void Serialize(object instance, SLJsonWriter writer) { SerializeObject(writer, instance); }

    public string Serialize(object instance)
    {
      var wr=new SLJsonWriter();
      SerializeObject(wr, instance);
      return wr.ToString();
    }


    [Obsolete("Use Serialize instead")]
    public SLJsonWriter SerializeObject(object instance)
    {
      var wr=new SLJsonWriter();
      SerializeObject(wr, instance);
      return wr;
    }


    void SerializeObject(SLJsonWriter writer, object instance)
    {
      if(instance==null)
        writer.WriteValueNull();
      else
      {
        writer.BeginObject();

        Type t=instance.GetType();

        foreach(FieldInfo fi in t.GetRuntimeFields())
          if(fi.IsPublic)
            foreach(SLJsonMemberAttribute attr in fi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
              SerializeProperty(writer, attr, fi.FieldType, fi.GetValue(instance));

        foreach(PropertyInfo pi in t.GetRuntimeProperties())
        {
          if(pi.CanRead && pi.CanWrite)
          {
            MethodInfo mi1=pi.GetGetMethod();
            if(mi1!=null && mi1.IsPublic)
            {
              MethodInfo mi2=pi.GetSetMethod();
              if(mi2!=null && mi2.IsPublic)
              {
                foreach(SLJsonMemberAttribute attr in pi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
                  SerializeProperty(writer, attr, pi.PropertyType, pi.GetValue(instance, null));
              }
            }
          }
        }

        writer.EndObject();
      }
    }

    void SerializeProperty(SLJsonWriter writer, SLJsonMemberAttribute attribute, Type type, object value)
    {
      writer.BeginField(attribute.MemberName);
      switch(attribute.MemberType)
      {
        case SLJsonMemberType.Value: SerializeValue(writer, value); break;
        case SLJsonMemberType.Object: SerializeObject(writer, value); break;
        case SLJsonMemberType.ValueArray: SerializeArray(writer, type, value, false); break;
        case SLJsonMemberType.ObjectArray: SerializeArray(writer, type, value, true); break;
        default: throw new NotImplementedException();
      }
    }

    void SerializeArray(SLJsonWriter writer, Type type, object array, bool asObject)
    {
      if(type.IsArray && type.GetArrayRank()!=1)
        throw new NotSupportedException("Multi-dimensional arrays are not supported");

      if(!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        throw new NotSupportedException("Type "+type.FullName+" does not implement IEnumerable");

      if(array==null)
      {
        writer.WriteValue(null);
        return;
      }

      writer.BeginArray();

      var helper=(IEnumerable)array;
      foreach(object value in helper)
      {
        if(asObject)
          SerializeObject(writer, value);
        else SerializeValue(writer, value);
      }

      writer.EndArray();
    }

    void SerializeValue(SLJsonWriter writer, object value)
    {
      if(value==null) writer.WriteValueNull();
      else if(value is bool) writer.WriteValue((bool)value);
      else if(value is sbyte) writer.WriteValue((sbyte)value);
      else if(value is byte) writer.WriteValue((byte)value);
      else if(value is short) writer.WriteValue((short)value);
      else if(value is ushort) writer.WriteValue((ushort)value);
      else if(value is int) writer.WriteValue((int)value);
      else if(value is uint) writer.WriteValue((uint)value);
      else if(value is long) writer.WriteValue((long)value);
      else if(value is ulong) writer.WriteValue((ulong)value);
      else if(value is float) writer.WriteValue((float)value);
      else if(value is double) writer.WriteValue((double)value);
      else if(value is DateTime) writer.WriteValue((DateTime)value);
      else if(value is TimeSpan) writer.WriteValue((TimeSpan)value);
      else
      {
        string s=value as string;
        if(s==null)
        {
          if(m_ThrowOnUnknownValueType)
            throw new NotSupportedException("Unknown data type "+value.GetType().FullName+" is not supported");

          s=Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        writer.WriteValue(s);
      }
    }


    bool m_ThrowOnUnknownValueType;
  }
}