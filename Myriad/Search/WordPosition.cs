using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Search
{
    public class WordPosition : IComparable<WordPosition>
    {
        readonly int wordIndex;
        readonly int length;
        readonly int queryIndex;

        public int QueryIndex
        {
            get
            {
                return queryIndex;
            }
        }

        public int WordIndex
        {
            get
            {
                return wordIndex;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }


        public WordPosition(int wordIndex, int queryIndex, int length)
        {
            this.wordIndex = wordIndex;
            this.queryIndex = queryIndex;
            this.length = length;
        }


        public int CompareTo(WordPosition otherWord)
        {
            if (otherWord == null) return -1;
            return wordIndex.CompareTo(otherWord.wordIndex);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }
            WordReference other = (WordReference)obj;
            if (this.wordIndex == other.WordIndex) return true; else return false;
        }

        public override int GetHashCode()
        {
            return WordIndex;
        }

        public static bool operator ==(WordPosition left, WordPosition right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(WordPosition left, WordPosition right)
        {
            return !(left == right);
        }

        public static bool operator <(WordPosition left, WordPosition right)
        {
            if (right == null) return false;
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(WordPosition left, WordPosition right)
        {
            if (right == null) return false;
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(WordPosition left, WordPosition right)
        {
            if (right == null) return false;
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(WordPosition left, WordPosition right)
        {
            if (right == null) return false;
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
