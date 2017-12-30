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

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  static class SLJsonConvert // For .NET MF only
  {
    public static string ToString(int value, string format) { return value.ToString(format); }
    public static string ToString(int value) { return value.ToString(); }
    public static string ToString(double value) { return value.ToString(); }
    public static bool TryParse(string text, out double result) { return double.TryParse(text, out result); }
  }

  //--------------------------------------------------------------------------
}