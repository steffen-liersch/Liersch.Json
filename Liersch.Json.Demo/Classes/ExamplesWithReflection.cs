//----------------------------------------------------------------------------
//
// Copyright © 2013-2018 Dipl.-Ing. (BA) Steffen Liersch
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
using System.Collections.Generic;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  static class ExamplesWithReflection
  {
    public static void Run()
    {
      Console.WriteLine("Reflection-based Example");
      Console.WriteLine("========================");
      Console.WriteLine();

      var e1=new Example();
      e1.PersonList=new List<Person>();
      e1.PersonList.Add(new Person() { LastName="Doe", FirstName="John" });
      e1.PersonList.Add(new Person() { LastName="Smith", FirstName="Jane" });
      e1.IntegerList=new List<int> { 10, 20, 30 };
      e1.IntegerArray=new int[] { 700, 800 };
      e1.StringValue="Example Text";
      e1.NotSerializedString="Other Text";

      string json=new SLJsonSerializer().SerializeObject(e1).ToString();
      Example e2=new SLJsonDeserializer().Deserialize<Example>(json);

      string f="{0,-24} => {1,16} - {2}";
      Console.WriteLine(string.Format(f, "Object", "e1", "e2"));

      CompareLists(f, "PersonList", e1.PersonList, e2.PersonList);
      CompareLists(f, "IntegerList", e1.IntegerList, e2.IntegerList);
      CompareLists(f, "IntegerArray", e1.IntegerArray, e2.IntegerArray);

      Console.WriteLine(string.Format(f, "StringValue", e1.StringValue, e2.StringValue));
      Console.WriteLine(string.Format(f, "NotSerializedString", e1.NotSerializedString, e2.NotSerializedString));
      Console.WriteLine();
    }

    static void CompareLists<T>(string format, string name, IList<T> list1, IList<T> list2)
    {
      int c1=list1.Count;
      int c2=list2.Count;

      Console.WriteLine(string.Format(format, name+".Count", c1, c2));

      int c=Math.Min(c1, c2);
      for(int i=0; i<c; i++)
        Console.WriteLine(string.Format(format, name+"["+i+"]", list1[i], list2[i]));
    }

    class Example
    {
      [SLJsonMember("PersonList", SLJsonMemberType.ObjectArray)]
      public List<Person> PersonList;

      [SLJsonMember("IntegerList", SLJsonMemberType.ValueArray)]
      public List<int> IntegerList;

      [SLJsonMember("IntegerArray", SLJsonMemberType.ValueArray)]
      public int[] IntegerArray;

      [SLJsonMember("StringValue")]
      public string StringValue;

      public string NotSerializedString;
    }

    class Person
    {
      [SLJsonMember("LastName")]
      public string LastName;

      [SLJsonMember("FirstName")]
      public string FirstName;

      public override string ToString() { return FirstName+" "+LastName; }
    }
  }

  //--------------------------------------------------------------------------
}