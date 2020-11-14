/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;

namespace Liersch.Json
{
  public partial class JsonException : Exception
  {
    public JsonException() : base("JSON format exception") { }
    public JsonException(string message) : base(message) { }
    public JsonException(string message, Exception innerException) : base(message, innerException) { }
  }

#if NET20 || NET30 || NET35 || NET40 || NET45 || NET46

  [Serializable]
  partial class JsonException
  {
    protected JsonException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
  }

#endif
}