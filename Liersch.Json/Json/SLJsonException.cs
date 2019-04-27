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

namespace Liersch.Json
{
  public partial class SLJsonException : Exception
  {
    public SLJsonException() : base("JSON format exception") { }
    public SLJsonException(string message) : base(message) { }
    public SLJsonException(string message, Exception innerException) : base(message, innerException) { }
  }

#if NET20 || NET30 || NET35 || NET40 || NET45 || NET46

  [Serializable]
  partial class SLJsonException
  {
    protected SLJsonException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }

#endif
}