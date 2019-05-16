/*
    This file belong to CollectionRef project,link address [https://github.com/ColinGao225/CollectionRef]
    Written by Colin Gao at 16th/5/2019  
    This project is forked from Microsoft source codes [https://github.com/dotnet/corefx]
    These have a MIT Lisence.That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 
 */
using Colin.CollectionRef;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Colin
{
    public static class CArray
    {
        public static int BinarySearch<T>(T[] aArr, int aIndex, int aLen, T aValue)
        {
            return BinarySearch(aArr, aIndex, aLen, aValue, Comparer<T>.Default);
        }
        public static int BinarySearch<T>(T[] aArr, int aIndex, int aLen, T aValue, IComparer<T> aComparer)
        {
            int low = aIndex;
            int high = aIndex + aLen - 1;
            int mid;
            int compare;
            while (low <= high)
            {
                mid = low + ((high - low) >> 1); //middle
                compare = aComparer.Compare(aArr[mid], aValue);
                if (compare == 0)
                    return mid;
                if (compare < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }
            return ~low;
        }

        public static void Sort<T>(T[] aArr, int aIndex, int aCount)
        {
            QuickSort(aArr, aIndex, aIndex + aCount - 1, Comparer<T>.Default);
        }

        public static void Sort<T>(T[] aArr, int aIndex, int aCount, IComparer<T> aComparer)
        {
            QuickSort(aArr, aIndex, aIndex + aCount - 1, aComparer);
        }

        public static void Clear<T>(T[] aArr, int aIndex, int aCount)
        {
            int aEnd = aIndex + aCount;
            for (int i = aIndex; i < aEnd; ++i)
            {
                aArr[i] = default;
            }


        }
        public static int FindIndex<T>(T[] aArr, int aIndex, int aLen, Predicate<T> aValue)
        {
            int aEnd = aIndex + aLen;
            for (int i = 0; i < aEnd; ++i)
            {
                if (aValue.Invoke(aArr[i]))
                    return i;
            }
            return -1;
        }
        public static int FindLastIndex<T>(T[] aArr, int aIndex, int aLen, Predicate<T> aValue)
        {
            for (int i = aIndex + aLen - 1; i > -1; --i)
            {
                if (aValue.Invoke(aArr[i]))
                    return i;
            }
            return -1;
        }

        public static void Fill<T>(T[] aArr, int aIndex, int aLen, T aValue)
        {
            for (int i = aIndex + aLen - 1; i > -1; --i)
            {
                aArr[i] = aValue;
            }
        }

        public static void ForEach<T>(T[] aArr, int aIndex, int aLen, Action<T> aAction)
        {
            for (int i = aIndex + aLen - 1; i > -1; --i)
            {
                aAction.Invoke(aArr[i]);
            }
        }
        public static void Reverse<T>(T[] aArr, int aIndex, int aLen)
        {
            int i = aIndex;
            int j = aIndex + aLen - 1;
            while (i < j)
            {
                Swap(ref aArr[i], ref aArr[j]);
                ++i;
                --j;
            }
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }

        /// <summary>
        /// Quick sort/Insert sort
        /// </summary>
        private static void QuickSort<T>(T[] aArr, int low, int high, IComparer<T> aComparer)
        {
            int i;
            while (low < high)
            {
                i = high - low;
                if (i < 17)
                {
                    if (i == 0)
                        return;
                    if (i == 1)
                    {
                        SwapIfGreater(ref aArr[low], ref aArr[high], aComparer);
                        return;
                    }
                    if (i == 2)
                    {
                        SwapIfGreater(ref aArr[low], ref aArr[low + 1], aComparer);
                        SwapIfGreater(ref aArr[low], ref aArr[high], aComparer);
                        SwapIfGreater(ref aArr[low + 1], ref aArr[high], aComparer);
                        return;
                    }

                    InsertSort(aArr, low, high, aComparer);
                    return;
                }
                i = low + ((high - low) >> 1); ; //middle
                SwapIfGreater(ref aArr[low], ref aArr[i], aComparer);
                SwapIfGreater(ref aArr[low], ref aArr[high], aComparer);
                SwapIfGreater(ref aArr[i], ref aArr[high], aComparer);
                T pivot = aArr[i];
                int j = high - 1; //
                Swap(ref aArr[i], ref aArr[j]);
                i = low + 1; //
                while (i < j)
                {
                    if (aComparer.Compare(aArr[i], pivot) > 0)
                    {
                        --j;
                        if (i == j)
                            break;

                        Swap(ref aArr[i], ref aArr[j]);
                    }
                    else
                        ++i;
                }
                if (i != high - 1)
                    Swap(ref aArr[i], ref aArr[high - 1]);

                QuickSort(aArr, i + 1, high, aComparer);
                high = i - 1;
            }
        }
        private static void InsertSort<T>(T[] aArr, int low, int high, IComparer<T> aComparer)
        {
            int i, j;
            T t;
            for (i = low; i < high; ++i)
            {
                j = i;
                t = aArr[i + 1];
                while (j >= low && aComparer.Compare(aArr[j], t) > 0)
                {
                    aArr[j + 1] = aArr[j];
                    --j;
                }
                aArr[j + 1] = t;
            }
        }

        private static void SwapIfGreater<T>(ref T a, ref T b, IComparer<T> aComparer)
        {
            if (aComparer.Compare(a, b) > 0)
            {
                T c = a;
                a = b;
                b = c;
            }
        }

        public static VArrayCollection<T> MToCollection<T>(this T[] aArr, int aIndex, int aCount)
        {
            return new VArrayCollection<T>(aArr, aIndex, aCount);
        }

        public static VArrayCollection<T> MToCollection<T>(this T[] aArr)
        {
            return new VArrayCollection<T>(aArr, 0, aArr.Length);
        }
    }

    /// <summary>
    /// wrapper a array like a collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct VArrayCollection<T> :
        IReadOnlyCollection<T>, IEnumerator<T>, IEnumerableRef<T>, IEnumeratorRef<T>
    {
        public T[] m_Array;
        public int m_End;
        public int m_Count;
        private int m_Pos;

        public VArrayCollection(T[] aArray, int aIndex, int aCount)
        {
            m_Array = aArray;
            m_End = aIndex + aCount;
            m_Count = aCount;
            m_Pos = aIndex - 1;
        }

        public int Count { get { return m_Count; } }

        public void Dispose()
        {
            m_Array = null;
            m_Pos = -1;
        }

        public VArrayCollection<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this;
        }

        public VArrayCollection<T> GetEnumeratorRef()
        {
            return this;
        }

        IEnumeratorRef<T> IEnumerableRef<T>.GetEnumeratorRef()
        {
            return this;
        }
        public bool MoveNext()
        {
            if (m_Pos + 1 < m_End)
            {
                ++m_Pos;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            m_Pos = m_End - m_Count - 1;
        }

        public T Current { get { return m_Array[m_Pos]; } }

        object IEnumerator.Current { get { return m_Array[m_Pos]; } }

        public ref T CurrentRef { get { return ref m_Array[m_Pos]; } }
    }

}
