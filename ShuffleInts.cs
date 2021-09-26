using System;
using System.Collections.Generic;

namespace MatrixScreen
{
    /// <summary>
    /// This class provides a collection of unique integers, in sequence, which can be retrieved randomly and restored when finished.
    /// The purpose of this is to restrict access to to a list of indexes so that only one is used at a time.
    /// </summary>
    class ShuffleInts
    {
        private Random _random;
        private HashSet<int> _availableInts;
        internal int SetSize { get; private set; }

        internal ShuffleInts(int setSize)
        {
            SetSize = setSize;

            _random = new Random();
            _availableInts = new HashSet<int>();

            for (int x = 0; x < SetSize; x++)
            {
                _availableInts.Add(x);
            }
        }

        /// <summary>
        /// Fetches the integer value at the desired index within the set.
        /// </summary>
        /// <param name="desiredIndex"></param>
        /// <returns></returns>
        internal int CheckoutInteger(int desiredIndex)
        {
            int index = 0;
            foreach (int integer in _availableInts)
            {
                // We iterate over the list a random number of times, and grab the index at that location.
                if (index == desiredIndex)
                {
                    _availableInts.Remove(integer);
                    return integer;
                }
                index++;
            }
            return -1;

        }

        /// <summary>
        /// Fetches the integer value at a random index within the set.
        /// </summary>
        /// <returns></returns>
        internal int CheckoutRandomInteger()
        {
            int randomIndex = _random.Next(0, _availableInts.Count);
            return CheckoutInteger(randomIndex);
        }

        /// <summary>
        /// Returns a previously checked out integer to the set.
        /// </summary>
        /// <param name="integer"></param>
        internal void CheckinInteger(int integer)
        {
            if (integer < SetSize)
            {
                _availableInts.Add(integer);
            }
        }
    }
}
