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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class UnitTest2
  {
    [TestMethod]
    public void TestSystematically()
    {
      SLJsonNodeType nt;

      nt=SLJsonNodeType.Missing;
      Check("{other: 123}", false, false, false, false, nt, false, 0, 0, 0, null);

      nt=SLJsonNodeType.Null;
      Check("{value: null}", true, false, false, false, nt, false, 0, 0, 0, null);

      nt=SLJsonNodeType.Boolean;
      Check("{value: false}", false, false, false, true, nt, false, 0, 0, 0, "false");
      Check("{value: true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check("{value: true }", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check("{value:true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" {value:true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { value:true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { value :true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { value : true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { value : true }", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { value : true } ", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { value :\ttrue\t}\t", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check("{\"value\":true}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { \"value\" : true } ", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check(" { 'value' : true } ", false, false, false, true, nt, true, 0, 0, 0, "true"); // Single-quotation marks are not allowed for JSON expressions

      nt=SLJsonNodeType.Number;
      Check("{value: 123}", false, false, false, true, nt, true, 123, 123, 123, "123");
      Check("{value: 1.23}", false, false, false, true, nt, true, 1, 1, 1.23, "1.23");
      Check("{value: 1.89}", false, false, false, true, nt, true, 1, 1, 1.89, "1.89");
      Check("{value: 0.123}", false, false, false, true, nt, false, 0, 0, 0.123, "0.123");
      Check("{value: .123}", false, false, false, true, nt, false, 0, 0, 0.123, ".123");
      Check("{value: 1e-100}", false, false, false, true, nt, false, 0, 0, 1e-100, "1e-100");
      Check("{value: 1.23e-100}", false, false, false, true, nt, false, 0, 0, 1.23e-100, "1.23e-100");
      Check("{value: 1e+100}", false, false, false, true, nt, true, 0, 0, 1e+100, "1e+100");
      Check("{value: 1.23e+100}", false, false, false, true, nt, true, 0, 0, 1.23e+100, "1.23e+100");

      nt=SLJsonNodeType.String;
      Check("{value: \"text\"}", false, false, false, true, nt, false, 0, 0, 0, "text");
      Check("{value: \"text with \\\" escape sequence\"}", false, false, false, true, nt, false, 0, 0, 0, "text with \" escape sequence");
      Check("{value: \"text with \\\' escape sequence\"}", false, false, false, true, nt, false, 0, 0, 0, "text with \' escape sequence");
      Check("{value: \'text with \\\' escape sequence\'}", false, false, false, true, nt, false, 0, 0, 0, "text with \' escape sequence");
      Check("{value: \"Special_\\0\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_\0");
      Check("{value: \"Special_\\10\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_\b");
      Check("{value: \"Special_\\108\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_\b8");
      Check("{value: \"Special_\\101\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_A");
      Check("{value: \"Special_\\1010\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_A0");
      Check("{value: \"Special_\\1018\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_A8");
      Check("{value: \"Special_\\u0041\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_A");
      Check("{value: \"Special_\\\\_\\/_/_\\b\\t\\n\\v\\f\\r\"}", false, false, false, true, nt, false, 0, 0, 0, "Special_\\_/_/_\b\t\n\v\f\r");
      Check("{value: \"null\"}", false, false, false, true, nt, false, 0, 0, 0, "null");
      Check("{value: \"false\"}", false, false, false, true, nt, false, 0, 0, 0, "false");
      Check("{value: \"FALSE\"}", false, false, false, true, nt, false, 0, 0, 0, "FALSE");
      Check("{value: \"true\"}", false, false, false, true, nt, true, 0, 0, 0, "true");
      Check("{value: \"TRUE\"}", false, false, false, true, nt, false, 0, 0, 0, "TRUE");
      Check("{value: \"123\"}", false, false, false, true, nt, true, 123, 123, 123, "123");
      Check("{value: 'single-quoted'}", false, false, false, true, nt, false, 0, 0, 0, "single-quoted"); // Single-quotation marks are not allowed for JSON expressions

      Check("{value: INVALID}", false, false, false, true, SLJsonNodeType.Number, false, 0, 0, 0, "INVALID");
    }

    void Check(
      string json,
      bool isNull, bool isArray, bool isObject, bool isValue, SLJsonNodeType valueType,
      bool valueBoolean, int valueInt32, long valueInt64, double valueNumber, string valueString)
    {
      SLJsonNode parsed=SLJsonParser.Parse(json);
      SLJsonNode n=parsed["value"];

      Assert.AreEqual(isArray, n.IsArray);
      Assert.AreEqual(isNull, n.IsNull);
      Assert.AreEqual(isObject, n.IsObject);
      Assert.AreEqual(isValue, n.IsValue);

      Assert.AreEqual(valueType, n.NodeType);
      Assert.AreEqual(n.IsBoolean || n.IsNumber || n.IsString, n.IsValue);
      Assert.AreEqual(valueType==SLJsonNodeType.Boolean, n.IsBoolean);
      Assert.AreEqual(valueType==SLJsonNodeType.Number, n.IsNumber);
      Assert.AreEqual(valueType==SLJsonNodeType.String, n.IsString);

      Assert.AreEqual(valueBoolean, n.AsBoolean);
      Assert.AreEqual(valueInt32, n.AsInt32);
      Assert.AreEqual(valueInt64, n.AsInt64);
      Assert.IsTrue(Math.Abs(n.AsDouble-valueNumber)<=1e-7);
      Assert.AreEqual(valueString, n.AsString);
    }
  }
}