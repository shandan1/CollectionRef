/*
    This file belong to CollectionRef project,link address [https://github.com/ColinGao225/CollectionRef]
    Written by Colin Gao at 16th/5/2019  
    This project is forked from Microsoft source codes [https://github.com/dotnet/corefx]
    These have a MIT Lisence.That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 
 */
using System;
using System.Collections.Generic;

namespace Colin
{
    public static class CSortHelper
    {
        public static void MSort<T>(this ISortable<T> aSet,Comparison<T> aComparison)
        {
            aSet.Sort(new CComparer<T>(aComparison));

        }
        public static CComparer<T> MToComparer<T>(this Comparison<T> aComparison)
        {
            return new CComparer<T>(aComparison);
        }

        public static CComparerFun<T> MToComparer<T>(this Func<T, T, int> aComparison)
        {
            return new CComparerFun<T>(aComparison);
        }

        public class CComparerFun<T>:IComparer<T>
        {
            Func<T,T,int> m_Comparer;
            public CComparerFun(Func<T, T, int> aComparer)
            {
                m_Comparer = aComparer;
            }
            public int Compare(T x, T y)
            {
                return m_Comparer.Invoke(x, y);
            }
        }
        public class CComparer<T> : IComparer<T>
        {
            Comparison<T> m_Comparer;
            public CComparer(Comparison<T> aComparer)
            {
                m_Comparer = aComparer;
            }
            public int Compare(T x, T y)
            {
                return m_Comparer.Invoke(x, y);
            }
        }
    }


    /// <summary>
    /// Collection can be sorted.
    /// </summary>
    public interface ISortable<T>
    {
        void Sort(IComparer<T> aComparer);
    }
}
