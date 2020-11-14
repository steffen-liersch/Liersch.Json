/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

#if NETMF

using System.Text;
using Microsoft.SPOT;

namespace System
{
  static class Console // For .NET MF only
  {
    public static void WriteLine(string text)
    {
      if(m_Buffer.Length<=0)
        Debug.Print(text ?? string.Empty);
      else
      {
        if(text!=null)
          m_Buffer.Append(text);
        Debug.Print(m_Buffer.ToString());
        m_Buffer.Length=0;
      }
    }

    public static void WriteLine() { WriteLine(null); }

    public static void Write(string text) { m_Buffer.Append(text); }

    public static void ReadKey(bool intercept) { }

    static StringBuilder m_Buffer=new StringBuilder();
  }
}

#endif