/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

#if !NETMF

using System.Globalization;

namespace Liersch.Json
{
  static class JsonConvert // For all platforms, but not .NET MF
  {
    public static string ToString(int value, string format) { return value.ToString(format, CultureInfo.InvariantCulture); }
    public static string ToString(int value) { return value.ToString(CultureInfo.InvariantCulture); }
    public static string ToString(double value) { return value.ToString("R", CultureInfo.InvariantCulture); }
    public static bool TryParse(string text, out double result) { return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result); }
  }
}

#endif