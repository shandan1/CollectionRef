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
    /// Dictionary  
    /// </summary>
    public class CDictionary<Key, Value> :
        IDictionary<Key, Value>, IReadOnlyCollection<KeyValuePair<Key, Value>>
    {
        protected int m_Count;
        protected int m_EndIndex;
        /// <summary>
        /// 0 based index
        /// </summary>
        protected int m_freeList;
        /// <summary>
        /// 1 based index
        /// </summary>
        protected int[] m_Buckets;
        protected VEntity[] m_Entities;
        protected Value[] m_Values;
        protected IEqualityComparer<Key> m_Comparer;

        public CDictionary():this(3)
        {

        }
        public CDictionary(int aCapacity):this(aCapacity,EqualityComparer<Key>.Default)
        {
           
        }
        public CDictionary(int aCapacity, IEqualityComparer<Key> aComparer)
        {
            aCapacity = CHashHelper.MGetPrime(aCapacity);
            m_freeList = -1;
            m_Buckets = new int[aCapacity];
            m_Entities = new VEntity[aCapacity];
            m_Values = new Value[aCapacity];
            m_Comparer = aComparer;
        }

        /// <summary>
        /// Get the index of key.structure changed opts may change it.
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public int IndexOfKey(Key aKey)
        {
            int aHashCode = m_Comparer.GetHashCode(aKey) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int i = m_Buckets[aHashCode % aNum] - 1;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Key, aKey))
                    return i;

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }
            return -1;
        }

        /// <summary>
        /// Get the index of value.structure changed opts may change it.
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public int IndexOfValue(Value aValue)
        {
            if (aValue == null)
            {
                for (int i = 0; i < m_EndIndex; ++i)
                {
                    if (m_Entities[i].m_Hashcode < 0)
                        continue;

                    if (m_Values[i] == null)
                        return i;
                }
            }
            else
            {
                for (int i = 0; i < m_EndIndex; ++i)
                {
                    if (m_Entities[i].m_Hashcode < 0)
                        continue;

                    if (aValue.Equals(m_Values[i]))
                        return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// Get key at index .structure changed opts may change it,and throw an exception.
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public Key GetKeyAt(int aIndex)
        {
            if (aIndex < m_EndIndex && m_Entities[aIndex].m_Hashcode > -1)
                return m_Entities[aIndex].m_Key;

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Get value at the index.structure changed opts may change it,and throw an exception.
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public Value GetValueAt(int aIndex)
        {
            if (aIndex < m_EndIndex && m_Entities[aIndex].m_Hashcode > -1)
                return m_Values[aIndex];
            else
                throw new InvalidOperationException();
        }
        /// <summary>
        /// Set value at the index.structure changed opts may change it,and throw an exception.
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public void SetValueAt(int aIndex, Value aValue)
        {
            if (aIndex < m_EndIndex && m_Entities[aIndex].m_Hashcode > -1)
                m_Values[aIndex] = aValue;
            else
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Get value ref at the index.structure changed opts may change it,and throw an exception.
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public ref Value GetValueRefAt(int aIndex)
        {
            if (aIndex < m_EndIndex && m_Entities[aIndex].m_Hashcode > -1)
                return ref m_Values[aIndex];
            else
                throw new InvalidOperationException();
        }

        public bool TryGetValue(Key aKey, out Value aValue)
        {
            int i = IndexOfKey(aKey);
            if (i < 0)
            {
                aValue = default;
                return false;
            }
            aValue = m_Values[i];
            return true;
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
            var aItems = m_Entities;
            while (i < m_EndIndex)
            {
                if (aItems[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }
                if (a != i)
                    aItems[a] = aItems[i];

                ++a;
                ++i;
            }

            //clear
            i = a;
            while (i < m_EndIndex)
            {
                aItems[i].m_Key = default;
                m_Values[i] = default;
                ++i;
            }

            //rebuild
            m_EndIndex = m_Count = a;
            i = 0;
            int aLen = m_Buckets.Length;
            CArray.Clear(m_Buckets, 0, aLen);
            while (i < m_EndIndex)
            {
                a = aItems[i].m_Hashcode % aLen;
                aItems[i].m_Next = m_Buckets[a] - 1;
                m_Buckets[a] = ++i;
            }
        }

        /// <summary>
        /// Compress and sort the data.
        /// </summary>
        public void SortByKey(IComparer<Key> aComparer)
        {
            int i = 0;
            int a = 0;
            var aItems = m_Entities;
            while (i < m_EndIndex)
            {
                if (aItems[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }
                if (a != i)
                    aItems[a] = aItems[i];

                ++a;
                ++i;
            }

            //clear
            i = a;
            while (i < m_EndIndex)
            {
                aItems[i].m_Key = default;
                m_Values[i] = default;
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
                a = aItems[i].m_Hashcode % aLen;
                aItems[i].m_Next = m_Buckets[a] - 1;
                m_Buckets[a] = ++i;
            }
        }

        /// <summary>
        /// Compress and sort the data.
        /// </summary>
        public void SortByValue(IComparer<Value> aComparer)
        {
            int i = 0;
            int a = 0;
            var aItems = m_Entities;
            while (i < m_EndIndex)
            {
                if (aItems[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }
                if (a != i)
                    aItems[a] = aItems[i];

                ++a;
                ++i;
            }

            //clear
            i = a;
            while (i < m_EndIndex)
            {
                aItems[i].m_Key = default;
                m_Values[i] = default;
                ++i;
            }

            QuickSortValue(0, a, aComparer);

            //rebuild
            m_EndIndex = m_Count = a;
            i = 0;
            int aLen = m_Buckets.Length;
            CArray.Clear(m_Buckets, 0, aLen);
            while (i < m_EndIndex)
            {
                a = aItems[i].m_Hashcode % aLen;
                aItems[i].m_Next = m_Buckets[a] - 1;
                m_Buckets[a] = ++i;
            }
        }

        protected void ReSize(int aCapacity)
        {
            aCapacity = CHashHelper.MGetPrime(aCapacity);
            var aNewEntities = new VEntity[aCapacity];
            var aNewValues = new Value[aCapacity];
            int i = 0;
            int o = 0;
            while (i < m_EndIndex)
            {
                if (m_Entities[i].m_Hashcode < 0)
                {
                    ++i;
                    continue;
                }

                aNewEntities[o] = m_Entities[i];
                aNewValues[o] = m_Values[i];
                ++o;
                ++i;
            }
            var aNewBuckets = new int[aCapacity];
            i = 0;
            while (i < o)
            {
                int a = aNewEntities[i].m_Hashcode % aCapacity;
                aNewEntities[i].m_Next = aNewBuckets[a] - 1;
                aNewBuckets[a] = i + 1;
            }
            m_freeList = -1;
            m_EndIndex = m_Count = o;
            m_Buckets = aNewBuckets;
            m_Entities = aNewEntities;
            m_Values = aNewValues;
        }

        public Value this[Key aKey]
        {
            get
            {
                int i = IndexOfKey(aKey);
                if (i < 0)
                    throw new KeyNotFoundException();
                return m_Values[i];
            }
            set
            {
                Replace(aKey, value);
            }
        }

        public VKeys Keys { get { return new VKeys(this); } }
        ICollection<Key> IDictionary<Key, Value>.Keys { get { return new VKeys(this); } }
        public VValues Values { get { return new VValues(this); } }
        ICollection<Value> IDictionary<Key, Value>.Values { get { return new VValues(this); } }

        public int Count { get { return m_Count; } }

        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Add item.When contain the key ,do nothing
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public void Add(Key aKey, Value aValue)
        {
            AddWithIndex(aKey, aValue);
        }
        /// <summary>
        /// Add item.When contain the key ,do nothing
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public void Add(KeyValuePair<Key, Value> aItem)
        {
            AddWithIndex(aItem.Key, aItem.Value);
        }

        /// <summary>
        /// Add item and return index.When contain the key ,do nothing and retrun the inversed index.
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public int AddWithIndex(Key aKey, Value aValue)
        {
            if (aKey == null)
                return int.MinValue;

            int aHashCode = m_Comparer.GetHashCode(aKey) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int aBucket = aHashCode % aNum;
            int i = m_Buckets[aBucket] - 1;
            var aEntities = m_Entities;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Key, aKey))
                {
                    return ~i;
                }

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }
            if (m_freeList > -1)
            {
                i = m_freeList;
                m_freeList = aEntities[i].m_Next;
            }
            else
            {
                if (m_EndIndex == aEntities.Length)
                {
                    ReSize(m_EndIndex + 1);
                    aEntities = m_Entities;
                    aBucket = aHashCode % m_Buckets.Length;
                }
                i = m_EndIndex;
                ++m_EndIndex;
            }

            ref VEntity aEntity = ref aEntities[i];
            aEntity.m_Hashcode = aHashCode;
            aEntity.m_Key = aKey;
            aEntity.m_Next = m_Buckets[aBucket] - 1;
            m_Values[i] = aValue;
            m_Buckets[aBucket] = i + 1;
            ++m_Count;

            return i;
        }

        /// <summary>
        /// Add item.When contain the key ,replace it and return the index
        /// </summary>
        /// <param name="aKey"></param>
        /// <returns></returns>
        public int Replace(Key aKey, Value aValue)
        {
            if (aKey == null)
                return int.MinValue;

            int aHashCode = m_Comparer.GetHashCode(aKey) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int aBucket = aHashCode % aNum;
            int i = m_Buckets[aBucket] - 1;
            var aEntities = m_Entities;

            while (i > -1)
            {
                if (m_Entities[i].m_Hashcode == aHashCode && m_Comparer.Equals(m_Entities[i].m_Key, aKey))
                {
                    m_Values[i] = aValue;
                    return i;
                }

                if (aNum < 1)
                    throw new InvalidOperationException();
                --aNum;

                i = m_Entities[i].m_Next;
            }
            if (m_freeList > -1)
            {
                i = m_freeList;
                m_freeList = aEntities[i].m_Next;
            }
            else
            {
                if (m_EndIndex == aEntities.Length)
                {
                    ReSize(m_EndIndex + 1);
                    aEntities = m_Entities;
                    aBucket = aHashCode % m_Buckets.Length;
                }
                i = m_EndIndex;
                ++m_EndIndex;
            }

            ref VEntity aEntity = ref aEntities[i];
            aEntity.m_Hashcode = aHashCode;
            aEntity.m_Key = aKey;
            aEntity.m_Next = m_Buckets[aBucket] - 1;
            m_Values[i] = aValue;
            m_Buckets[aBucket] = i + 1;
            ++m_Count;
            return i;
        }

        public void Clear()
        {
            m_EndIndex = 0;
            m_Count = 0;
            m_freeList = -1;
            CArray.Clear(m_Buckets, 0, m_Buckets.Length);
            CArray.Clear(m_Entities, 0, m_EndIndex);
            CArray.Clear(m_Values, 0, m_EndIndex);
        }

        public bool Contains(KeyValuePair<Key, Value> aItem)
        {
            return IndexOfKey(aItem.Key) > -1;
        }

        public bool ContainsKey(Key aKey)
        {
            return IndexOfKey(aKey) > -1;
        }

        public void CopyTo(KeyValuePair<Key, Value>[] aArr, int aArrIndex)
        {
            for (int i = 0; i < m_EndIndex; ++i)
            {
                if (m_Entities[i].m_Hashcode < 0)
                    continue;

                aArr[aArrIndex] = new KeyValuePair<Key, Value>(m_Entities[i].m_Key, m_Values[i]);
                ++aArrIndex;
            }
        }


        public bool Remove(Key aKey)
        {
            int aHashCode = m_Comparer.GetHashCode(aKey) & CHashHelper.F31BitMask;
            int aNum = m_Buckets.Length;
            int a = aHashCode % aNum;
            int i = m_Buckets[a] - 1;
            var aItems = m_Entities;
            int aLast = -1;
            while (i > -1)
            {
                ref var aItem = ref aItems[i];
                if (aItem.m_Hashcode == aHashCode && m_Comparer.Equals(aItem.m_Key, aKey))
                {
                    if (aLast < 0)
                        m_Buckets[a] = aItem.m_Next + 1;
                    else
                        aItems[aLast].m_Next = aItem.m_Next;

                    aItem.m_Hashcode = -1;
                    aItem.m_Key = default;
                    aItem.m_Next = m_freeList;
                    m_Values[i] = default;

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

        public bool Remove(KeyValuePair<Key, Value> aItem)
        {
            return Remove(aItem.Key);
        }

     

        /// <summary>
        /// Compress and return the value array.
        /// </summary>
        /// <returns></returns>
        public Value[] GetValueArrayCompressed()
        {
            Compress();

            return m_Values;
        }

        public VEntities GetEnumerator()
        {
            return new VEntities(this);
        }

        IEnumerator<KeyValuePair<Key, Value>> IEnumerable<KeyValuePair<Key, Value>>.GetEnumerator()
        {
            return new VEntities(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new VEntities(this);
        }


        public struct VEntity
        {
            /// <summary>
            /// positvie value, -1 means deleted
            /// </summary>
            internal int m_Hashcode;
            /// <summary>
            /// 0 based index
            /// </summary>
            internal int m_Next;
            internal Key m_Key;
        }

        /// <summary>
        /// Keys collection
        /// </summary>
        public struct VKeys : ICollection<Key>, IEnumerator<Key>
        {
            private CDictionary<Key, Value> m_This;
            private int m_Pos;
            public VKeys(CDictionary<Key, Value> aThis)
            {
                m_This = aThis;
                m_Pos = -1;
            }

            public int Count { get { return m_This.m_Count; } }

            public bool IsReadOnly { get { return false; } }

            public void Add(Key item)
            {
                m_This.Add(item, default);
            }

            public void Clear()
            {
                m_This.Clear();
            }

            public bool Contains(Key aItem)
            {
                return m_This.ContainsKey(aItem);
            }

            public void CopyTo(Key[] aArr, int aArrIndex)
            {
                var aEntities = m_This.m_Entities;
                var aEnd = m_This.m_EndIndex;
                for (int i = 0; i < aEnd; ++i)
                {
                    if (aEntities[i].m_Hashcode < 0)
                        continue;

                    aArr[aArrIndex] = aEntities[i].m_Key;
                    ++aArrIndex;
                }
            }
            public VKeys GetEnumerator()
            {
                return this;
            }
            IEnumerator<Key> IEnumerable<Key>.GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            public bool Remove(Key aKey)
            {
                return m_This.Remove(aKey);
            }

            public Key Current { get { return m_This.m_Entities[m_Pos].m_Key; } }

            object IEnumerator.Current { get { return m_This.m_Entities[m_Pos].m_Key; } }

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

            public void Reset()
            {
                m_Pos = -1;
            }

            public void Dispose()
            {
                m_Pos = -1;
            }
        }

        /// <summary>
        /// Value collection
        /// </summary>
        public struct VValues : ICollection<Value>, IEnumerator<Value>, IEnumerableRef<Value>, IEnumeratorRef<Value>
        {
            private CDictionary<Key, Value> m_This;
            private int m_Pos;
            public VValues(CDictionary<Key, Value> aThis)
            {
                m_This = aThis;
                m_Pos = -1;
            }

            public int Count { get { return m_This.Count; } }

            public bool IsReadOnly { get { return false; } }

            public Value Current { get { return m_This.m_Values[m_Pos]; } }
            object IEnumerator.Current { get { return m_This.m_Values[m_Pos]; } }
            public ref Value CurrentRef { get { return ref m_This.m_Values[m_Pos]; } }

            public void Add(Value item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                m_This.Clear();
            }

            public bool Contains(Value aValue)
            {
                var aEntities = m_This.m_Entities;
                var aValues = m_This.m_Values;
                var aEnd = m_This.m_EndIndex;
                if (aValue == null)
                {
                    for (int i = 0; i < aEnd; ++i)
                    {
                        if (aEntities[i].m_Hashcode < 0)
                            continue;

                        if (aValues[i] == null)
                            return true;
                    }
                }
                else
                {
                    for (int i = 0; i < aEnd; ++i)
                    {
                        if (aEntities[i].m_Hashcode < 0)
                            continue;

                        if (aValue.Equals(aValues[i]))
                            return true;
                    }
                }

                return false;
            }

            public void CopyTo(Value[] aArr, int aArrIndex)
            {
                var aEntities = m_This.m_Entities;
                var aValues = m_This.m_Values;
                var aEnd = m_This.m_EndIndex;
                for (int i = 0; i < aEnd; ++i)
                {
                    if (aEntities[i].m_Hashcode < 0)
                        continue;

                    aArr[aArrIndex] = aValues[i];
                    ++aArrIndex;
                }
            }

            public bool Remove(Value aValue)
            {
                var aEntities = m_This.m_Entities;
                var aValues = m_This.m_Values;
                var aEnd = m_This.m_EndIndex;

                if (aValue == null)
                {
                    for (int i = 0; i < aEnd; ++i)
                    {
                        if (aEntities[i].m_Hashcode < 0)
                            continue;

                        if (aValues[i] == null)
                            return m_This.Remove(aEntities[i].m_Key);
                    }
                }
                else
                {
                    for (int i = 0; i < aEnd; ++i)
                    {
                        if (aEntities[i].m_Hashcode < 0)
                            continue;

                        if (aValue.Equals(aValues[i]))
                            return m_This.Remove(aEntities[i].m_Key);
                    }
                }
                return false;
            }

            public void Reset()
            {
                m_Pos = -1;
            }

            public void Dispose()
            {
                m_Pos = -1;
            }

            public VValues GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            IEnumerator<Value> IEnumerable<Value>.GetEnumerator()
            {
                return this;
            }

            public VValues GetEnumeratorRef()
            {
                return this;
            }

            IEnumeratorRef<Value> IEnumerableRef<Value>.GetEnumeratorRef()
            {
                return this;
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

        public struct VEntities : IEnumerator<KeyValuePair<Key, Value>>
        {
            private CDictionary<Key, Value> m_This;
            private int m_Pos;
            public VEntities(CDictionary<Key, Value> aThis)
            {
                m_Pos = -1;
                m_This = aThis;
            }
            public KeyValuePair<Key, Value> Current
            {
                get
                {
                    return new KeyValuePair<Key, Value>(
                        m_This.m_Entities[m_Pos].m_Key,
                        m_This.m_Values[m_Pos]);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return new KeyValuePair<Key, Value>(
                        m_This.m_Entities[m_Pos].m_Key,
                        m_This.m_Values[m_Pos]);
                }
            }

            public void Dispose()
            {
                m_Pos = -1;
                m_This = null;
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

            public void Reset()
            {
                m_Pos = -1;
            }
        }


        private void QuickSortKey(int low, int high, IComparer<Key> aComparer)
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
                        SwapIfGreaterKey(low, high, aComparer);
                        return;
                    }
                    if (i == 2)
                    {
                        SwapIfGreaterKey(low, low + 1, aComparer);
                        SwapIfGreaterKey(low, high, aComparer);
                        SwapIfGreaterKey(low + 1, high, aComparer);
                        return;
                    }
                    InsertSortKey(low, high, aComparer);
                    return;
                }

                i = low + ((high - low) >> 1);  //middle
                SwapIfGreaterKey(low, i, aComparer);
                SwapIfGreaterKey(low, high, aComparer);
                SwapIfGreaterKey(i, high, aComparer);
                Key pivot = m_Entities[i].m_Key;
                int j = high - 1; //
                Swap(i, j);
                i = low + 1; //
                while (i < j)
                {
                    if (aComparer.Compare(m_Entities[i].m_Key, pivot) > 0)
                    {
                        --j;
                        if (i == j)
                            break;

                        Swap(i, j);
                    }
                    else
                        ++i;
                }
                if (i != high - 1)
                    Swap(i, high - 1);

                QuickSortKey(i + 1, high, aComparer);
                high = i - 1;
            }
        }
        private void InsertSortKey(int low, int high, IComparer<Key> aComparer)
        {
            int i, j;
            VEntity t1;
            Value t2;
            for (i = low; i < high; ++i)
            {
                j = i;
                t1 = m_Entities[i + 1];
                t2 = m_Values[i + 1];
                while (j >= low && aComparer.Compare(m_Entities[j].m_Key, t1.m_Key) > 0)
                {
                    m_Entities[j + 1] = m_Entities[j];
                    m_Values[j + 1] = m_Values[j];
                    --j;
                }
                m_Entities[j + 1] = t1;
                m_Values[j + 1] = t2;
            }
        }
        private void SwapIfGreaterKey(int i1, int i2, IComparer<Key> aComparer)
        {
            ref VEntity a = ref m_Entities[i1];
            ref VEntity b = ref m_Entities[i2];
            if (aComparer.Compare(a.m_Key, b.m_Key) > 0)
            {
                VEntity c = a;
                a = b;
                b = c;

                Value c1 = m_Values[i1];
                m_Values[i1] = m_Values[i2];
                m_Values[i2] = c1;
            }
        }

        private void QuickSortValue(int low, int high, IComparer<Value> aComparer)
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
                        SwapIfGreaterValue(low, high, aComparer);
                        return;
                    }
                    if (i == 2)
                    {
                        SwapIfGreaterValue(low, low + 1, aComparer);
                        SwapIfGreaterValue(low, high, aComparer);
                        SwapIfGreaterValue(low + 1, high, aComparer);
                        return;
                    }
                    InsertSortValue(low, high, aComparer);
                    return;
                }

                i = low + ((high - low) >> 1);  //middle
                SwapIfGreaterValue(low, i, aComparer);
                SwapIfGreaterValue(low, high, aComparer);
                SwapIfGreaterValue(i, high, aComparer);
                Value pivot = m_Values[i];
                int j = high - 1; //
                Swap(i, j);
                i = low + 1; //
                while (i < j)
                {
                    if (aComparer.Compare(m_Values[i], pivot) > 0)
                    {
                        --j;
                        if (i == j)
                            break;

                        Swap(i, j);
                    }
                    else
                        ++i;
                }
                if (i != high - 1)
                    Swap(i, high - 1);

                QuickSortValue(i + 1, high, aComparer);
                high = i - 1;
            }
        }

        private void InsertSortValue(int low, int high, IComparer<Value> aComparer)
        {
            int i, j;
            VEntity t1;
            Value t2;
            for (i = low; i < high; ++i)
            {
                j = i;
                t1 = m_Entities[i + 1];
                t2 = m_Values[i + 1];
                while (j >= low && aComparer.Compare(m_Values[j], t2) > 0)
                {
                    m_Entities[j + 1] = m_Entities[j];
                    m_Values[j + 1] = m_Values[j];
                    --j;
                }
                m_Entities[j + 1] = t1;
                m_Values[j + 1] = t2;
            }
        }

        private void SwapIfGreaterValue(int i1, int i2, IComparer<Value> aComparer)
        {
            ref Value a = ref m_Values[i1];
            ref Value b = ref m_Values[i2];
            if (aComparer.Compare(a, b) > 0)
            {
                VEntity c1 = m_Entities[i1];
                m_Entities[i1] = m_Entities[i2];
                m_Entities[i2] = c1;

                Value c = a;
                a = b;
                b = c;
            }
        }

        private void Swap(int i1, int i2)
        {
            VEntity k1 = m_Entities[i1];
            m_Entities[i1] = m_Entities[i2];
            m_Entities[i2] = k1;

            Value c1 = m_Values[i1];
            m_Values[i1] = m_Values[i2];
            m_Values[i2] = c1;
        }
    }
}
