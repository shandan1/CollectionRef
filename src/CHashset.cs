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
    public class CHashset<T> : ICollection<T>, IReadOnlyCollection<T>, ISortable<T>
    {
        protected int m_Count;
        protected int m_EndIndex;
        /// <summary>
        /// 1 based index
        /// </summary>
        protected int[] m_Buckets;
        /// <summary>
        /// 0 based index
        /// </summary>
        protected int m_freeList;
        protected VEntity[] m_Entities;
        protected IEqualityComparer<T> m_Comparer;

        public CHashset() : this(3)
        {
        }
        public CHashset(int aCapacity) : this(aCapacity, EqualityComparer<T>.Default)
        {

        }
        public CHashset(int aCapacity, IEqualityComparer<T> aComparer)
        {
            aCapacity = CHashHelper.MGetPrime(aCapacity);
            m_Buckets = new int[aCapacity];
            m_Entities = new VEntity[aCapacity];
            m_Comparer = aComparer;
            m_freeList = -1;
        }

        public int Count { get { return m_Count; } }

        public int Capacity
        {
            get { return m_Buckets.Length; }
            set
            {
                if (value > m_Buckets.Length)
                    ReSize(value);
            }
        }


        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Union/Multi-Add
        /// </summary>
        public void UnionWith<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            int aNum = m_Count + aOther.Count;
            if (aNum > m_Buckets.Length)
                ReSize(aNum);

            foreach (var aItem in aOther)
            {
                Add(aItem);
            }
        }

        public void IntersectWith<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            if (m_Count == 0)
                return;

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                    return;

                if (aOther2.Count == 0)
                {
                    Clear();
                    return;
                }

                for (int i = 0; i < m_EndIndex; ++i)
                {
                    if (m_Entities[i].m_Hashcode < 0)
                        continue;

                    T aVale = m_Entities[i].m_Value;
                    if (!aOther2.Contains(aVale))
                    {
                        Remove(aVale);
                    }
                }
            }
            else
            {
                byte[] aBytes = new byte[m_Buckets.Length / 8 + 1];
                int i;
                foreach (T aValue in aOther)
                {
                    i = IndexOf(aValue);
                    if (i < 0)
                        continue;
                    aBytes[i / 8] |= (byte)(1 << (i % 8));
                }
                for (i = 0; i < m_EndIndex; ++i)
                {
                    if (m_Entities[i].m_Hashcode > -1 && (aBytes[i / 8] & (1 << (i % 8))) == 0)
                    {
                        Remove(m_Entities[i].m_Value);
                    }
                }
            }
        }
        public void ExceptWith<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            if (m_Count == 0)
                return;

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                {
                    Clear();
                    return;
                }
            }

            foreach (T aItem in aOther)
            {
                Remove(aItem);
            }
        }

        public void XOrWith<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            if (m_Count == 0)
            {
                UnionWith(aOther);
                return;
            }

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                {
                    Clear();
                    return;
                }

                for (int i = 0; i < aOther2.m_EndIndex; ++i)
                {
                    if (aOther2.m_Entities[i].m_Hashcode < 0)
                        continue;

                    T aVale = aOther2.m_Entities[i].m_Value;
                    if (!Remove(aVale))
                        Add(aVale);
                }
            }
            else
            {
                byte[] aBytesN = new byte[m_Buckets.Length / 8 + 1];
                byte[] aBytesC = new byte[m_Buckets.Length / 8 + 1];
                int i;
                foreach (T aValue in aOther)
                {
                    if (AddWithIndex(aValue, out i))
                    {
                        aBytesN[i / 8] |= (byte)(1 << (i % 8));
                    }
                    else
                    {
                        if (i > -1 && (aBytesN[i / 8] & (1 << (i % 8))) == 0)
                            aBytesC[i / 8] |= (byte)(1 << (i % 8));
                    }
                }
                for (i = 0; i < m_EndIndex; ++i)
                {
                    if ((aBytesC[i / 8] & (1 << (i % 8))) != 0)
                    {
                        Remove(m_Entities[i].m_Value);
                    }
                }
            }
        }

        public bool IsSubsetOf<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            if (m_Count == 0)
                return true;

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                    return true;

                if (m_Count > aOther.Count)
                    return false;

                for (int i = 0; i < m_EndIndex; ++i)
                {
                    if (m_Entities[i].m_Hashcode < 0)
                        continue;

                    T aVale = m_Entities[i].m_Value;
                    if (!aOther2.Contains(aVale))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                byte[] aBytesN = new byte[m_Buckets.Length / 8 + 1];
                int aCount = 0;
                foreach (var aItem in aOther)
                {
                    int i = IndexOf(aItem);
                    if (i < 0)
                        continue;

                    if ((aBytesN[i / 8] & (1 << (i % 8))) == 0)
                    {
                        ++aCount;
                        aBytesN[i / 8] |= (byte)(1 << (i % 8));
                    }
                }

                return aCount == m_Count;
            }
        }
        public bool IsSupersetOf<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            if (aOther.Count == 0)
                return true;

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                    return true;
                if (aOther2.Count > m_Count)
                    return false;
            }

            foreach (T aItem in aOther)
            {
                if (!Contains(aItem))
                    return false;
            }
            return true;
        }
        public bool IsOverLap<U>(U aOther) where U : IReadOnlyCollection<T>
        {
            if (m_Count == 0)
                return false;

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                    return true;
            }

            foreach (T aItem in aOther)
            {
                if (Contains(aItem))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsSetEqual<U>(U aOther) where U : IReadOnlyCollection<T>
        {

            if (aOther is CHashset<T>)
            {
                var aOther2 = (CHashset<T>)(object)aOther;
                if (aOther2 == this)
                    return true;

                if (m_Count != aOther2.m_Count)
                    return false;

                foreach (T aItem in aOther)
                {
                    if (!Contains(aItem))
                        return false;
                }
                return true;
            }
            else
            {
                if (m_Count == 0 && aOther.Count > 0)
                    return false;


                byte[] aBytesN = new byte[m_Buckets.Length / 8 + 1];
                int aCount = 0;
                foreach (var aItem in aOther)
                {
                    int i = IndexOf(aItem);
                    if (i < 0)
                        return false;

                    if ((aBytesN[i / 8] & (1 << (i % 8))) == 0)
                    {
                        ++aCount;
                        aBytesN[i / 8] |= (byte)(1 << (i % 8));
                    }
                }
                return aCount == m_Count;
            }
        }

        /// <summary>
        /// Add item
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public void Add(T aValue)
        {
            if (aValue == null)
                return;

            int aHashCode = m_Comparer.GetHashCode(aValue) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int aBucket = aHashCode % aNum;
            int i = m_Buckets[aBucket] - 1;
            var aItems = m_Entities;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Value, aValue))
                    return;

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }

            if (m_freeList > -1)
            {
                i = m_freeList;
                m_freeList = aItems[i].m_Next;
            }
            else
            {
                if (m_EndIndex == aItems.Length)
                {
                    ReSize(m_EndIndex + 1);
                    aItems = m_Entities;
                    aBucket = aHashCode % m_Buckets.Length;
                }
                i = m_EndIndex;
                ++m_EndIndex;
            }
            ref VEntity aItem = ref aItems[i];
            aItem.m_Hashcode = aHashCode;
            aItem.m_Value = aValue;
            aItem.m_Next = m_Buckets[aBucket] - 1;
            m_Buckets[aBucket] = i + 1;
            ++m_Count;

        }
        /// <summary>
        /// Add item and get index
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public bool AddWithIndex(T aValue, out int aIndex)
        {
            if (aValue == null)
            {
                aIndex = -1;
                return false;
            }

            int aHashCode = m_Comparer.GetHashCode(aValue) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int aBucket = aHashCode % aNum;
            int i = m_Buckets[aBucket] - 1;
            var aItems = m_Entities;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Value, aValue))
                {
                    aIndex = i;
                    return false;
                }

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }

            if (m_freeList > -1)
            {
                i = m_freeList;
                m_freeList = aItems[i].m_Next;
            }
            else
            {
                if (m_EndIndex == aItems.Length)
                {
                    ReSize(m_EndIndex + 1);
                    aItems = m_Entities;
                    aBucket = aHashCode % m_Buckets.Length;
                }
                i = m_EndIndex;
                ++m_EndIndex;
            }

            ref VEntity aItem = ref aItems[i];
            aItem.m_Hashcode = aHashCode;
            aItem.m_Value = aValue;
            aItem.m_Next = m_Buckets[aBucket] - 1;
            m_Buckets[aBucket] = i + 1;
            ++m_Count;
            aIndex = i;
            return true;
        }

        public bool Remove(T aValue)
        {
            int aHashCode = m_Comparer.GetHashCode(aValue) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int a = aHashCode % aNum;
            int i = m_Buckets[a] - 1;
            var aItems = m_Entities;
            int aLast = -1;
            while (i > -1)
            {
                ref var aItem = ref aItems[i];
                if (aItem.m_Hashcode == aHashCode && m_Comparer.Equals(aItem.m_Value, aValue))
                {
                    if (aLast < 0)
                        m_Buckets[a] = aItem.m_Next + 1;
                    else
                        aItems[aLast].m_Next = aItem.m_Next;

                    aItem.m_Hashcode = -1;
                    aItem.m_Value = default;
                    aItem.m_Next = m_freeList;
                    m_freeList = i;
                    --m_Count;

                    return true;
                }

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }

            return false;
        }

        /// <summary>
        /// Make a continuous data block ,begin with 0 index,better for iterator
        /// </summary>
        public void Compress()
        {
            if (m_Count == m_EndIndex)
                return;

            int i = 0;
            int a = 0;
            var aEntities = m_Entities;
            while (i < m_EndIndex)
            {
                if (aEntities[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }
                if (a != i)
                    aEntities[a] = aEntities[i];

                ++a;
                ++i;
            }

            //clear
            i = a;
            while (i < m_EndIndex)
            {
                aEntities[i].m_Value = default;
                ++i;
            }
            //rebuild
            m_EndIndex = m_Count = a;
            i = 0;
            int aLen = m_Buckets.Length;
            CArray.Clear(m_Buckets, 0, aLen);
            while (i < m_EndIndex)
            {
                a = aEntities[i].m_Hashcode % aLen;
                aEntities[i].m_Next = m_Buckets[a] - 1;
                m_Buckets[a] = ++i;
            }
        }
        /// <summary>
        /// Compress and sort the data.
        /// </summary>
        public void Sort(IComparer<T> aComparer)
        {
            int i = 0;
            int a = 0;
            var aEntities = m_Entities;
            while (i < m_EndIndex)
            {
                if (aEntities[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }
                if (a != i)
                    aEntities[a] = aEntities[i];

                ++a;
                ++i;
            }

            //clear
            i = a;
            while (i < m_EndIndex)
            {
                aEntities[i].m_Value = default;
                ++i;
            }

            QuickSortKey(0, a, aComparer);
            //rebuild
            m_EndIndex = m_Count = a;
            i = 0;
            int aLen = m_Buckets.Length;
            CArray.Clear(m_Buckets, 0, aLen);
            while (i < m_EndIndex)
            {
                a = aEntities[i].m_Hashcode % aLen;
                aEntities[i].m_Next = m_Buckets[a] - 1;
                m_Buckets[a] = ++i;
            }
        }
        /// <summary>
        /// Increase capacity and compress data.
        /// </summary>
        /// <param name="aCapacity"></param>
        protected void ReSize(int aCapacity)
        {
            aCapacity = CHashHelper.MGetPrime(aCapacity);
            var aNewItems = new VEntity[aCapacity];
            int i = 0;
            int o = 0;
            while (i < m_EndIndex)
            {
                if (m_Entities[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }

                aNewItems[o] = m_Entities[i];
                ++o;
                ++i;
            }
            var aNewBuckets = new int[aCapacity];
            i = 0;
            while (i < o)
            {
                int a = aNewItems[i].m_Hashcode % aCapacity;
                aNewItems[i].m_Next = aNewBuckets[a] - 1;
                aNewBuckets[a] = i + 1;
            }
            m_freeList = -1;
            m_EndIndex = m_Count = o;
            m_Buckets = aNewBuckets;
            m_Entities = aNewItems;
        }

        public void Clear()
        {
            m_EndIndex = 0;
            m_Count = 0;
            m_freeList = -1;
            CArray.Clear(m_Buckets, 0, m_Buckets.Length);
            CArray.Clear(m_Entities, 0, m_EndIndex);
        }

        public bool Contains(T aValue)
        {
            int aHashCode = m_Comparer.GetHashCode(aValue) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int i = m_Buckets[aHashCode % aNum] - 1;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Value, aValue))
                    return true;

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }

            return false;
        }

        /// <summary>
        /// Get the index of the value.add/remove/compress/sort opts may change it.
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public int IndexOf(T aValue)
        {
            int aHashCode = m_Comparer.GetHashCode(aValue) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int i = m_Buckets[aHashCode % aNum] - 1;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Value, aValue))
                    return i;

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }

            return -1;
        }

        /// <summary>
        /// Get the Value frome the index.add/remove/compress/sort opts may change it,and throw exception.
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public T GetValueAt(int aIndex)
        {
            if (aIndex >= m_EndIndex || m_Entities[aIndex].m_Hashcode < 0)
                throw new InvalidOperationException();

            return m_Entities[aIndex].m_Value;

        }


        public void CopyTo(T[] aArr, int aArrIndex)
        {
            for (int i = 0; i < m_EndIndex; ++i)
            {
                if (m_Entities[i].m_Hashcode != -1)
                {
                    aArr[aArrIndex] = m_Entities[i].m_Value;
                    ++aArrIndex;
                }
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
        protected struct VEntity
        {
            /// <summary>
            /// -1 means deleted
            /// </summary>
            internal int m_Hashcode;
            /// <summary>
            /// 0 based index
            /// </summary>
            internal int m_Next;
            internal T m_Value;
        }

        public struct VEnumerator : IEnumerator<T>
        {
            public VEnumerator(CHashset<T> aThis)
            {
                m_This = aThis;
                m_Pos = -1;
            }

            public CHashset<T> m_This;
            public int m_Pos;

            public T Current { get { return m_This.m_Entities[m_Pos].m_Value; } }

            object IEnumerator.Current { get { return m_This.m_Entities[m_Pos].m_Value; } }

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
                int aEnd = m_This.m_EndIndex;
                var aItems = m_This.m_Entities;
                for (int i = m_Pos + 1; i < aEnd; ++i)
                {
                    if (aItems[i].m_Hashcode > -1)
                    {
                        m_Pos = i;
                        return true;
                    }
                }
                return false;
            }
        }

        private void QuickSortKey(int low, int high, IComparer<T> aComparer)
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
                        SwapIfGreaterKey(ref m_Entities[low], ref m_Entities[high], aComparer);
                        return;
                    }
                    if (i == 2)
                    {
                        SwapIfGreaterKey(ref m_Entities[low], ref m_Entities[low + 1], aComparer);
                        SwapIfGreaterKey(ref m_Entities[low], ref m_Entities[high], aComparer);
                        SwapIfGreaterKey(ref m_Entities[low + 1], ref m_Entities[high], aComparer);
                        return;
                    }
                    InsertSortKey(low, high, aComparer);
                    return;
                }

                i = low + ((high -low)>> 1); //middle
                SwapIfGreaterKey(ref m_Entities[low], ref m_Entities[i], aComparer);
                SwapIfGreaterKey(ref m_Entities[low], ref m_Entities[high], aComparer);
                SwapIfGreaterKey(ref m_Entities[i], ref m_Entities[high], aComparer);
                T pivot = m_Entities[i].m_Value;
                int j = high - 1; //
                Swap(ref m_Entities[i], ref m_Entities[j]);
                i = low + 1; //
                while (i < j)
                {
                    if (aComparer.Compare(m_Entities[i].m_Value, pivot) > 0)
                    {
                        --j;
                        if (i == j)
                            break;

                        Swap(ref m_Entities[i], ref m_Entities[j]);
                    }
                    else
                        ++i;
                }
                if (i != high - 1)
                    Swap(ref m_Entities[i], ref m_Entities[high - 1]);

                QuickSortKey(i + 1, high, aComparer);
                high = i - 1;
            }
        }
        private void InsertSortKey(int low, int high, IComparer<T> aComparer)
        {
            int i, j;
            VEntity t1;
            for (i = low; i < high; ++i)
            {
                j = i;
                t1 = m_Entities[i + 1];
                while (j >= low && aComparer.Compare(m_Entities[j].m_Value, t1.m_Value) > 0)
                {
                    m_Entities[j + 1] = m_Entities[j];
                    --j;
                }
                m_Entities[j + 1] = t1;
            }
        }
        private void SwapIfGreaterKey(ref VEntity a, ref VEntity b, IComparer<T> aComparer)
        {

            if (aComparer.Compare(a.m_Value, b.m_Value) > 0)
            {
                VEntity c = a;
                a = b;
                b = c;
            }
        }

        private void Swap(ref VEntity i1, ref VEntity i2)
        {
            VEntity c1 = i1;
            i1 = i2;
            i2 = c1;
        }
    }
}
