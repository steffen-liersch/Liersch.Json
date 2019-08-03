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
      ExampleOuter o1=CreateExample();

      string s1=new SLJsonSerializer().Serialize(o1);

      var d=new SLJsonDeserializer();
      ExampleOuter o2=d.Deserialize<ExampleOuter>(s1);

      CompareSomeFields(o1, o2);
      Assert.AreEqual(2.345f, o1.RetrievePrivateValue());
      Assert.AreEqual(0, o2.RetrievePrivateValue());

      string s2=new SLJsonSerializer().Serialize(o2);
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

    static ExampleOuter CreateExample()
    {
      var res=new ExampleOuter()
      {
        ValueString="Test",
        ValueStringArray=new string[] { "A", "B,", "C" },
        ValueDoubleArray=new double[] { 2, 3.14, 10000 },
        PropertyInteger=27,
        PropertyDateTime=new DateTime(2017, 12, 27, 14, 30, 0),
      };

      res.ValueObject=new ExampleInner(2);
      res.ValueObjectArray=new ExampleInner[] { new ExampleInner(4), new ExampleInner(6) };
      res.ChangePrivateValue(2.345f);
      return res;
    }

    static void CompareSomeFields(ExampleOuter o1, ExampleOuter o2)
    {
      Assert.AreEqual(o1.ValueBoolean1, o2.ValueBoolean1);
      Assert.AreEqual(o1.ValueBoolean2, o2.ValueBoolean2);
      Assert.AreEqual(o1.ValueBoolean3, o2.ValueBoolean3);
      Assert.AreEqual(o1.ValueString, o2.ValueString);
      Assert.AreEqual(o1.PropertyInteger, o2.PropertyInteger);
      Assert.AreEqual(o1.PropertyDateTime, o2.PropertyDateTime);
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


    [TestMethod]
    public void TestObsoleteSerializeObject()
    {
      ExampleOuter o1=CreateExample();

      var wr1=new SLJsonWriter();

#pragma warning disable 618 // Disable obsolete warning

      var ser=new SLJsonSerializer(wr1);
      SLJsonWriter wr2=ser.SerializeObject(o1);
      string s1=ser.ToString();

#pragma warning restore 618

      string s2=wr1.ToString();

      Assert.AreSame(wr1, wr2);
      Assert.AreEqual(s1, s2);

      var o2=new SLJsonDeserializer().Deserialize<ExampleOuter>(s1);
      CompareSomeFields(o1, o2);
    }

    [TestMethod]
    public void TestObsoleteSerializeObject2()
    {
      ExampleOuter o1=CreateExample();
      var ser=new SLJsonSerializer();

#pragma warning disable 618 // Disable obsolete warning

      SLJsonWriter wr=ser.SerializeObject(o1);
      string s1=ser.ToString();

#pragma warning restore 618

      string s2=wr.ToString();
      Assert.AreEqual(s1, s2);

      var o2=new SLJsonDeserializer().Deserialize<ExampleOuter>(s1);
      CompareSomeFields(o1, o2);
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

      public float RetrievePrivateValue() { return ValuePrivate; }

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