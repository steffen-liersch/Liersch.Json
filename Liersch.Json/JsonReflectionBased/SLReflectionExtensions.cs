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

namespace System.Reflection
{
  static partial class SLReflectionExtensions
  {
#if NET20 || NET30 || NET35 || NET40
    public static Type GetTypeInfo(this Type type) { return type; }
    public static System.Collections.Generic.IEnumerable<FieldInfo> GetRuntimeFields(this Type type) { return type.GetFields(); }
    public static System.Collections.Generic.IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type) { return type.GetProperties(); }
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETSTANDARD2_0
    public static Type[] GetGenericArguments(this Type type) { return type.GetGenericArguments(); }
    public static MethodInfo GetGetMethod(this PropertyInfo info) { return info.GetMethod; }
    public static MethodInfo GetSetMethod(this PropertyInfo info) { return info.SetMethod; }
#endif
  }
}