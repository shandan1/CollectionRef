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
    /// similar to List,but delete action may change the index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CCollection<T> : ICollection<T>, IReadOnlyCollection<T>, IEnumerableRef<T>
    {
        protected T[] m_Array;
        protected int m_Count;

        public CCollection() : this(4)
        {
        }
        public CCollection(int aCapacity)
        {
            m_Array = new T[aCapacity];
            m_Count = 0;
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

        public T this[int index]
        {
            get { return m_Array[index]; }
            set { m_Array[index] = value; }
        }

        public int Count
        {
            get { return m_Count; }
        }

        public bool IsReadOnly { get { return false; } }

        public void Add(T aItem)
        {
            CheckSize(1);
            m_Array[m_Count++] = aItem;
           
        }
        public void AddRange<U>(U aItems)where U: IReadOnlyCollection<T>
        {
            CheckSize(aItems.Count);
            foreach(var aItem in aItems)
            {
                m_Array[m_Count++] = aItem;
            }
        }

        public void Clear()
        {

            CArray.Clear(m_Array, 0, m_Count);
            m_Count = 0;
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(m_Array, item, 0, m_Count) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(m_Array, 0, array, arrayIndex, m_Count);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(m_Array, item, 0, m_Count);
        }

        public bool Remove(T item)
        {
            int i = Array.IndexOf(m_Array, item, 0, m_Count);
            if (i < 0)
                return false;

            RemoveAt(i);
            return true;
        }

        public void RemoveAt(int aIndex)
        {
            if (aIndex < m_Count)
            {
                --m_Count;
                m_Array[aIndex] = m_Array[m_Count];
                m_Array[m_Count] = default;
            }
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
            public VEnumerator(CCollection<T> aThis)
            {
                m_This = aThis;
                m_Pos = -1;
            }

            public CCollection<T> m_This;
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
