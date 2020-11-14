/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

namespace Liersch.Json
{
  public sealed class JsonMonitor
  {
    public bool IsModified { get; set; }

    public bool IsReadOnly { get; set; }

    internal JsonMonitor() { }
  }
}