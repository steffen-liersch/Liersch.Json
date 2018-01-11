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
    public int CurrentColumn { get { return m_PosInfo.X+1; } }
    public int CurrentRow { get { return m_PosInfo.Y+1; } }

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

    public override string ToString()
    {
      return "Row: "+SLJsonConvert.ToString(CurrentRow)+
        "; Column: "+SLJsonConvert.ToString(CurrentColumn);
    }

    public void ReadNext()
    {
      if(!TryReadNext())
        throw new SLJsonException("Unexpected end of stream");
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
          return false; // End

        c=m_JsonExpression[m_Index++];

        if(!IsWhiteSpace(c))
        {
          m_PosInfo=m_PosWork;
          UpdateWorkPosition(c);
          break;
        }

        UpdateWorkPosition(c);
      }


      // Special symbol?
      if(c_SpecialChars.IndexOf(c)>=0)
      {
        m_SpecialChar=c;
        return true;
      }


      if(c=='"' || c=='\'')
      {
        char quote=c;

        // Read string
        StringBuilder sb=null;
        int startIndex=m_Index;
        while(true)
        {
          if(m_Index>=m_Length)
            throw new SLJsonException("Unterminated string expression");

          c=m_JsonExpression[m_Index++];
          UpdateWorkPosition(c);

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
            return true;
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

              c=m_JsonExpression[m_Index++];
              UpdateWorkPosition(c);

              switch(c)
              {
                case '"': sb.Append('"'); break;
                case '\\': sb.Append('\\'); break;
                case '/': sb.Append('/'); break;
                case 'b': sb.Append('\b'); break;
                case 't': sb.Append('\t'); break;
                case 'n': sb.Append('\n'); break;
                case 'f': sb.Append('\f'); break;
                case 'r': sb.Append('\r'); break;
                case 'u':
                  SLPosition lastPI=m_PosInfo;
                  m_PosInfo=m_PosWork; // Update position information at first for the case of a format exception.

                  if(m_Index+4>m_Length)
                    throw new SLJsonException("Unexpected end of escape sequence");

                  string s=m_JsonExpression.Substring(m_Index, 4);
                  int i=Convert.ToInt32(s, 16);
                  sb.Append((char)i);

                  UpdateWorkPosition(m_JsonExpression[m_Index++]);
                  UpdateWorkPosition(m_JsonExpression[m_Index++]);
                  UpdateWorkPosition(m_JsonExpression[m_Index++]);
                  UpdateWorkPosition(m_JsonExpression[m_Index++]);

                  m_PosInfo=lastPI; // Restore position information.
                  break;

                default:
                  m_PosInfo=m_PosWork;
                  throw new SLJsonException("Unsupported escape sequence ("+c.ToString()+")");
              }
              break;
          }
        }
      }
      else
      {
        // Read token
        int startIndex=m_Index-1;
        while(true)
        {
          if(m_Index>=m_Length)
          {
            m_Token=m_JsonExpression.Substring(startIndex);
            return true; // End
          }

          c=m_JsonExpression[m_Index];

          if(IsWhiteSpace(c) || c_SpecialChars.IndexOf(c)>=0)
          {
            m_Token=m_JsonExpression.Substring(startIndex, m_Index-startIndex);
            return true;
          }

          m_Index++;
          UpdateWorkPosition(c);
        }
      }
    }

    static bool IsWhiteSpace(char c) { return c==' ' || c>='\t' && c<='\r' || c=='\x00a0' ||c=='\x0085'; }

    //------------------------------------------------------------------------

    void UpdateWorkPosition(char c)
    {
      if(c!='\n')
        m_PosWork.X++;
      else
      {
        m_PosWork.X=0;
        m_PosWork.Y++;
      }
    }

    struct SLPosition
    {
      public int X;
      public int Y;
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