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

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  sealed class UnitTest1
  {
    public void Run()
    {
      Test=new UnitTestHelper();
      Test.PrintHeadline("UnitTest1 - Basic Features");

      TestReadOnly();
      TestReadWrite();
      TestCreateNew();
      TestOperators();
      TestSerialization();
      TestParseInvalid();
      TestParseAndSerialize();

      Test.PrintSummary();
      Test=null;
    }

    //------------------------------------------------------------------------

    void TestReadOnly()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);

      Test.Assert(() => n["person"]["firstName"].AsString=="Jane");
      Test.Assert(() => n["person"]["lastName"].AsString=="Doe");
      Test.Assert(() => n["person"]["zipCode"].AsString=="12345");

      Test.Assert(() => !n["person"]["street"].IsReadOnly);
      SLJsonMonitor m=n.CreateMonitor();
      Test.Assert(() => !m.IsModified);
      Test.Assert(() => !m.IsReadOnly);
      m.IsReadOnly=true;
      Test.Assert(() => n["person"]["street"].IsReadOnly);

      Test.Assert(() => n["test"]["emptyArray"].IsArray);
      Test.Assert(() => n["test"]["emptyArray"].Count==0);
      Test.Assert(() => n["test"]["emptyObject"].IsObject);
      Test.Assert(() => n["test"]["emptyObject"].Count==0);

      Test.Assert(() => n["test"]["testArray"].IsArray);
      Test.Assert(() => n["test"]["testArray"].Count==4);
      Test.Assert(() => n["test"]["testArray"][0].AsInt32==10);
      Test.Assert(() => n["test"]["testArray"][1].AsInt32==20);
      Test.Assert(() => n["test"]["testArray"][2].AsInt32==30);
      Test.Assert(() => n["test"]["testArray"][3].AsInt32==40);
      Test.Assert(() => n["test"]["testArray"][4].AsInt32==0); // Access to missing entry
      Test.Assert(() => !n["test"]["testArray"][4].IsValue); // Check missing entry
      Test.Assert(() => n["test"]["testArray"].Count==4); // Check count again

      Test.Assert(() => n["test"]["testObject"].NodeType==SLJsonNodeType.Object);
      Test.Assert(() => n["test"]["testObject"].Count==2);
      Test.Assert(() => n["test"]["testObject"].AsString==null);

      Test.Assert(() => n["test"]["testValueMissing__"].NodeType==SLJsonNodeType.Missing);
      Test.Assert(() => !n["test"]["testValueMissing__"].AsBoolean);
      Test.Assert(() => n["test"]["testValueMissing__"].AsInt32==0);
      Test.Assert(() => n["test"]["testValueMissing__"].AsString==null);

      Test.Assert(() => n["test"]["testValueNull"].NodeType==SLJsonNodeType.Null);
      Test.Assert(() => !n["test"]["testValueNull"].AsBoolean);
      Test.Assert(() => n["test"]["testValueNull"].AsInt32==0);
      Test.Assert(() => n["test"]["testValueNull"].AsString==null);

      Test.Assert(() => n["test"]["testValueTrue"].NodeType==SLJsonNodeType.Boolean);
      Test.Assert(() => n["test"]["testValueTrue"].AsBoolean);
      Test.Assert(() => n["test"]["testValueTrue"].AsInt32==0);
      Test.Assert(() => n["test"]["testValueTrue"].AsString=="true");

      Test.Assert(() => n["test"]["testValue32"].NodeType==SLJsonNodeType.Number);
      Test.Assert(() => n["test"]["testValue32"].AsInt32==256);
      Test.Assert(() => n["test"]["testValue32"].AsString=="+256");

      Test.Assert(() => n["test"]["testValueString1"].NodeType==SLJsonNodeType.String);
      Test.Assert(() => n["test"]["testValueString1"].AsString=="abc 'def' ghi");

      Test.Assert(() => n["test"]["testValueString2"].NodeType==SLJsonNodeType.String);
      Test.Assert(() => n["test"]["testValueString2"].AsString=="ABC 'DEF' GHI");

      Test.Assert(() => n["test"]["testValueString3"].NodeType==SLJsonNodeType.String);
      Test.Assert(() => n["test"]["testValueString3"].AsString=="First Line\r\nSecond Line\r\nThird Line\0");

      // .NET MF seems to work internally with zero-terminated strings. As a result the test case fails.
      if("123\0".Length==4)
        Test.Assert(() => n["test"]["testValueString3"].AsJsonCompact==@"""First Line\r\nSecond Line\r\nThird Line\u0000""");

      Test.Assert(() => !m.IsModified);
      Test.Assert(() => m.IsReadOnly);
    }

    void TestReadWrite()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);
      SLJsonMonitor m=n.CreateMonitor();

      // Try to read some properties
      Test.Assert(() => n["person"]["firstName"].AsString=="Jane");
      Test.Assert(() => n["person"]["lastName"].AsString=="Doe");
      Test.Assert(() => n["person"]["zipCode"].AsString=="12345");
      Test.Assert(() => !m.IsModified);

      try
      {
        m.IsReadOnly=true;
        n["abc"]["def"][100]=0;
        Test.Assert(() => false);
      }
      catch(InvalidOperationException)
      {
        m.IsReadOnly=false;
        Test.Assert(() => true);
      }

      Test.Assert(() => !m.IsModified);

      // Try to change an existing property
      n["person"]["firstName"].AsString="John";
      Test.Assert(() => n["person"]["firstName"].AsString=="John");
      Test.Assert(() => m.IsModified);

      // Try to add a new property
      int c=n["person"].Count;
      Test.Assert(() => n["person"]["newProperty"].NodeType==SLJsonNodeType.Missing);
      n["person"]["newProperty"].AsInt32=333;
      Test.Assert(() => n["person"].Count==c+1);
      Test.Assert(() => n["person"]["newProperty"].NodeType==SLJsonNodeType.Number);
      Test.Assert(() => n["person"]["newProperty"].AsString=="333");

      // Try to delete a property
      c=n["person"].Count;
      Test.Assert(() => n["person"]["lastName"].NodeType==SLJsonNodeType.String);
      n["person"]["lastName"].Remove();
      Test.Assert(() => n["person"].Count==c-1);
      Test.Assert(() => n["person"]["lastName"].NodeType==SLJsonNodeType.Missing);
      Test.Assert(() => n["person"]["lastName"].AsString==null);
    }

    void TestCreateNew()
    {
      var n=new SLJsonNode();
      n["person"]["firstName"].AsString="John";
      n["person"]["lastName"].AsString="Doe";
      Test.Assert(() => n.Count==1);
      Test.Assert(() => n["person"].Count==2);
      Test.Assert(() => n["person"]["firstName"].AsString=="John");
      Test.Assert(() => n["person"]["lastName"].AsString=="Doe");

      n["intValue"].AsInt32=27;
      Test.Assert(() => n["intValue"].IsNumber);
      Test.Assert(() => n["intValue"].AsString=="27");
      Test.Assert(() => n["intValue"].Remove());
      Test.Assert(() => !n["intValue"].Remove());
      Test.Assert(() => n["intValue"].IsMissing);
      Test.Assert(() => n["intValue"].AsString==null);

      Test.Assert(() => n["testArray"].IsMissing);
      n["testArray"][0].AsInt32=11;
      Test.Assert(() => n["testArray"].IsArray);
      Test.Assert(() => n["testArray"].Count==1);
      n["testArray"][0].AsInt32=77;
      Test.Assert(() => n["testArray"].Count==1);
      Test.Assert(() => n["testArray"][0].AsInt32==77);
      Test.Assert(() => n["testArray"][1].IsMissing);
      n["testArray"][2].AsInt32=200;
      Test.Assert(() => n["testArray"][1].IsNull);
      Test.Assert(() => n["testArray"][2].IsNumber);
      Test.Assert(() => n["testArray"][3].IsMissing);
      n["testArray"][3].AsInt32=300;
      Test.Assert(() => n["testArray"][3].IsNumber);

      Test.Assert(() => n["testArray"].Count==4);
      Test.Assert(() => !n["testArray"][100].Remove());
      Test.Assert(() => !n["testArray"][100].Remove());
      Test.Assert(() => n["testArray"].Count==4);
      Test.Assert(() => n["testArray"][1].Remove());
      Test.Assert(() => n["testArray"].Count==3);

      Test.Assert(() => n["emptyArray"].IsMissing);
      n["emptyArray"].CreateEmptyArray();
      Test.Assert(() => n["emptyArray"].IsArray);
      Test.Assert(() => n["emptyArray"].Count==0);
      Test.Assert(() => n["emptyArray"].AsJsonCompact=="[]");

      Test.Assert(() => n["emptyObject"].IsMissing);
      n["emptyObject"].CreateEmptyObject();
      Test.Assert(() => n["emptyObject"].IsObject);
      Test.Assert(() => n["emptyObject"].Count==0);
      Test.Assert(() => n["emptyObject"].AsJsonCompact=="{}");
    }

    void TestOperators()
    {
      var n=new SLJsonNode();

      n["person"]["age"]=27;
      Test.Assert(() => n["person"]["age"].IsNumber);
      Test.Assert(() => n["person"]["age"]==27);
      Test.Assert(() => n["person"]["age"]=="27");

      n["person"]["age"].AsInt32=28;
      Test.Assert(() => n["person"]["age"].IsNumber);
      Test.Assert(() => n["person"]["age"]==28);
      Test.Assert(() => n["person"]["age"]=="28");

      Test.Assert(() => n["person"]["age"].Remove());
      Test.Assert(() => n["person"]["age"]!=null);
      Test.Assert(() => n["person"]["age"].IsMissing);

      n["person"]["age"].AsString="29";
      Test.Assert(() => n["person"]["age"].IsString);
      Test.Assert(() => n["person"]["age"]==29);
      Test.Assert(() => n["person"]["age"]=="29");

      n["person"]["age"]="30";
      Test.Assert(() => n["person"]["age"].IsString);
      Test.Assert(() => n["person"]["age"]==30);
      Test.Assert(() => n["person"]["age"]=="30");

      n["person"]["age"]=null;
      Test.Assert(() => n["person"]["age"].IsNull);
      Test.Assert(() => !n["person"]["age"].IsString);

      n["person"]["age"]=(string)null;
      Test.Assert(() => n["person"]["age"].IsNull);
      Test.Assert(() => !n["person"]["age"].IsString);

      n["person"]["age"].AsString=null;
      Test.Assert(() => n["person"]["age"].IsNull);
      Test.Assert(() => !n["person"]["age"].IsString);

      n["person"]["other"]["property"]="test_1";
      SLJsonNode o=n["person"]["other"]["property"];
      Test.Assert(() => o.IsString);
      Test.Assert(() => o.AsString=="test_1");
      o.AsString="test_2";
      Test.Assert(() => n["person"]["other"]["property"].AsString=="test_2");

      n["person"]["other"][2]=300;
      n["person"]["other"][1]=200;
      n["person"]["other"][0]=100;

      Test.Assert(() => o.AsString=="test_2");
      o.AsString="test_3";
      Test.Assert(() => o.AsString=="test_3");

      Test.Assert(() => n["person"]["other"][0].AsInt32==100);
      Test.Assert(() => n["person"]["other"][1].AsInt32==200);
      Test.Assert(() => n["person"]["other"][2].AsInt32==300);
      Test.Assert(() => n["person"]["other"]["property"].AsString==null);
      Test.Assert(() => n["person"]["other"]["property"].IsMissing);
    }

    void TestSerialization()
    {
      string s=RetrieveJsonExpression();
      SLJsonNode n=SLJsonParser.Parse(s);

      n["newProperty"]["value"].AsInt32=27;
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Number);
      Test.Assert(() => n["newProperty"]["value"].AsJson=="27");
      Test.Assert(() => RemoveWhitespace(n["newProperty"].AsJson)=="{\"value\":27}");

      n["newProperty"]["value"][0].AsInt32=100;
      n["newProperty"]["value"][3].AsInt32=333;
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Array);
      Test.Assert(() => n["newProperty"].AsJsonCompact=="{\"value\":[100,null,null,333]}");

      n["newProperty"]["value"].AsString="Text";
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.String);

      n["newProperty"]["value"].AsString=null;
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Null);

      Test.Assert(() => n["newProperty"]["value"].Remove());
      Test.Assert(() => n["newProperty"]["value"].NodeType==SLJsonNodeType.Missing);
    }

    void TestParseInvalid()
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

    void TestParseAndSerialize()
    {
      ParseAndSerialize("{\"a\": 1, \"b\": 2}", SLJsonNodeType.Object);
      ParseAndSerialize("[{\"a\":1, \"b\":2}, 3, 4]", SLJsonNodeType.Array);
      ParseAndSerialize("null", SLJsonNodeType.Null);
      ParseAndSerialize("false", SLJsonNodeType.Boolean);
      ParseAndSerialize("true", SLJsonNodeType.Boolean);
      ParseAndSerialize("1234", SLJsonNodeType.Number);
      ParseAndSerialize("\"text\"", SLJsonNodeType.String);
    }

    //------------------------------------------------------------------------

    void ParseInvalid(string jsonExpression, string expectedErrorMessage)
    {
      try
      {
        SLJsonParser.Parse(jsonExpression, true);
        Test.Assert(() => false);
      }
      catch(SLJsonException e)
      {
        Test.Assert(() => e.Message==expectedErrorMessage);
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
      Test.Assert(() => n.NodeType==nodeType);

      string s=RemoveWhitespace(jsonExpression);
      Test.Assert(() => n.AsJsonCompact==s);
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

    //------------------------------------------------------------------------

    UnitTestHelper Test;
  }

  //--------------------------------------------------------------------------
}