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

#if !NETMF
using System.Globalization;
using System.Linq.Expressions;
#endif

namespace Liersch.Json
{
  //--------------------------------------------------------------------------

  sealed class UnitTestHelper
  {
    public void Reset()
    {
      m_CountSucceeded=0;
      m_CountFailed=0;
    }

    public void PrintHeadline(string headline)
    {
      Console.WriteLine(headline);
      Console.WriteLine(new StringBuilder().Append('=', headline.Length).ToString());
      Console.WriteLine();
    }

    public void PrintSummary()
    {
      Console.WriteLine("Succeeded Tests: "+m_CountSucceeded);
      Console.WriteLine("Failed Tests: "+m_CountFailed);
      Console.WriteLine();
    }

    //------------------------------------------------------------------------

#if NETMF

    public delegate bool SLUnitTestFunction();

    public void Assert(SLUnitTestFunction func)
    {
      bool res=func();
      if(res)
        m_CountSucceeded++;
      else
      {
        m_CountFailed++;
        Console.WriteLine("[FAILED]");
      }
    }

#else

    public void Assert(Expression<Func<bool>> expression)
    {
      Func<bool> func=expression.Compile();
      bool res=func();
      if(res)
        m_CountSucceeded++;
      else
      {
        m_CountFailed++;
        Console.WriteLine("[FAILED] "+expression.Body.ToString());
      }
    }

#endif

    //------------------------------------------------------------------------

    int m_CountSucceeded;
    int m_CountFailed;
  }

  //--------------------------------------------------------------------------
}