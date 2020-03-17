using Myriad.Library;

namespace Myriad.Parser
{
    public class StringRange
    {
        int start = Ordinals.first;
        int end = Ordinals.first;
        int max;
        internal static StringRange InvalidRange
        {
            get
            {
                StringRange result = new StringRange();
                result.Invalidate();
                return result;
            }
        }
        internal void MoveStartTo(int start)
        {
            this.start = start;
        }

        internal void BumpStart()
        {
            start++;
        }

        internal void MoveEndTo(int end)
        {
            this.end = end;
        }

        internal void BumpEnd()
        {
            end++;
        }

        internal void GoToNextStartPosition()
        {
            start= end + 1;
        }

        internal void Reset()
        {
            start = Ordinals.first;
            end = Ordinals.first;
        }

        internal void SetLimit(int max)
        {
            start = Ordinals.first;
            end = Ordinals.first;
            this.Max = max;
        }

        internal bool Invalid
        {
            get
            {
                return !Valid;
            }
        }

        internal bool Valid
        {
            get
            {
                return (start <= end) && (start >= 0) && (end >= 0) && (start <= Max) && (end <= Max);
            }
        }

        internal int Start
        {
            get
            {
                return start;
            }
        }

        internal int End
        {
            get
            {
                return end;
            }
        }

        internal int Length
        {
            get
            {
                return end - start + 1;
            }
        }

        internal int ExclusiveLength
        {
            get
            {
                return end - start;
            }
        }

        internal int Space
        {
            get
            {
                return Max - end;
            }
        }

        internal bool AtLimit
        {
            get
            {
                return end >= Max;
            }
        }

        public int Max { get => max; set => max = value; }

        internal void Copy(StringRange source)
        {
            this.start = source.start;
            this.end = source.end;
            this.max = source.max;
        }

        internal void MoveEndToLimit()
        {
            end = max;
        }

        internal void Invalidate()
        {
            start = -1;
        }

        internal void PullEnd()
        {
            end--;
        }
    }

}
