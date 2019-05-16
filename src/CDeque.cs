/*
    This file belong to CollectionRef project,link address [https://github.com/ColinGao225/CollectionRef]
    Written by Colin Gao at 16th/5/2019  
    This project is forked from Microsoft source codes [https://github.com/dotnet/corefx]
    These have a MIT Lisence.That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 
 */
using System;

namespace Colin.CollectionRef
{
     public class CDeque<T> :CQueue<T>
    {
        public T RemoveLast()
        {
            if (m_Count == 0)
                throw new InvalidOperationException("is empty");

            var aLast = m_Head + m_Count - 1;
            var aLen = m_Array.Length;
            if (aLast >= aLen)
                aLast -= aLen;

            var aItem = m_Array[aLast];
            m_Array[aLast] = default;
            --m_Count;
            return aItem;
        }

        public bool TryRemoveLast(out T aResult)
        {
            if (m_Count == 0)
            {
                aResult = default;
                return false;
            }

            var aLast = m_Head + m_Count - 1;
            var aLen = m_Array.Length;
            if (aLast >= aLen)
                aLast -= aLen;

            aResult = m_Array[aLast];
            m_Array[aLast] = default;
            --m_Count;
            return true;
        }

        /// <summary>
        /// Return the item at the tail of the queue,but not remove it.throw exception when empty
        /// </summary>
        /// <returns></returns>
        public T PeekLast()
        {
            if (m_Count == 0)
                throw new InvalidOperationException("is empty");

            var aLast = m_Head + m_Count - 1;
            var aLen = m_Array.Length;
            if (aLast >= aLen)
                aLast -= aLen;

            return m_Array[aLast];
        }
        /// <summary>
        /// Return the item at the tail of the queue,but not remove it.return false when empty.
        /// </summary>
        /// <returns></returns>
        public bool TryPeekLast(out T aResult)
        {
            if (m_Count == 0)
            {
                aResult = default;
                return false;
            }
            var aLast = m_Head + m_Count - 1;
            var aLen = m_Array.Length;
            if (aLast >= aLen)
                aLast -= aLen;

            aResult = m_Array[aLast];
            return true;
        }


        /// <summary>
        /// Add item to the head of the queue
        /// </summary>
        /// <param name="aItem"></param>
        public void AddFirst(T aItem)
        {
            CheckSize(1);

            --m_Head;
            if (m_Head == -1)
                m_Head = m_Array.Length - 1;

            m_Array[m_Head] = aItem;
            ++m_Count;
        }
    }
}
