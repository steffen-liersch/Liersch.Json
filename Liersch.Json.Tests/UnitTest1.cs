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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestReadOnly()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);

      Assert.AreEqual("Jane", n["person"]["firstName"].AsString);
      Assert.AreEqual("Doe", n["person"]["lastName"].AsString);
      Assert.AreEqual("12345", n["person"]["zipCode"].AsString);

      Assert.IsFalse(n["person"]["street"].IsReadOnly);
      SLJsonMonitor m=n.CreateMonitor();
      Assert.IsFalse(m.IsModified);
      Assert.IsFalse(m.IsReadOnly);
      m.IsReadOnly=true;
      Assert.IsTrue(n["person"]["street"].IsReadOnly);

      Assert.IsTrue(n["test"]["emptyArray"].IsArray);
      Assert.AreEqual(0, n["test"]["emptyArray"].Count);
      Assert.IsTrue(n["test"]["emptyObject"].IsObject);
      Assert.AreEqual(0, n["test"]["emptyObject"].Count);

      Assert.IsTrue(n["test"]["testArray"].IsArray);
      Assert.AreEqual(4, n["test"]["testArray"].Count);
      Assert.AreEqual(10, n["test"]["testArray"][0].AsInt32);
      Assert.AreEqual(20, n["test"]["testArray"][1].AsInt32);
      Assert.AreEqual(30, n["test"]["testArray"][2].AsInt32);
      Assert.AreEqual(40, n["test"]["testArray"][3].AsInt32);
      Assert.AreEqual(0, n["test"]["testArray"][4].AsInt32); // Access to missing entry
      Assert.IsFalse(n["test"]["testArray"][4].IsValue); // Check missing entry
      Assert.AreEqual(4, n["test"]["testArray"].Count); // Check count again

      Assert.AreEqual(SLJsonNodeType.Object, n["test"]["testObject"].NodeType);
      Assert.AreEqual(2, n["test"]["testObject"].Count);
      Assert.IsNull(n["test"]["testObject"].AsString);

      Assert.AreEqual(SLJsonNodeType.Missing, n["test"]["testValueMissing__"].NodeType);
      Assert.IsFalse(n["test"]["testValueMissing__"].AsBoolean);
      Assert.AreEqual(0, n["test"]["testValueMissing__"].AsInt32);
      Assert.IsNull(n["test"]["testValueMissing__"].AsString);

      Assert.AreEqual(SLJsonNodeType.Null, n["test"]["testValueNull"].NodeType);
      Assert.IsFalse(n["test"]["testValueNull"].AsBoolean);
      Assert.AreEqual(0, n["test"]["testValueNull"].AsInt32);
      Assert.IsNull(n["test"]["testValueNull"].AsString);

      Assert.AreEqual(SLJsonNodeType.Boolean, n["test"]["testValueTrue"].NodeType);
      Assert.IsTrue(n["test"]["testValueTrue"].AsBoolean);
      Assert.AreEqual(1, n["test"]["testValueTrue"].AsInt32);
      Assert.AreEqual("true", n["test"]["testValueTrue"].AsString);

      Assert.AreEqual(SLJsonNodeType.Number, n["test"]["testValue32"].NodeType);
      Assert.AreEqual(256, n["test"]["testValue32"].AsInt32);
      Assert.AreEqual("+256", n["test"]["testValue32"].AsString);

      Assert.AreEqual(SLJsonNodeType.String, n["test"]["testValueString1"].NodeType);
      Assert.AreEqual("abc 'def' ghi", n["test"]["testValueString1"].AsString);

      Assert.AreEqual(SLJsonNodeType.String, n["test"]["testValueString2"].NodeType);
      Assert.AreEqual("ABC 'DEF' GHI", n["test"]["testValueString2"].AsString);

      Assert.AreEqual(SLJsonNodeType.String, n["test"]["testValueString3"].NodeType);
      Assert.AreEqual("First Line\r\nSecond Line\r\nThird Line\0", n["test"]["testValueString3"].AsString);

      // .NET MF seems to work internally with zero-terminated strings. As a result the test case fails.
      if("123\0".Length==4)
        Assert.AreEqual(@"""First Line\r\nSecond Line\r\nThird Line\u0000""", n["test"]["testValueString3"].AsJsonCompact);

      Assert.IsFalse(m.IsModified);
      Assert.IsTrue(m.IsReadOnly);
    }

    [TestMethod]
    public void TestReadWrite()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);
      SLJsonMonitor m=n.CreateMonitor();

      // Try to read some properties
      Assert.AreEqual("Jane", n["person"]["firstName"].AsString);
      Assert.AreEqual("Doe", n["person"]["lastName"].AsString);
      Assert.AreEqual("12345", n["person"]["zipCode"].AsString);
      Assert.IsFalse(m.IsModified);

      try
      {
        m.IsReadOnly=true;
        n["abc"]["def"][100]=0;
        Assert.Fail();
      }
      catch(InvalidOperationException)
      {
        m.IsReadOnly=false;
      }

      Assert.IsFalse(m.IsModified);

      // Try to change an existing property
      n["person"]["firstName"].AsString="John";
      Assert.AreEqual("John", n["person"]["firstName"].AsString);
      Assert.IsTrue(m.IsModified);

      // Try to add a new property
      int c=n["person"].Count;
      Assert.AreEqual(SLJsonNodeType.Missing, n["person"]["newProperty"].NodeType);
      n["person"]["newProperty"].AsInt32=333;
      Assert.AreEqual(c+1, n["person"].Count);
      Assert.AreEqual(SLJsonNodeType.Number, n["person"]["newProperty"].NodeType);
      Assert.AreEqual("333", n["person"]["newProperty"].AsString);

      // Try to delete a property
      c=n["person"].Count;
      Assert.AreEqual(SLJsonNodeType.String, n["person"]["lastName"].NodeType);
      n["person"]["lastName"].Remove();
      Assert.AreEqual(c-1, n["person"].Count);
      Assert.AreEqual(SLJsonNodeType.Missing, n["person"]["lastName"].NodeType);
      Assert.IsNull(n["person"]["lastName"].AsString);
    }

    [TestMethod]
    public void TestCreateNew()
    {
      var n=new SLJsonNode();
      n["person"]["firstName"].AsString="John";
      n["person"]["lastName"].AsString="Doe";
      Assert.AreEqual(1, n.Count);
      Assert.AreEqual(2, n["person"].Count);
      Assert.AreEqual("John", n["person"]["firstName"].AsString);
      Assert.AreEqual("Doe", n["person"]["lastName"].AsString);

      n["intValue"].AsInt32=27;
      Assert.IsTrue(n["intValue"].IsNumber);
      Assert.AreEqual("27", n["intValue"].AsString);
      Assert.IsTrue(n["intValue"].Remove());
      Assert.IsFalse(n["intValue"].Remove());
      Assert.IsTrue(n["intValue"].IsMissing);
      Assert.IsNull(n["intValue"].AsString);

      Assert.IsTrue(n["testArray"].IsMissing);
      n["testArray"][0].AsInt32=11;
      Assert.IsTrue(n["testArray"].IsArray);
      Assert.AreEqual(1, n["testArray"].Count);
      n["testArray"][0].AsInt32=77;
      Assert.AreEqual(1, n["testArray"].Count);
      Assert.AreEqual(77, n["testArray"][0].AsInt32);
      Assert.IsTrue(n["testArray"][1].IsMissing);
      n["testArray"][2].AsInt32=200;
      Assert.IsTrue(n["testArray"][1].IsNull);
      Assert.IsTrue(n["testArray"][2].IsNumber);
      Assert.IsTrue(n["testArray"][3].IsMissing);
      n["testArray"][3].AsInt32=300;
      Assert.IsTrue(n["testArray"][3].IsNumber);

      Assert.AreEqual(4, n["testArray"].Count);
      Assert.IsFalse(n["testArray"][100].Remove());
      Assert.IsFalse(n["testArray"][100].Remove());
      Assert.AreEqual(4, n["testArray"].Count);
      Assert.IsTrue(n["testArray"][1].Remove());
      Assert.AreEqual(3, n["testArray"].Count);

      Assert.IsTrue(n["emptyArray"].IsMissing);
      n["emptyArray"].CreateEmptyArray();
      Assert.IsTrue(n["emptyArray"].IsArray);
      Assert.AreEqual(0, n["emptyArray"].Count);
      Assert.AreEqual("[]", n["emptyArray"].AsJsonCompact);

      Assert.IsTrue(n["emptyObject"].IsMissing);
      n["emptyObject"].CreateEmptyObject();
      Assert.IsTrue(n["emptyObject"].IsObject);
      Assert.AreEqual(0, n["emptyObject"].Count);
      Assert.AreEqual("{}", n["emptyObject"].AsJsonCompact);
    }

    [TestMethod]
    public void TestOperatorsForNull()
    {
      SLJsonNode n=null;
      Assert.IsFalse(n);
      Assert.IsTrue(n==0);
      Assert.IsTrue(n==0L);
      Assert.IsTrue(Math.Abs(n-0.0)<=1e-7);
      Assert.IsTrue((string)n==null);
    }

    [TestMethod]
    public void TestOperatorsForFalse()
    {
      SLJsonNode n=false;
      Assert.IsFalse(n);
      Assert.IsTrue(n==0);
      Assert.IsTrue(n==0L);
      Assert.IsTrue(Math.Abs(n-0.0)<=1e-7);
      Assert.IsTrue(n=="false");
    }

    [TestMethod]
    public void TestOperatorsForTrue()
    {
      SLJsonNode n=true;
      Assert.IsTrue(n);
      Assert.IsTrue(n==1);
      Assert.IsTrue(n==1L);
      Assert.IsTrue(Math.Abs(n-1.0)<=1e-7);
      Assert.IsTrue(n=="true");
    }

    [TestMethod]
    public void TestOperatorsForInteger()
    {
      SLJsonNode n=123;
      Assert.IsTrue(n);
      Assert.IsTrue(n==123);
      Assert.IsTrue(n==123L);
      Assert.IsTrue(Math.Abs(n-123.0)<=1e-7);
      Assert.IsTrue(n=="123");
    }

    [TestMethod]
    public void TestOperatorsForDouble()
    {
      SLJsonNode n=123.456;
      Assert.IsTrue(n);
      Assert.IsTrue(n==123);
      Assert.IsTrue(n==123L);
      Assert.IsTrue(Math.Abs(n-123.456)<=1e-7);
      Assert.IsTrue(n=="123.456");
    }

    [TestMethod]
    public void TestOperatorsForStringFalse()
    {
      SLJsonNode n="false";
      Assert.IsFalse(n);
      Assert.IsTrue(n==0);
      Assert.IsTrue(n==0L);
      Assert.IsTrue(Math.Abs(n-0.0)<=1e-7);
      Assert.IsTrue(n=="false");
    }

    [TestMethod]
    public void TestOperatorsForStringTrue()
    {
      SLJsonNode n="true";
      Assert.IsTrue(n);
      Assert.IsTrue(n==1);
      Assert.IsTrue(n==1L);
      Assert.IsTrue(Math.Abs(n-1.0)<=1e-7);
      Assert.IsTrue(n=="true");
    }

    [TestMethod]
    public void TestOperatorsForStringDouble()
    {
      SLJsonNode n="123.456";
      Assert.IsTrue(n);
      Assert.IsTrue(n==123);
      Assert.IsTrue(n==123L);
      Assert.IsTrue(Math.Abs(n-123.456)<=1e-7);
      Assert.IsTrue(n=="123.456");
    }

    [TestMethod]
    public void TestOperators()
    {
      var n=new SLJsonNode();

      n["person"]["age"]=27;
      Assert.IsTrue(n["person"]["age"].IsNumber);
      Assert.IsTrue(n["person"]["age"]==27);
      Assert.IsTrue(n["person"]["age"]=="27");

      n["person"]["age"].AsInt32=28;
      Assert.IsTrue(n["person"]["age"].IsNumber);
      Assert.IsTrue(n["person"]["age"]==28);
      Assert.IsTrue(n["person"]["age"]=="28");

      Assert.IsTrue(n["person"]["age"].Remove());
      Assert.IsNotNull(n["person"]["age"]);
      Assert.IsTrue(n["person"]["age"].IsMissing);

      n["person"]["age"].AsString="29";
      Assert.IsTrue(n["person"]["age"].IsString);
      Assert.IsTrue(n["person"]["age"]=29);
      Assert.IsTrue(n["person"]["age"]=="29");

      n["person"]["age"]="30";
      Assert.IsTrue(n["person"]["age"].IsString);
      Assert.IsTrue(n["person"]["age"]==30);
      Assert.IsTrue(n["person"]["age"]=="30");

      n["person"]["age"]=null;
      Assert.IsTrue(n["person"]["age"].IsNull);
      Assert.IsFalse(n["person"]["age"].IsString);

      n["person"]["age"]=(string)null;
      Assert.IsTrue(n["person"]["age"].IsNull);
      Assert.IsFalse(n["person"]["age"].IsString);

      n["person"]["age"].AsString=null;
      Assert.IsTrue(n["person"]["age"].IsNull);
      Assert.IsFalse(n["person"]["age"].IsString);

      n["person"]["other"]["property"]="test_1";
      SLJsonNode o=n["person"]["other"]["property"];
      Assert.IsTrue(o.IsString);
      Assert.AreEqual("test_1", o.AsString);
      o.AsString="test_2";
      Assert.AreEqual("test_2", n["person"]["other"]["property"].AsString);

      n["person"]["other"][2]=300;
      n["person"]["other"][1]=200;
      n["person"]["other"][0]=100;

      Assert.AreEqual("test_2", o.AsString);
      o.AsString="test_3";
      Assert.AreEqual("test_3", o.AsString);

      Assert.AreEqual(100, n["person"]["other"][0].AsInt32);
      Assert.AreEqual(200, n["person"]["other"][1].AsInt32);
      Assert.AreEqual(300, n["person"]["other"][2].AsInt32);
      Assert.AreEqual(null, n["person"]["other"]["property"].AsString);
      Assert.IsTrue(n["person"]["other"]["property"].IsMissing);
    }

    [TestMethod]
    public void TestSerialization()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);

      n["newProperty"]["value"].AsInt32=27;
      Assert.AreEqual(SLJsonNodeType.Number, n["newProperty"]["value"].NodeType);
      Assert.AreEqual("27", n["newProperty"]["value"].AsJson);
      Assert.AreEqual("{\"value\":27}", RemoveWhitespace(n["newProperty"].AsJson));

      n["newProperty"]["value"][0].AsInt32=100;
      n["newProperty"]["value"][3].AsInt32=333;
      Assert.AreEqual(SLJsonNodeType.Array, n["newProperty"]["value"].NodeType);
      Assert.AreEqual("{\"value\":[100,null,null,333]}", n["newProperty"].AsJsonCompact);

      n["newProperty"]["value"].AsString="Text";
      Assert.AreEqual(SLJsonNodeType.String, n["newProperty"]["value"].NodeType);

      n["newProperty"]["value"].AsString=null;
      Assert.AreEqual(SLJsonNodeType.Null, n["newProperty"]["value"].NodeType);

      Assert.IsTrue(n["newProperty"]["value"].Remove());
      Assert.AreEqual(SLJsonNodeType.Missing, n["newProperty"]["value"].NodeType);
    }

    [TestMethod]
    public void TestParseInvalid()
    {
      ParseInvalid("\n'abc def", "Syntax error in JSON expression at row 2 in column 1: Unterminated string expression");
      ParseInvalid("\r'abc def \\", "Syntax error in JSON expression at row 2 in column 1: Unterminated string expression");

      ParseInvalid("\r\r'abc def \\u", "Syntax error in JSON expression at row 3 in column 12: Unexpected end of escape sequence");
      ParseInvalid("\n\r\n'abc def \\u00", "Syntax error in JSON expression at row 3 in column 12: Unexpected end of escape sequence");
      ParseInvalid("\r\n\n'abc def \\u000", "Syntax error in JSON expression at row 3 in column 12: Unexpected end of escape sequence");

      ParseInvalid("\n\n'abc \\A def'", "Syntax error in JSON expression at row 3 in column 7: Unsupported escape sequence: \\A");
      ParseInvalid("\n\r\n\r\n\r'abc \\u004G def'", "Syntax error in JSON expression at row 4 in column 11: Invalid escape sequence: \\u004G");
      ParseInvalid("\r\n\r\n\r\n' abc \\u00G4 def'", "Syntax error in JSON expression at row 4 in column 11: Invalid escape sequence: \\u00G");

      ParseInvalid("{ x x }", "Syntax error in JSON expression at row 1 in column 5: Colon expected");
      ParseInvalid("{ x: 1, [0, 1, 2] }", "Syntax error in JSON expression at row 1 in column 9: Unexpected token");
      ParseInvalid("{ x: 1 y: 2}", "Syntax error in JSON expression at row 1 in column 8: Separator expected");
      ParseInvalid("{ x: 1, y: [0, 1 2] }", "Syntax error in JSON expression at row 1 in column 18: Separator expected");

      ParseInvalid("\n   {}   [", "Syntax error in JSON expression at row 2 in column 9: Unexpected special character: [");
      ParseInvalid("\n   { }   \n  \n  \r  foo", "Syntax error in JSON expression at row 5 in column 3: Unexpected token: foo");
      ParseInvalid("\n   {  }   \r\n\n\r\n\n       '123'", "Syntax error in JSON expression at row 6 in column 8: Unexpected string: 123");
    }

    [TestMethod]
    public void TestParseAndSerialize()
    {
      ParseAndSerialize("{\"a\": 1, \"b\": 2}", SLJsonNodeType.Object);
      ParseAndSerialize("[{\"a\":1, \"b\":2}, 3, 4]", SLJsonNodeType.Array);
      ParseAndSerialize("null", SLJsonNodeType.Null);
      ParseAndSerialize("false", SLJsonNodeType.Boolean);
      ParseAndSerialize("true", SLJsonNodeType.Boolean);
      ParseAndSerialize("1234", SLJsonNodeType.Number);
      ParseAndSerialize("\"text\"", SLJsonNodeType.String);
    }


    void ParseInvalid(string jsonExpression, string expectedErrorMessage)
    {
      try
      {
        SLJsonParser.Parse(jsonExpression, true);
        Assert.Fail();
      }
      catch(SLJsonException e)
      {
        Assert.AreEqual(expectedErrorMessage, e.Message);
      }
    }

    void ParseAndSerialize(string jsonExpression, SLJsonNodeType nodeType)
    {
      if(nodeType!=SLJsonNodeType.Object)
        ParseAndSerialize(jsonExpression, true, nodeType);
      else
      {
        ParseAndSerialize(jsonExpression, false, nodeType);
        ParseAndSerialize(jsonExpression, true, nodeType);
      }
    }

    void ParseAndSerialize(string jsonExpression, bool allowArraysAndValues, SLJsonNodeType nodeType)
    {
      SLJsonNode n=SLJsonParser.Parse(jsonExpression, allowArraysAndValues);
      Assert.AreEqual(nodeType, n.NodeType);

      string s=RemoveWhitespace(jsonExpression);
      Assert.AreEqual(s, n.AsJsonCompact);
    }

    static string RemoveWhitespace(string value)
    {
      if(value==null)
        return null;

      int c=value.Length;
      var sb=new StringBuilder(c);
      for(int i=0; i<c; i++)
      {
        char z=value[i];
        if(z>' ')
          sb.Append(z);
      }

      return sb.ToString();
    }

    static string RetrieveJsonExpression()
    {
      return @"
      {
        person: {
          firstName: 'Jane',
          lastName: 'Doe',
          street: 'Main',
          number: '123 c',
          city: 'Democity',
          zipCode: '12345'
        },
        test: {
          emptyArray: [],
          emptyObject: {},
          testArray: [10, 20, 30, 40],
          testObject: {a: 'First', b: 'Second'},
          testValueNull: null,
          testValueFalse: false,
          testValueTrue: true,
          testValue32: +256,
          testValue64: 10000000000000000,
          testValueDouble: 3.1415,
          testValueString1: 'abc \'def\' ghi',
          testValueString2: 'A\u0042C \'DE\106\' GHI',
          testValueString3: 'First Line\r\nSecond Line\r\nThird Line\0'
        }
      }";
    }
  }
}