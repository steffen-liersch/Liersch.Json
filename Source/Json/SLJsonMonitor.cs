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

  public sealed class SLJsonMonitor
  {
    public bool IsModified { get { return m_IsModified; } set { m_IsModified=value; } }
    public bool IsReadOnly { get { return m_IsReadOnly; } set { m_IsReadOnly=value; } }

    bool m_IsModified;
    bool m_IsReadOnly;
  }

  //--------------------------------------------------------------------------
}