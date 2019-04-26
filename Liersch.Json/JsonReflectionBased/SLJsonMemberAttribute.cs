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

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public sealed class SLJsonMemberAttribute : Attribute
  {
    public SLJsonMemberAttribute(string memberName) : this(memberName, SLJsonMemberType.Value) { }
    public SLJsonMemberAttribute(string memberName, SLJsonMemberType memberType) { m_MemberName=memberName; m_MemberType=memberType; }
    public string MemberName { get { return m_MemberName; } }
    public SLJsonMemberType MemberType { get { return m_MemberType; } }
    readonly string m_MemberName;
    readonly SLJsonMemberType m_MemberType;
  }

  //--------------------------------------------------------------------------
}