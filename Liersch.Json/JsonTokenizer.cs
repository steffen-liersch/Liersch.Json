/*--------------------------------------------------------------------------*\
::
::  Copyright © 2013-2020 Steffen Liersch
::  https://www.steffen-liersch.de/
::
\*--------------------------------------------------------------------------*/

using System;
using System.Text;

namespace Liersch.Json
{
  public sealed class JsonTokenizer
  {
    public bool AreSingleQuotesEnabled { get; set; }

    public int CurrentColumn { get { return m_PosInfo.CurrentColumn; } }
    public int CurrentRow { get { return m_PosInfo.CurrentRow; } }

    public bool HasSpecialChar { get { return Token==null; } }
    public char SpecialChar { get; private set; }

    public string Token { get; private set; }
    public bool TokenIsString { get; private set; }


    public JsonTokenizer(string json)
    {
      if(json==null)
        throw new ArgumentNullException("json");

      m_Json=json;
      m_Length=json.Length;
    }

    public override string ToString() { return m_PosInfo.ToString(); }


    public void ReadNext()
    {
      if(!TryReadNext())
        throw new JsonException("Unexpected end of JSON");
    }

    public void ReadColon()
    {
      ReadNext();
      if(SpecialChar!=':')
        throw new JsonException("Colon expected");
    }

    public void ReadString()
    {
      ReadNext();
      if(!TokenIsString)
        throw new JsonException("String expected");
    }

    public bool BeginReadArray()
    {
      ReadNext();
      return !HasSpecialChar || SpecialChar!=']';
    }

    public void SkipValue()
    {
      ReadNext();
      SkipValueBody();
    }

    public void SkipValueBody()
    {
      if(HasSpecialChar)
      {
        switch(SpecialChar)
        {
          case '{': SkipObjectProperties(); break;
          case '[': SkipArrayValues(); break;
          default: throw new JsonException("Unexpected token");
        }
      }
    }

    public void SkipObjectProperties()
    {
      ReadNext();

      if(HasSpecialChar)
      {
        switch(SpecialChar)
        {
          case '}': return;
          default: throw new JsonException("Unexpected token");
        }
      }

      while(true)
      {
        if(!TokenIsString)
          throw new JsonException("String expected");

        ReadColon();
        SkipValue();
        ReadNext();

        switch(SpecialChar)
        {
          case ',': ReadNext(); break;
          case '}': return;
          default: throw new JsonException("Unexpected token");
        }
      }
    }

    public void SkipArrayValues()
    {
      ReadNext();

      bool canComma=false;
      while(true)
      {
        if(!HasSpecialChar)
          ReadNext();
        else
        {
          switch(SpecialChar)
          {
            case ',':
              if(!canComma)
                throw new JsonException("Unexpected token");

              SkipValue();
              ReadNext();
              break;

            case '{':
              SkipObjectProperties();
              ReadNext();
              break;

            case '[':
              SkipArrayValues();
              ReadNext();
              break;

            case ']': return;
            default: throw new JsonException("Unexpected token");
          }
        }

        canComma=true;
      }
    }


    public bool TryReadNext()
    {
      SpecialChar='\0';
      Token=null;
      TokenIsString=false;

      // Skip whitespace
      char c;
      while(true)
      {
        if(m_Index>=m_Length)
        {
          m_PosInfo=m_PosWork;
          return false; // End
        }

        c=m_Json[m_Index++];

        if(!IsWhiteSpace(c))
        {
          m_PosInfo=m_PosWork;
          m_PosWork.Update(c);
          break;
        }

        m_PosWork.Update(c);
      }

      // Special symbol?
      if(c_SpecialChars.IndexOf(c)>=0)
      {
        SpecialChar=c;
        return true;
      }

      // Read string or token
      if(c=='"' || AreSingleQuotesEnabled && c=='\'')
        ReadStringToken(c);
      else ReadToken();
      return true;
    }


    void ReadToken()
    {
      int startIndex=m_Index-1;
      while(true)
      {
        if(m_Index>=m_Length)
        {
          Token=m_Json.Substring(startIndex);
          return; // End
        }

        char c=m_Json[m_Index];

        if(IsWhiteSpace(c) || c_SpecialChars.IndexOf(c)>=0)
        {
          Token=m_Json.Substring(startIndex, m_Index-startIndex);
          return;
        }

        m_Index++;
        m_PosWork.Update(c);
      }
    }

    void ReadStringToken(char quotation)
    {
      StringBuilder sb=null;
      int startIndex=m_Index;
      while(true)
      {
        if(m_Index>=m_Length)
          throw new JsonException("Unterminated string expression");

        char c=m_Json[m_Index++];
        m_PosWork.Update(c);

        if(c==quotation)
        {
          if(sb!=null)
          {
            Token=sb.ToString();
            sb.Length=0;
          }
          else
          {
            int len=m_Index-startIndex-1;
            Token=len!=0 ? m_Json.Substring(startIndex, len) : string.Empty;
          }
          TokenIsString=true;
          return;
        }

        switch(c)
        {
          default:
            if(sb!=null)
              sb.Append(c);
            break;

          case '\\':
            if(m_Index>=m_Length)
              throw new JsonException("Unterminated string expression");

            if(sb==null)
            {
              sb=m_Sb;
              sb.Append(m_Json, startIndex, m_Index-startIndex-1);
            }

            AppendEscapeSequence();
            break;
        }
      }
    }

    void AppendEscapeSequence()
    {
      char c=m_Json[m_Index];

      if(IsOctalDigit(c))
      {
        AppendFromOctal();
        return;
      }

      m_Index++;

      switch(c)
      {
        //case '0': m_Sb.Append('\0'); break; // Null character : \u0000
        case 'b': m_Sb.Append('\b'); break; // Backspace      : \u0008
        case 't': m_Sb.Append('\t'); break; // Horizontal tab : \u0009
        case 'n': m_Sb.Append('\n'); break; // Line feed      : \u000A
        case 'v': m_Sb.Append('\v'); break; // Vertical tab   : \u000B
        case 'f': m_Sb.Append('\f'); break; // Form feed      : \u000C
        case 'r': m_Sb.Append('\r'); break; // Carriage return: \u000D

        case '"':
        case '/':
        case '\'':
        case '\\':
          m_Sb.Append(c);
          break;

        case 'u':
          m_PosWork.Update(c);
          AppendFromHex();
          return; // Exit

        default:
          m_PosInfo=m_PosWork;
          throw new JsonException("Unsupported escape sequence: \\"+c.ToString());
      }

      m_PosWork.Update(c);
    }

    void AppendFromOctal()
    {
      int len=1;
      while(m_Index+len<m_Length && len<3)
      {
        char c=m_Json[m_Index+len];
        if(!IsOctalDigit(c))
          break;
        len++;
      }

      AppendFromAny(8, len, m_Index-1); // \0..\777
    }

    void AppendFromHex()
    {
      if(m_Index+4>m_Length)
      {
        m_PosInfo=m_PosWork;
        throw new JsonException("Unexpected end of escape sequence");
      }

      AppendFromAny(16, 4, m_Index-2); // \u0000..\uFFFF
    }

    void AppendFromAny(int numericBase, int length, int startIndex)
    {
      // Convert.ToInt32(m_Json.Substring(m_Index, length), numericBase) can't
      // be used here due to a ArgumentException is thrown in .NET MF if numericBase is 8.
      int z=0;
      for(int i = length; i>0; i--)
      {
        char c=m_Json[m_Index++];

        int v;
        if(c>='0' && c<='9')
          v=c-'0';
        else if(c>='A' && c<='Z')
          v=c-'A'+10;
        else if(c>='a' && c<='z')
          v=c-'a'+10;
        else v=-1;

        if(v<0 || v>=numericBase)
        {
          m_PosInfo=m_PosWork;
          throw new JsonException("Invalid escape sequence: "+m_Json.Substring(startIndex, m_Index-startIndex));
        }

        m_PosWork.Update(c);

        z*=numericBase;
        z+=v;
      }

      m_Sb.Append((char)z);
    }

    static bool IsOctalDigit(char c) { return c>='0' && c<='7'; }

    static bool IsWhiteSpace(char c) { return c==' ' || c>='\t' && c<='\r' || c=='\x00a0' ||c=='\x0085'; }


    struct Position
    {
      public int CurrentColumn { get { return m_X+1; } }
      public int CurrentRow { get { return m_Y+1; } }

      public override string ToString()
      {
        return "Row: "+JsonConvert.ToString(CurrentRow)+
          "; Column: "+JsonConvert.ToString(CurrentColumn);
      }

      public void Update(char c)
      {
        // Linux  : LF
        // Mac OS : CR
        // Windows: CR + LF
        switch(c)
        {
          case '\n':
            switch(m_Last)
            {
              case '\r': m_Last='\0'; break;
              case '\n': m_Last='\0'; m_Y++; break;
              default: m_Last=c; m_Y++; m_X=0; break;
            }
            break;

          case '\r':
            switch(m_Last)
            {
              case '\n': m_Last='\0'; break;
              case '\r': m_Last='\0'; m_Y++; break;
              default: m_Last=c; m_Y++; m_X=0; break;
            }
            break;

          default: m_Last='\0'; m_X++; break;
        }
      }

      int m_X;
      int m_Y;
      char m_Last;
    }


    const string c_SpecialChars="{}[]:,";
    readonly string m_Json;
    readonly int m_Length;

    int m_Index;
    Position m_PosWork=default(Position);
    Position m_PosInfo;

    readonly StringBuilder m_Sb=new StringBuilder();
  }
}