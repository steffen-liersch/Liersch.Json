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

namespace Liersch.Json
{
  partial class SLJsonNode
  {
    public static implicit operator SLJsonNode(bool value) { return new SLJsonNode(SLJsonNodeType.Boolean, value ? "true" : "false"); }
    public static implicit operator SLJsonNode(int value) { return new SLJsonNode(SLJsonNodeType.Number, SLJsonConvert.ToString(value)); }
    public static implicit operator SLJsonNode(long value) { return new SLJsonNode(SLJsonNodeType.Number, SLJsonConvert.ToString(value)); }
    public static implicit operator SLJsonNode(double value) { return new SLJsonNode(SLJsonNodeType.Number, SLJsonConvert.ToString(value)); }
    public static implicit operator SLJsonNode(string value) { return new SLJsonNode(value!=null ? SLJsonNodeType.String : SLJsonNodeType.Null, value); }

    public static implicit operator bool(SLJsonNode node) { return node!=null && node.AsBoolean; }
    public static implicit operator int(SLJsonNode node) { return node!=null ? node.AsInt32 : 0; }
    public static implicit operator long(SLJsonNode node) { return node!=null ? node.AsInt64 : 0; }
    public static implicit operator double(SLJsonNode node) { return node!=null ? node.AsDouble : 0; }
    public static implicit operator string(SLJsonNode node) { return node!=null ? node.AsString : null; }
  }
}