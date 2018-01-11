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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  public delegate object SLJsonConverter(string value);

  //--------------------------------------------------------------------------

  public sealed class SLJsonDeserializer
  {
    public void RegisterConverter(Type type, SLJsonConverter converter) { m_Converters[type]=converter; }

    public T Deserialize<T>(string jsonExpression)
    {
      SLJsonNode n=SLJsonParser.Parse(jsonExpression);
      return Deserialize<T>(n);
    }

    public T Deserialize<T>(SLJsonNode node)
    {
      if(node==null)
        throw new ArgumentNullException("node");

      if(node.IsObject)
        return (T)DeserializeObject(typeof(T), node);

      return Activator.CreateInstance<T>();
    }

    //------------------------------------------------------------------------

    object DeserializeObject(Type type, SLJsonNode container)
    {
      object res=Activator.CreateInstance(type);

      foreach(FieldInfo fi in type.GetRuntimeFields())
        if(fi.IsPublic)
          foreach(SLJsonMemberAttribute attr in fi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
            fi.SetValue(res, DeserializeObjectInternal(container[attr.MemberName], attr.MemberType, fi.FieldType, fi.GetValue(res)));

      foreach(PropertyInfo pi in type.GetRuntimeProperties())
        if(pi.CanRead && pi.CanWrite /*&& pi.GetMethod.IsPublic && pi.SetMethod.IsPublic*/)
          foreach(SLJsonMemberAttribute attr in pi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
            pi.SetValue(res, DeserializeObjectInternal(container[attr.MemberName], attr.MemberType, pi.PropertyType, pi.GetValue(res, null)), null);

      return res;
    }

    object DeserializeObjectInternal(SLJsonNode node, SLJsonMemberType memberType, Type type, object value)
    {
      if(node!=null)
      {
        switch(memberType)
        {
          case SLJsonMemberType.Value:
            if(node.IsValue)
              return DeserializeValue(type, node);
            break;

          case SLJsonMemberType.ValueArray:
            if(node.IsArray)
              return DeserializeArray(type, node, false);
            break;

          case SLJsonMemberType.Object:
            if(node.IsObject)
              return DeserializeObject(type, node);
            break;

          case SLJsonMemberType.ObjectArray:
            if(node.IsArray)
              return DeserializeArray(type, node, true);
            break;

          default:
            throw new NotImplementedException();
        }
      }

      return value;
    }

    object DeserializeArray(Type type, SLJsonNode array, bool asObject)
    {
      if(!type.IsArray)
        throw new NotSupportedException("Type "+type.FullName+" is not an array");

      Type t=type.GetElementType();

      object defaultValue=null;
      if(!asObject && !t.GetTypeInfo().IsClass)
        defaultValue=Activator.CreateInstance(t);

      int c=array.Count;
      var res=Array.CreateInstance(t, c);

      for(int i=0; i<c; i++)
      {
        object v=defaultValue;
        SLJsonNode n=array[i];
        if(n!=null)
        {
          if(asObject)
          {
            if(n.IsObject)
              v=DeserializeObject(t, n);
          }
          else
          {
            if(n.IsValue)
              v=DeserializeValue(t, n);
          }
        }
        res.SetValue(v, i);
      }

      return res;
    }

    object DeserializeValue(Type type, SLJsonNode value) { return ParseValue(type, value.AsString); }

    //------------------------------------------------------------------------

    object ParseValue(Type type, string value)
    {
      SLJsonConverter parseValue;
      if(m_Converters.TryGetValue(type, out parseValue))
        return parseValue(value);

      if(typeof(Enum).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        return Enum.Parse(type, value);

#if NET20
      Type[] types=type.GetGenericArguments();
#else
      Type[] types=type.GenericTypeArguments;
#endif

      if(types.Length>0)
      {
        if(types.Length!=1 || type.GetGenericTypeDefinition()!=typeof(Nullable<>))
          throw new NotSupportedException();

        return ParseValue(types[0], value); // Recursive call
      }

      object res=Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
      return res;
    }

    static object ParseTime(string value)
    {
#if NET20
      return TimeSpan.Parse(value);
#else
      return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
#endif
    }

    static Dictionary<Type, SLJsonConverter> CreateStandardConverters()
    {
      var res=new Dictionary<Type, SLJsonConverter>();
      res.Add(typeof(DateTime), delegate(string value) { return DateTime.Parse(value, CultureInfo.InvariantCulture); });
      res.Add(typeof(TimeSpan), ParseTime);
      return res;
    }

    //------------------------------------------------------------------------

    Dictionary<Type, SLJsonConverter> m_Converters=CreateStandardConverters();
  }

  //--------------------------------------------------------------------------
}