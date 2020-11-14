/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

namespace Liersch.Json
{
  public enum JsonNodeType
  {
    None,
    Missing,
    Null,
    Array,
    Object,
    Boolean,
    Number,
    String,
  }
}