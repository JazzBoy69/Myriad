

namespace Myriad.Library
{
    //todo move to Feliciana Library
    public class ReadOnlyStringRange
    {
        protected int start = Ordinals.first;
        protected int end = Result.notfound;
        protected int max;

        public ReadOnlyStringRange()
        {
        }
        public ReadOnlyStringRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public static ReadOnlyStringRange InvalidRange
        {
            get
            {
                ReadOnlyStringRange result = new ReadOnlyStringRange();
                result.Invalidate();
                return result;
            }
        }

        public void Invalidate()
        {
            start = -1;
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
    }

    public class StringRange :ReadOnlyStringRange
    {
        public StringRange() : base()
        {
        }
        public StringRange(int start, int end) : base(start, end)
        {
        }

        public new static StringRange InvalidRange
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


        public void PullEnd()
        {
            end--;
        }
    }

}
