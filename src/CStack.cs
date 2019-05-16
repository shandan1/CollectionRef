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
    /// last-in first-out stack
    /// </summary>
    public class CStack<T> : IReadOnlyCollection<T>, IEnumerableRef<T>
    {
        protected T[] m_Array;
        protected int m_Count;
        public CStack() : this(4)
        {

        }
        public CStack(int aCapacity)
        {
            m_Array = new T[aCapacity];
        }

        private void CheckSize(int aCount)
        {
            if (m_Count + aCount > m_Array.Length)
            {
                var aIncrement = (m_Count + aCount) >> 1;
                var aNewArr = new T[m_Count + (aIncrement > 8 ? aIncrement : 8)];
                Array.Copy(m_Array, 0, aNewArr, 0, m_Count);
                m_Array = aNewArr;
            }
        }

        public T[] GetValueArray()
        {
            return m_Array;
        }

        public int Count { get { return m_Count; } }

        public bool IsReadOnly { get { return false; } }

        public T Peek()
        {
            if (m_Count < 1)
                throw new InvalidOperationException("Stack is empty.");

            return m_Array[m_Count - 1];
        }
        public bool TryPeek(out T aResult)
        {
            if (m_Count < 1)
            {
                aResult = default;
                return false;
            }
            aResult = m_Array[m_Count - 1];
            return true;
        }

        public T Pop()
        {
            if (m_Count < 1)
                throw new InvalidOperationException("Stack is empty.");
            --m_Count;
            T aData = m_Array[m_Count];
            m_Array[m_Count] = default;
            return aData;
        }

        public bool TryPop(out T aResult)
        {
            if (m_Count < 1)
            {
                aResult = default;
                return false;
            }
            --m_Count;
            aResult = m_Array[m_Count];
            m_Array[m_Count] = default;
            return true;
        }

        public void Push(T aItem)
        {
            CheckSize(1);
            m_Array[m_Count++] = aItem;
        }
        public void PushRange<U>(U aItems)where U : IReadOnlyCollection<T>
        {
            CheckSize(aItems.Count);
            foreach (var aItem in aItems)
            {
                m_Array[m_Count++] = aItem;
            }
        }

        public void PushRange(T[] aArr, int aIndex, int aCount)
        {
            CheckSize(aCount);
            int aEnd = aIndex + aCount;
            for (int i = aIndex; i < aEnd; ++i)
            {
                m_Array[m_Count++] = aArr[i];
            }
        }

        public void Add(T item)
        {
            Push(item);
        }

        public void Clear()
        {
            CArray.Clear(m_Array, 0, m_Count);
            m_Count = 0;
        }

        public bool Contains(T item)
        {
            return m_Count != 0 && Array.LastIndexOf(m_Array, item, m_Count - 1) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(m_Array, 0, array, arrayIndex, m_Count);
        }

        public VEnumerator GetEnumerator()
        {
            return new VEnumerator(this);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new VEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
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
            public VEnumerator(CStack<T> aThis)
            {
                m_This = aThis;
                m_Pos = -1;
            }

            public CStack<T> m_This;
            public int m_Pos;

            public T Current { get { return m_This.m_Array[m_Pos]; } }

            object IEnumerator.Current { get { return m_This.m_Array[m_Pos]; } }

            public ref T CurrentRef { get { return ref m_This.m_Array[m_Pos]; } }

            public void Dispose()
            {
                m_This = null;
                m_Pos = -1;
            }
            public void Reset()
            {
                m_Pos = -1;
            }

            public bool MoveNext()
            {
                if (m_Pos + 1 < m_This.m_Count)
                {
                    ++m_Pos;
                    return true;
                }

                return false;
            }
        }
    }
}
