using Myriad.Library;

namespace Myriad.Parser
{
    public class StringRange
    {
        int start = Ordinals.first;
        int end = Ordinals.first;
        int max;
        public StringRange()
        {
        }
        public StringRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
        public static StringRange InvalidRange
        {
            get
            {
                StringRange result = new StringRange();
                result.Invalidate();
                return result;
            }
        }
        public void MoveStartTo(int start)
        {
            this.start = start;
        }

        public void BumpStart()
        {
            start++;
        }

        public void MoveEndTo(int end)
        {
            this.end = end;
        }

        public void BumpEnd()
        {
            end++;
        }

        public void GoToNextStartPosition()
        {
            start= end + 1;
        }

        public void Reset()
        {
            start = Ordinals.first;
            end = Ordinals.first;
        }

        public void SetLimit(int max)
        {
            start = Ordinals.first;
            end = Ordinals.first;
            this.Max = max;
        }

        public bool Invalid
        {
            get
            {
                return !Valid;
            }
        }

        public bool Valid
        {
            get
            {
                return (start <= end) && (start >= 0) && (end >= 0) && (start <= Max) && (end <= Max);
            }
        }

        public int Start
        {
            get
            {
                return start;
            }
        }

        public int End
        {
            get
            {
                return end;
            }
        }

        public int Length
        {
            get
            {
                return end - start + 1;
            }
        }

        public int ExclusiveLength
        {
            get
            {
                return end - start;
            }
        }

        public int Space
        {
            get
            {
                return Max - end;
            }
        }

        public bool AtLimit
        {
            get
            {
                return end >= Max;
            }
        }

        public int Max { get => max; set => max = value; }

        public void Copy(StringRange source)
        {
            this.start = source.start;
            this.end = source.end;
            this.max = source.max;
        }

        public void MoveEndToLimit()
        {
            end = max;
        }

        public void Invalidate()
        {
            start = -1;
        }

        public void PullEnd()
        {
            end--;
        }
    }

}
