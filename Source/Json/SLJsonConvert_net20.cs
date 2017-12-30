//----------------------------------------------------------------------------
//
// Copyright © 2013-2017 Dipl.-Ing. (BA) Steffen Liersch
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

using System.Globalization;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  static class SLJsonConvert // For all platforms, but not .NET MF
  {
    public static string ToString(int value, string format) { return value.ToString(format, CultureInfo.InvariantCulture); }
    public static string ToString(int value) { return value.ToString(CultureInfo.InvariantCulture); }
    public static string ToString(double value) { return value.ToString(CultureInfo.InvariantCulture); }
    public static bool TryParse(string text, out double result) { return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result); }
  }

  //--------------------------------------------------------------------------
}