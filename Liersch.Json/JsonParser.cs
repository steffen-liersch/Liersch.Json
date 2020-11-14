/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

namespace Liersch.Json
{
  public sealed class JsonParser
  {
    public bool AreSingleQuotesAllowed { get; set; }

    public bool AreUnquotedNamesAllowed { get; set; }

    public bool IsNumericCheckDisabled { get; set; }


    public static JsonNode Parse(string jsonExpression)
    {
      return m_Parser.ParseObject(jsonExpression);
    }

    public static JsonNode Parse(string jsonExpression, bool allowArraysAndValues)
    {
      return allowArraysAndValues ? m_Parser.ParseAny(jsonExpression) : m_Parser.ParseObject(jsonExpression);
    }


    public JsonNode ParseAny(string jsonExpression) { return ParseRoot(jsonExpression, true); }

    public JsonNode ParseObject(string jsonExpression) { return ParseRoot(jsonExpression, false); }


    JsonNode ParseRoot(string jsonExpression, bool allowArraysAndValues)
    {
      var tokenizer=new JsonTokenizer(jsonExpression);
      tokenizer.AreSingleQuotesEnabled=AreSingleQuotesAllowed;
      try
      {
        tokenizer.ReadNext();

        JsonNode res;
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
      catch(JsonException e)
      {
        throw new JsonException(
          "Syntax error in JSON expression at row "+
          JsonConvert.ToString(tokenizer.CurrentRow)+" in column "+
          JsonConvert.ToString(tokenizer.CurrentColumn)+": "+
          e.Message,
          e);
      }
    }

    JsonNode ParseObject(JsonTokenizer tokenizer)
    {
      var res=new JsonNode();
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
            throw new JsonException("Separator expected");

          tokenizer.ReadNext();
        }

        if(tokenizer.HasSpecialChar)
          throw new JsonException("Unexpected token");

        if(!AreUnquotedNamesAllowed && !tokenizer.TokenIsString)
          throw new JsonException("String expected");

        string field=tokenizer.Token;

        tokenizer.ReadNext();
        if(tokenizer.SpecialChar!=':')
          throw new JsonException("Colon expected");

        tokenizer.ReadNext();

        JsonNode value=ParseValue(tokenizer);

        res.m_Object[field]=value; // No exception for multiple fields with the same name
        value.AssignParent(res);

        needSep=true;
      }
    }

    JsonNode ParseArray(JsonTokenizer tokenizer)
    {
      var res=new JsonNode();
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
            throw new JsonException("Separator expected");

          tokenizer.ReadNext();
        }

        JsonNode value=ParseValue(tokenizer);

        res.m_Array.Add(value);
        value.AssignParent(res);

        needSep=true;
      }
    }

    JsonNode ParseValue(JsonTokenizer tokenizer)
    {
      if(tokenizer.HasSpecialChar)
      {
        switch(tokenizer.SpecialChar)
        {
          case '{': return ParseObject(tokenizer);
          case '[': return ParseArray(tokenizer);
          default: throw new JsonException("Unexpected token: "+tokenizer.SpecialChar.ToString());
        }
      }

      if(tokenizer.TokenIsString)
        return new JsonNode(JsonNodeType.String, tokenizer.Token);

      string t=tokenizer.Token;
      switch(t)
      {
        case "null":
          return new JsonNode(JsonNodeType.Null, null);

        case "false":
        case "true":
          return new JsonNode(JsonNodeType.Boolean, t);

        default:
          if(!IsNumericCheckDisabled)
          {
            double d;
            if(!JsonConvert.TryParse(t, out d))
              throw new JsonException("Numeric value expected");

            t=JsonConvert.ToString(d); // Normalize numeric value
          }

          return new JsonNode(JsonNodeType.Number, t);
      }
    }


    static void CheckEndOfExpression(JsonTokenizer tokenizer)
    {
      if(tokenizer.TryReadNext())
        ThrowUnexpected(tokenizer);
    }

    static void ThrowUnexpected(JsonTokenizer tokenizer)
    {
      if(tokenizer.HasSpecialChar)
        throw new JsonException("Unexpected special character: "+tokenizer.SpecialChar.ToString());

      throw new JsonException((tokenizer.TokenIsString ? "Unexpected string: " : "Unexpected token: ")+tokenizer.Token);
    }


    static readonly JsonParser m_Parser=new JsonParser();
  }
}