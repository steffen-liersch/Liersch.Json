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

#if NET20 || NET30

namespace System.Runtime.CompilerServices
{
  //--------------------------------------------------------------------------

  /// <summary> This attribute enables extension methods in .NET 2.0 and 3.0. </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
  sealed class ExtensionAttribute : Attribute
  {
  }

  //--------------------------------------------------------------------------
}

#endif