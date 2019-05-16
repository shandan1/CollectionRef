/*
    This file belong to CollectionRef project,link address [https://github.com/ColinGao225/CollectionRef]
    Written by Colin Gao at 16th/5/2019  
    This project is forked from Microsoft source codes [https://github.com/dotnet/corefx]
    These have a MIT Lisence.That means you can use and  modify the code at all,in the condition that you specify the refferences in your project. 
 */
using System;
namespace Colin
{
    public static class CHashHelper
    {
        public const int F31BitMask = 0x7F_FF_FF_FF;

        /// <summary>
        /// prime array in integer range
        /// </summary>
        public static readonly int[] m_Primes = { 3, 7, 11, 17, 29, 43, 67, 101, 151, 227, 347, 521, 787, 1181, 1777, 2671, 4007, 6011, 9029, 13553, 20333, 30509, 45763, 68659, 103001, 154501, 231779, 347671, 521519, 782297, 1173463, 1760203, 2640317, 3960497, 5940761, 8911141, 13366711, 20050081, 30075127, 45112693, 67669079, 101503627, 152255461, 228383273, 342574909, 513862367, 770793589, 1156190419, 1734285653, 2147483647 };

        /// <summary>
        /// Get the smallest prime int greater than the value
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static int MGetPrime(int aValue)
        {
            int aLen = m_Primes.Length;
            for (int i = 0; i < aLen; ++i)
            {
                if (m_Primes[i] < aValue)
                    continue;

                return m_Primes[i];
            }
            throw new ArgumentOutOfRangeException();
        }

    }
}
