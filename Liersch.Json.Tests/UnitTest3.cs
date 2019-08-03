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
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Liersch.Json.Tests
{
  [TestClass]
  public class UnitTest3
  {
    [TestMethod]
    public void TestReflection()
    {
      var o1=new ExampleOuter()
      {
        ValueString="Test",
        ValueStringArray=new string[] { "A", "B,", "C" },
        ValueDoubleArray=new double[] { 2, 3.14, 10000 },
        PropertyInteger=27,
        PropertyDateTime=new DateTime(2017, 12, 27, 14, 30, 0),
      };

      o1.ValueObject=new ExampleInner(2);
      o1.ValueObjectArray=new ExampleInner[] { new ExampleInner(4), new ExampleInner(6) };
      o1.ChangePrivateValue(2.345f);

      string s1=Serialize(o1);
      //Console.WriteLine(s1);

      var d=new SLJsonDeserializer();
      ExampleOuter o2=d.Deserialize<ExampleOuter>(s1);

      string s2=Serialize(o2);
      Assert.AreEqual(s1, s2);

      SLJsonNode n1=SLJsonParser.Parse(s1);
      SLJsonNode n2=n1.Clone();
      SLJsonNode n3=SLJsonParser.Parse(n2.AsJson);
      SLJsonNode n4=SLJsonParser.Parse(n3.AsJsonCompact);
      Assert.IsTrue(n1!=n2);
      CompareNodes(n1, n1);
      CompareNodes(n1, n2);
      CompareNodes(n1, n3);
      CompareNodes(n1, n4);
    }

    void CompareNodes(SLJsonNode n1, SLJsonNode n2)
    {
      Assert.AreEqual(n1.NodeType, n2.NodeType);
      Assert.AreEqual(n1.AsString, n2.AsString);

      if(n1.IsArray && n2.IsArray)
      {
        int c1=n1.Count;
        int c2=n2.Count;
        Assert.AreEqual(c1, c2);
        if(c1==c2)
          for(int i=0; i<c1; i++)
            CompareNodes(n1[i], n2[i]);
      }

      if(n1.IsObject && n2.IsObject)
      {
        int c1=n1.Count;
        int c2=n2.Count;
        Assert.AreEqual(c1, c2);
        if(c1==c2)
          foreach(string k in n1.Names)
            CompareNodes(n1[k], n2[k]);
      }
    }

    static string Serialize(object instance) { return new SLJsonSerializer().Serialize(instance); }


    [TestMethod]
    public void TesReflectionWithConverter1()
    {
      var ser=new SLJsonSerializer();

      const string prefix="prefix: ";
      ser.RegisterConverter<DateTime>(x => prefix+x.ToString(CultureInfo.InvariantCulture));

      var o1=new ExampleOuter();
      o1.PropertyDateTime=new DateTime(1950, 7, 20, 12, 34, 56);

      string s=ser.Serialize(o1);
      Assert.IsTrue(s.Contains(prefix+o1.PropertyDateTime.ToString(CultureInfo.InvariantCulture)));

      var des=new SLJsonDeserializer();
      des.RegisterConverter<DateTime>(x =>
      {
        string z=x.Substring(x.IndexOf(prefix)+prefix.Length);
        return DateTime.Parse(z, CultureInfo.InvariantCulture);
      });

      var o2=des.Deserialize<ExampleOuter>(s);
      Assert.AreEqual(o1.PropertyDateTime, o2.PropertyDateTime);
    }


    [TestMethod]
    public void TesReflectionWithConverter2()
    {
      var ser=new SLJsonSerializer();
      TestStringConversion(ser, null);

      ser.RegisterConverter<string>(x => x ?? string.Empty);
      TestStringConversion(ser, string.Empty);

      ser.RegisterConverter<string>(x => x ?? "empty");
      TestStringConversion(ser, "empty");
    }

    static void TestStringConversion(SLJsonSerializer serializer, string expectedValue)
    {
      var o1=new ExampleOuter();
      string s=serializer.Serialize(o1);
      var o2=new SLJsonDeserializer().Deserialize<ExampleOuter>(s);
      Assert.AreEqual(expectedValue, o2.ValueString);
    }


    class ExampleOuter
    {
      [SLJsonMember("ValuePrivate")]
      float ValuePrivate;

      [SLJsonMember("ValueObject", SLJsonMemberType.Object)]
      public ExampleInner ValueObject;

      [SLJsonMember("ValueObjectArray", SLJsonMemberType.ObjectArray)]
      public ExampleInner[] ValueObjectArray;

      [SLJsonMember("ValueBoolean1")]
      public bool? ValueBoolean1=null;

      [SLJsonMember("ValueBoolean2")]
      public bool? ValueBoolean2=false;

      [SLJsonMember("ValueBoolean3")]
      public bool? ValueBoolean3=true;

      [SLJsonMember("ValueString")]
      public string ValueString;

      [SLJsonMember("ValueStringArray", SLJsonMemberType.ValueArray)]
      public string[] ValueStringArray;

      [SLJsonMember("ValueDoubleArray", SLJsonMemberType.ValueArray)]
      public double[] ValueDoubleArray;

      [SLJsonMember("PropertyInteger")]
      public int PropertyInteger { get; set; }

      [SLJsonMember("PropertyDateTime ")]
      public DateTime PropertyDateTime { get; set; }

      public void ChangePrivateValue(float value) { ValuePrivate=value; }
    }


    class ExampleInner
    {
      [SLJsonMember("Value")]
      public int Value;

      public int OtherValue;

      public ExampleInner() { }
      public ExampleInner(int value) { Value=value; OtherValue=value*value; }
    }
  }
}