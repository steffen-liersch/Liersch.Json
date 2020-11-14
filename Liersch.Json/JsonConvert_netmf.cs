/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

#if NETMF

namespace Liersch.Json
{
  static class JsonConvert // For .NET MF only
  {
    public static string ToString(int value, string format) { return value.ToString(format); }
    public static string ToString(int value) { return value.ToString(); }
    public static string ToString(double value) { return value.ToString(); }
    public static bool TryParse(string text, out double result) { return double.TryParse(text, out result); }
  }
}

#endif