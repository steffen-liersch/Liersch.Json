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
using System.Text;

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  public sealed class SLJsonTokenizer
  {
    public int CurrentColumn { get { return m_PosInfo.CurrentColumn; } }
    public int CurrentRow { get { return m_PosInfo.CurrentRow; } }

    public bool HasSpecialChar { get { return m_Token==null; } }
    public char SpecialChar { get { return m_SpecialChar; } }

    public string Token { get { return m_Token; } }
    public bool TokenIsString { get { return m_TokenIsString; } }

    //------------------------------------------------------------------------

    public SLJsonTokenizer(string jsonExpression)
    {
      if(jsonExpression==null)
        throw new ArgumentNullException("jsonExpression");

      m_JsonExpression=jsonExpression;
      m_Length=jsonExpression.Length;
    }

    public override string ToString() { return m_PosInfo.ToString(); }

    public void ReadNext()
    {
      if(!TryReadNext())
        throw new SLJsonException("Unexpected end of JSON");
    }

    public bool TryReadNext()
    {
      m_SpecialChar='\0';
      m_Token=null;
      m_TokenIsString=false;

      // Skip whitespace
      char c;
      while(true)
      {
        if(m_Index>=m_Length)
        {
          m_PosInfo=m_PosWork;
          return false; // End
        }

        c=m_JsonExpression[m_Index++];

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
        m_SpecialChar=c;
        return true;
      }

      // Read string or token
      if(c=='"' || c=='\'')
        ReadString(c);
      else ReadToken();
      return true;
    }

    //------------------------------------------------------------------------

    void ReadToken()
    {
      int startIndex=m_Index-1;
      while(true)
      {
        if(m_Index>=m_Length)
        {
          m_Token=m_JsonExpression.Substring(startIndex);
          return; // End
        }

        char c=m_JsonExpression[m_Index];

        if(IsWhiteSpace(c) || c_SpecialChars.IndexOf(c)>=0)
        {
          m_Token=m_JsonExpression.Substring(startIndex, m_Index-startIndex);
          return;
        }

        m_Index++;
        m_PosWork.Update(c);
      }
    }

    void ReadString(char quote)
    {
      StringBuilder sb=null;
      int startIndex=m_Index;
      while(true)
      {
        if(m_Index>=m_Length)
          throw new SLJsonException("Unterminated string expression");

        char c=m_JsonExpression[m_Index++];
        m_PosWork.Update(c);

        if(c==quote)
        {
          if(sb!=null)
            m_Token=sb.ToString();
          else
          {
            int len=m_Index-startIndex-1;
            m_Token=len!=0 ? m_JsonExpression.Substring(startIndex, len) : string.Empty;
          }
          m_TokenIsString=true;
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
              throw new SLJsonException("Unterminated string expression");

            if(sb==null)
            {
              sb=m_Sb;
              sb.Length=0;
              sb.Append(m_JsonExpression, startIndex, m_Index-startIndex-1);
            }

            AppendEscapeSequence();
            break;
        }
      }
    }

    void AppendEscapeSequence()
    {
      char c=m_JsonExpression[m_Index];

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
          throw new SLJsonException("Unsupported escape sequence: \\"+c.ToString());
      }

      m_PosWork.Update(c);
    }

    void AppendFromOctal()
    {
      int len=1;
      while(m_Index+len<m_Length && len<3)
      {
        char c=m_JsonExpression[m_Index+len];
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
        throw new SLJsonException("Unexpected end of escape sequence");
      }

      AppendFromAny(16, 4, m_Index-2); // \u0000..\uFFFF
    }

    void AppendFromAny(int numericBase, int length, int startIndex)
    {
      // Convert.ToInt32(m_JsonExpression.Substring(m_Index, length), numericBase) can't
      // be used here due to a ArgumentException is thrown in .NET MF if numericBase is 8.
      int z=0;
      for(int i=length; i>0; i--)
      {
        char c=m_JsonExpression[m_Index++];

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
          throw new SLJsonException("Invalid escape sequence: "+m_JsonExpression.Substring(startIndex, m_Index-startIndex));
        }

        m_PosWork.Update(c);

        z*=numericBase;
        z+=v;
      }

      m_Sb.Append((char)z);
    }

    static bool IsOctalDigit(char c) { return c>='0' && c<='7'; }

    static bool IsWhiteSpace(char c) { return c==' ' || c>='\t' && c<='\r' || c=='\x00a0' ||c=='\x0085'; }

    //------------------------------------------------------------------------

    struct SLPosition
    {
      public int CurrentColumn { get { return m_X+1; } }
      public int CurrentRow { get { return m_Y+1; } }

      public override string ToString()
      {
        return "Row: "+SLJsonConvert.ToString(CurrentRow)+
          "; Column: "+SLJsonConvert.ToString(CurrentColumn);
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

    //------------------------------------------------------------------------

    const string c_SpecialChars="{}[]:,";
    readonly string m_JsonExpression;
    readonly int m_Length;
    int m_Index;
    SLPosition m_PosWork;
    SLPosition m_PosInfo;
    char m_SpecialChar;
    string m_Token;
    bool m_TokenIsString;
    StringBuilder m_Sb=new StringBuilder();
  }

  //--------------------------------------------------------------------------
}