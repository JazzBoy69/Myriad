using System;
using System.Collections.Generic;
using System.Text;

namespace Myriad.Library
{
    public class Range
    {
        public const int invalidID = Numbers.nothing;
        internal static Range InvalidRange = new Range(invalidID, invalidID);

        KeyID start;
        KeyID end;

        public Range(int? startID, int? endID)
        {
            this.start = new KeyID(startID);
            this.end = new KeyID(endID);
        }

        public Range((int startID, int endID) input)
        {
            this.start = new KeyID(input.startID);
            this.end = new KeyID(input.endID);
        }

        internal Range(Tuple<int, int> tuple)
        {
            this.start = new KeyID(tuple.Item1);
            this.end = new KeyID(tuple.Item2);
        }

        public Range(int id)
        {
            start = new KeyID(id);
            end = new KeyID(id);
        }
        private static bool GoodStrings(string start, string end)
        {
            return !((String.IsNullOrEmpty(start)) && (String.IsNullOrEmpty(end)));
        }
        public Range(string start, string end)
        {
            if (!GoodStrings(start, end))
            {
                this.start = new KeyID(invalidID);
                this.end = new KeyID(invalidID);
            }
            else
            {
                this.start = new KeyID(start);
                this.end = new KeyID(end);
            }
        }

        public int StartID
        {
            get
            {
                return start.ID;
            }
        }
        public int EndID
        {
            get
            {
                return end.ID;
            }
        }

        internal bool Contains(Range targetRange)
        {
            return (targetRange.start.ID >= start.ID) && (targetRange.end.ID <= end.ID);
        }

        internal void ExtendTo(int end)
        {
            this.end.Set(end);
        }


        internal bool Equals(Range otherRange)
        {
            return start.ID == otherRange.start.ID && end.ID == otherRange.end.ID;
        }

        public int Length 
        { 
            get 
            { 
                return end.ID - start.ID + 1; 
            } 
        }

        public (int startID, int endID) Key 
        { 
            get 
            { 
                return (start.ID, end.ID); 
            } 
        }

        internal void Invalidate()
        {
            start = new KeyID(invalidID);
            end = new KeyID(invalidID);

        }

        internal static bool InRange(Range range, Range targetRange)
        {
            if ((range == null) || (targetRange == null)) return false;
            return (targetRange.start.ID <= range.end.ID) && (targetRange.end.ID >= range.start.ID);
        }  
    }
}