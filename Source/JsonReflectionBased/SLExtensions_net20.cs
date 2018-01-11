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

#if NET20 || NET30 || NET35 || NET40

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  static class SLExtensions // Required for .NET 2.0 up to 4.x; Does not work for .NET Standard
  {
    public static Type GetTypeInfo(this Type type) { return type; }
    public static IEnumerable<FieldInfo> GetRuntimeFields(this Type type) { return type.GetFields(); }
    public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type) { return type.GetProperties(); }
  }

  //--------------------------------------------------------------------------
}

#endif