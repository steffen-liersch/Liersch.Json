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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Liersch.Json
{
  public sealed class SLJsonDeserializer
  {
    public void RegisterConverter(Type type, Func<string, object> converter) { m_Converters[type]=converter; }

    public T Deserialize<T>(string jsonExpression) where T : new()
    {
      SLJsonNode n=SLJsonParser.Parse(jsonExpression);
      return Deserialize<T>(n);
    }

    public T Deserialize<T>(SLJsonNode node) where T : new()
    {
      if(node==null)
        throw new ArgumentNullException("node");

      if(node.IsObject)
        return (T)DeserializeObject(typeof(T), node);

      return new T();
      //return Activator.CreateInstance<T>();
    }


    object DeserializeObject(Type type, SLJsonNode container)
    {
      object res=Activator.CreateInstance(type);

      foreach(FieldInfo fi in type.GetRuntimeFields())
        if(fi.IsPublic)
          foreach(SLJsonMemberAttribute attr in fi.GetCustomAttributes(typeof(SLJsonMemberAttribute), false))
            fi.SetValue(res, DeserializeObjectInternal(container[attr.MemberName], attr.MemberType, fi.FieldType, fi.GetValue(res)));

      foreach(PropertyInfo pi in type.GetRuntimeProperties())
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
                pi.SetValue(res, DeserializeObjectInternal(container[attr.MemberName], attr.MemberType, pi.PropertyType, pi.GetValue(res, null)), null);
            }
          }
        }
      }
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
      int c=array.Count;

      Type elemType;
      Array resArray;
      IList resList;

      if(type.IsArray)
      {
        if(type.GetArrayRank()!=1)
          throw new NotSupportedException("Multi-dimensional arrays are not supported");

        elemType=type.GetElementType();
        resArray=Array.CreateInstance(elemType, c);
        resList=null;
      }
      else
      {
        Type[] args=type.GetGenericArguments();
        elemType=args.Length==1 ? args[0] : null;
        if(elemType==null || !typeof(List<>).MakeGenericType(elemType).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
          throw new NotSupportedException("Type "+type.FullName+" is neither an array nor a List<>");

        resList=(IList)Activator.CreateInstance(type);
        resArray=null;
      }

      object defaultValue=null;
      if(!asObject && !elemType.GetTypeInfo().IsClass)
        defaultValue=Activator.CreateInstance(elemType);

      for(int i=0; i<c; i++)
      {
        object v=defaultValue;
        SLJsonNode n=array[i];
        if(n!=null)
        {
          if(asObject)
          {
            if(n.IsObject)
              v=DeserializeObject(elemType, n);
          }
          else
          {
            if(n.IsValue)
              v=DeserializeValue(elemType, n);
          }
        }

        if(resArray!=null)
          resArray.SetValue(v, i);
        else resList.Add(v);
      }

      return resArray ?? resList;
    }

    object DeserializeValue(Type type, SLJsonNode value) { return ParseValue(type, value.AsString); }


    object ParseValue(Type type, string value)
    {
      Func<string, object> parseValue;
      if(m_Converters.TryGetValue(type, out parseValue))
        return parseValue(value);

      if(typeof(Enum).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        return Enum.Parse(type, value);

      Type[] types=type.GetGenericArguments();
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
#if NET20 || NET30 || NET35
      return TimeSpan.Parse(value);
#else
      return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
#endif
    }

    static Dictionary<Type, Func<string, object>> CreateStandardConverters()
    {
      var res=new Dictionary<Type, Func<string, object>>();
      res.Add(typeof(DateTime), delegate(string value) { return DateTime.Parse(value, CultureInfo.InvariantCulture); });
      res.Add(typeof(TimeSpan), ParseTime);
      return res;
    }


    Dictionary<Type, Func<string, object>> m_Converters=CreateStandardConverters();
  }
}