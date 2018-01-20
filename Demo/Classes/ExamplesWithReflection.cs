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
      e1.IntegerArray=new int[] { 10, 20, 30, 700, 800 };
      e1.StringValue="Example Text";
      e1.NotSerializedString="Other Text";

      string json=new SLJsonSerializer().SerializeObject(e1).ToString();
      Example e2=new SLJsonDeserializer().Deserialize<Example>(json);

      string f="{0,-24} => {1,16} - {2}";
      Console.WriteLine(string.Format(f, "Object", "e1", "e2"));

      int c1=e1.IntegerArray.Length;
      int c2=e2.IntegerArray.Length;

      Console.WriteLine(string.Format(f, "IntegerArray.Length", c1, c2));

      int c=Math.Min(c1, c2);
      for(int i=0; i<c; i++)
        Console.WriteLine(string.Format(f, "IntegerArray["+i+"]", e1.IntegerArray[i], e2.IntegerArray[i]));

      Console.WriteLine(string.Format(f, "StringValue", e1.StringValue, e2.StringValue));
      Console.WriteLine(string.Format(f, "NotSerializedString", e1.NotSerializedString, e2.NotSerializedString));
      Console.WriteLine();
    }

    class Example
    {
      [SLJsonMember("IntegerArray", SLJsonMemberType.ValueArray)]
      public int[] IntegerArray;

      [SLJsonMember("StringValue")]
      public string StringValue;

      public string NotSerializedString;
    }
  }

  //--------------------------------------------------------------------------
}