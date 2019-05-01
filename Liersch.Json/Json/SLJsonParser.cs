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
  public sealed class SLJsonParser
  {
    public bool AreSingleQuotesAllowed { get; set; }

    public bool AreUnquotedNamesAllowed { get; set; }

    public bool IsNumericCheckDisabled { get; set; }


    public static SLJsonNode Parse(string jsonExpression)
    {
      return m_Parser.ParseObject(jsonExpression);
    }

    public static SLJsonNode Parse(string jsonExpression, bool allowArraysAndValues)
    {
      return allowArraysAndValues ? m_Parser.ParseAny(jsonExpression) : m_Parser.ParseObject(jsonExpression);
    }


    public SLJsonNode ParseAny(string jsonExpression) { return ParseRoot(jsonExpression, true); }

    public SLJsonNode ParseObject(string jsonExpression) { return ParseRoot(jsonExpression, false); }


    SLJsonNode ParseRoot(string jsonExpression, bool allowArraysAndValues)
    {
      var tokenizer=new SLJsonTokenizer(jsonExpression);
      tokenizer.AreSingleQuotesEnabled=AreSingleQuotesAllowed;
      try
      {
        tokenizer.ReadNext();

        SLJsonNode res;
        if(allowArraysAndValues)
          res=ParseValue(tokenizer);
        else
        {
          if(!tokenizer.HasSpecialChar || tokenizer.SpecialChar!='{')
            ThrowUnexpected(tokenizer);

          res=ParseObject(tokenizer);
        }

        CheckEndOfExpression(tokenizer);
        return res;
      }
      catch(SLJsonException e)
      {
        throw new SLJsonException(
          "Syntax error in JSON expression at row "+
          SLJsonConvert.ToString(tokenizer.CurrentRow)+" in column "+
          SLJsonConvert.ToString(tokenizer.CurrentColumn)+": "+
          e.Message,
          e);
      }
    }

    SLJsonNode ParseObject(SLJsonTokenizer tokenizer)
    {
      var res=new SLJsonNode();
      res.MakeObject();
      bool needSep=false;
      while(true)
      {
        tokenizer.ReadNext();

        if(tokenizer.SpecialChar=='}')
          return res;

        if(needSep)
        {
          if(tokenizer.SpecialChar!=',')
            throw new SLJsonException("Separator expected");

          tokenizer.ReadNext();
        }

        if(tokenizer.HasSpecialChar)
          throw new SLJsonException("Unexpected token");

        if(!AreUnquotedNamesAllowed && !tokenizer.TokenIsString)
          throw new SLJsonException("String expected");

        string field=tokenizer.Token;

        tokenizer.ReadNext();
        if(tokenizer.SpecialChar!=':')
          throw new SLJsonException("Colon expected");

        tokenizer.ReadNext();

        SLJsonNode value=ParseValue(tokenizer);

        res.m_Object[field]=value; // No exception for multiple fields with the same name
        value.AssignParent(res);

        needSep=true;
      }
    }

    SLJsonNode ParseArray(SLJsonTokenizer tokenizer)
    {
      var res=new SLJsonNode();
      res.MakeArray();
      bool needSep=false;
      while(true)
      {
        tokenizer.ReadNext();

        if(tokenizer.SpecialChar==']')
          return res;

        if(needSep)
        {
          if(tokenizer.SpecialChar!=',')
            throw new SLJsonException("Separator expected");

          tokenizer.ReadNext();
        }

        SLJsonNode value=ParseValue(tokenizer);

        res.m_Array.Add(value);
        value.AssignParent(res);

        needSep=true;
      }
    }

    SLJsonNode ParseValue(SLJsonTokenizer tokenizer)
    {
      if(tokenizer.HasSpecialChar)
      {
        switch(tokenizer.SpecialChar)
        {
          case '{': return ParseObject(tokenizer);
          case '[': return ParseArray(tokenizer);
          default: throw new SLJsonException("Unexpected token: "+tokenizer.SpecialChar.ToString());
        }
      }

      if(tokenizer.TokenIsString)
        return new SLJsonNode(SLJsonNodeType.String, tokenizer.Token);

      string t=tokenizer.Token;
      switch(t)
      {
        case "null":
          return new SLJsonNode(SLJsonNodeType.Null, null);

        case "false":
        case "true":
          return new SLJsonNode(SLJsonNodeType.Boolean, t);

        default:
          if(!IsNumericCheckDisabled)
          {
            double d;
            if(!SLJsonConvert.TryParse(t, out d))
              throw new SLJsonException("Numeric value expected");

            t=SLJsonConvert.ToString(d); // Normalize numeric value
          }

          return new SLJsonNode(SLJsonNodeType.Number, t);
      }
    }


    static void CheckEndOfExpression(SLJsonTokenizer tokenizer)
    {
      if(tokenizer.TryReadNext())
        ThrowUnexpected(tokenizer);
    }

    static void ThrowUnexpected(SLJsonTokenizer tokenizer)
    {
      if(tokenizer.HasSpecialChar)
        throw new SLJsonException("Unexpected special character: "+tokenizer.SpecialChar.ToString());

      throw new SLJsonException((tokenizer.TokenIsString ? "Unexpected string: " : "Unexpected token: ")+tokenizer.Token);
    }


    static readonly SLJsonParser m_Parser=new SLJsonParser();
  }
}