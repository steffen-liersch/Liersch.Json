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

using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Runtime.Serialization;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  //[Serializable]
  //[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
  //[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
  public class SLJsonException : Exception
  {
    public SLJsonException() : base("JSON format exception") { }
    public SLJsonException(string message) : base(message) { }
    public SLJsonException(string message, Exception innerException) : base(message, innerException) { }
    //protected SLJsonException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  //--------------------------------------------------------------------------
}