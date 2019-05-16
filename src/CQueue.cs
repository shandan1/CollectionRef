/*
    This file belong to CollectionRef project,link address [https://github.com/ColinGao225/CollectionRef]
    Written by Colin Gao at 16th/5/2019  
    This project is forked from Microsoft source codes [https://github.com/dotnet/corefx]
    These have a MIT Lisence.That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace Colin.CollectionRef
{
    /// <summary>
    /// first-in first-out queue
    /// </summary>
    public class CQueue<T> : IReadOnlyCollection<T>, IEnumerableRef<T>
    {
        protected T[] m_Array;
        protected int m_Count;
        protected int m_Head;
        public CQueue()
        {

        }
        public CQueue(int aCapacity)
        {
            m_Array = new T[aCapacity];
        }

        public T[] GetValueArrayCompressed()
        {
            Compress();
            return m_Array;
        }

        /// <summary>
        ///  compress,  move data from the head to the index 0.
        /// </summary>
        public void Compress()
        {
            if (m_Head == 0)
                return;

            CArray.Reverse(m_Array, 0, m_Head);
            CArray.Reverse(m_Array, m_Head, m_Array.Length - m_Head);
            CArray.Reverse(m_Array, 0, m_Array.Length);

            m_Head = 0;
        }

        protected void CheckSize(int aCount)
        {
            if (m_Count + aCount >= m_Array.Length)
            {
                aCount = (m_Count + aCount) >> 1;
                var aNewArr = new T[m_Count + (aCount > 8 ? aCount : 8)];
                aCount = m_Array.Length - m_Head;
                if (m_Count > aCount)
                {
                    Array.Copy(m_Array, m_Head, aNewArr, 0, aCount);
                    Array.Copy(m_Array, 0, aNewArr, aCount, m_Count - aCount);
                }
                else
                    Array.Copy(m_Array, m_Head, aNewArr, 0, m_Count);

                m_Array = aNewArr;
                m_Head = 0;
            }
        }

        /// <summary>
        /// Get value at the index,may throw out of range exception.
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public T GetValueAt(int aIndex)
        {
            var i = m_Head + aIndex;
            var aLen = m_Array.Length;
            if (i >= aLen)
                i -= aLen;

            return m_Array[i];
        }

        /// <summary>
        /// Get value refferance at the index,may throw out of range exception.
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public ref T GetValueRefAt(int aIndex)
        {
            var i = m_Head + aIndex;
            var aLen = m_Array.Length;
            if (i >= aLen)
                i -= aLen;

            return ref m_Array[i];
        }
        /// <summary>
        /// Set value at the index ,may cause memory leak.
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public void SetValueAt(int aIndex,T aValue)
        {
            var i = m_Head + aIndex;
            var aLen = m_Array.Length;
            if (i >= aLen)
                i -= aLen;

            m_Array[i] = aValue;
        }

        public void Clear()
        {
            if (m_Count != 0)
            {
                int aCount = m_Array.Length - m_Head;
                if (m_Count > aCount)
                {
                    CArray.Clear(m_Array, m_Head, aCount);
                    CArray.Clear(m_Array, 0, m_Count - aCount);
                }
                else
                    CArray.Clear(m_Array, m_Head, m_Count);
            }

            m_Head = 0;
            m_Count = 0;
        }

        public void CopyTo(T[] aArr, int aArrIndex)
        {
            if (m_Count == 0)
                return;

            int aCount = m_Array.Length - m_Head;
            if (m_Count > aCount)
            {
                Array.Copy(m_Array, m_Head, aArr, aArrIndex, aCount);
                Array.Copy(m_Array, 0, aArr, aArrIndex + aCount, m_Count - aCount);
            }
            else
                Array.Copy(m_Array, 0, aArr, aArrIndex, m_Count);
        }
        /// <summary>
        /// Add item to the tail of the queue
        /// </summary>
        /// <param name="aItem"></param>
        public void AddLast(T aItem)
        {
            CheckSize(1);

            int aEnd = m_Head + m_Count;
            int aLen = m_Array.Length;
            if (aEnd >= aLen)
                aEnd -= aLen;
           
            m_Array[aEnd] = aItem;
            ++m_Count;
        }

        /// <summary>
        /// remove item from the head of the queue,and return it.throw exception when empty
        /// </summary>
        /// <param name="aItem"></param>
        public T RemoveFirst()
        {
            if (m_Count == 0)
                throw new InvalidOperationException("is empty");

            var aItem = m_Array[m_Head];
            m_Array[m_Head] = default;
            ++m_Head;
            if (m_Head == m_Array.Length)
                m_Head = 0;

            --m_Count;
            return aItem;
        }
        /// <summary>
        /// remove item from the head of the queue,and return it. return false when empty.
        /// </summary>
        /// <param name="aItem"></param>
        public bool TryRemoveFirst(out T aResult)
        {
            if (m_Count == 0)
            {
                aResult = default;
                return false;
            }

            aResult = m_Array[m_Head];
            m_Array[m_Head] = default;
            ++m_Head;
            if (m_Head == m_Array.Length)
                m_Head = 0;

            --m_Count;
            return true;
        }

        /// <summary>
        /// Return the item at the head of the queue,but not remove it.throw exception when empty
        /// </summary>
        /// <returns></returns>
        public T PeekFirst()
        {
            if (m_Count == 0)
                throw new InvalidOperationException("is empty");

            return m_Array[m_Head];
        }
        /// <summary>
        /// Return the item at the head of the queue,but not remove it.return false when empty.
        /// </summary>
        /// <returns></returns>
        public bool TryPeekFirst(out T aResult)
        {
            if (m_Count == 0)
            {
                aResult = default;
                return false;
            }

            aResult = m_Array[m_Head];
            return true;
        }

        public bool Contains(T aItem)
        {
            if (m_Count == 0)
                return false;

            int aCount = m_Array.Length-m_Head;
            if (m_Count > aCount)
            {
                return
                     Array.IndexOf(m_Array, aItem, m_Head, aCount) > -1 ||
                     Array.IndexOf(m_Array, aItem, 0, m_Count - aCount) > -1;
            }
            else
                return Array.IndexOf(m_Array, aItem, m_Head, m_Count) > -1;

        }

        public int Count { get { return m_Count; } }

        public VEnumerator GetEnumerator()
        {
            return new VEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new VEnumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new VEnumerator(this);
        }

        public VEnumerator GetEnumeratorRef()
        {
            return new VEnumerator(this);
        }
        IEnumeratorRef<T> IEnumerableRef<T>.GetEnumeratorRef()
        {
            return new VEnumerator(this);
        }
        public struct VEnumerator : IEnumerator<T>, IEnumeratorRef<T>
        {
            public VEnumerator(CQueue<T> aThis)
            {
                m_This = aThis;
                m_Pos = aThis.m_Head-1;
            }

            public CQueue<T> m_This;
            public int m_Pos;

            public T Current { get { return m_This.m_Array[m_Pos]; } }

            object IEnumerator.Current { get { return m_This.m_Array[m_Pos]; } }

            public ref T CurrentRef { get { return ref m_This.m_Array[m_Pos]; } }

            public void Dispose()
            {
                m_This = null;
                m_Pos = - 1;
            }
            public void Reset()
            {
                m_Pos = m_This.m_Head - 1;
            }

            public bool MoveNext()
            {
                int aLen = m_This.m_Array.Length;
                int aPos = m_Pos + 1;
                if (aPos < m_This.m_Head)
                    aPos += aLen;

                if (aPos < m_This.m_Head + m_This.m_Count)
                {
                    if (aPos >= aLen)
                        aPos -= aLen;

                    m_Pos = aPos;
                    return true;
                }
                return false;
            }
        }
    }
}
