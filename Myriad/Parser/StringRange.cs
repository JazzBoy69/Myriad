using Myriad.Library;

namespace Myriad.AppliedClasses
{
    public class StringRange
    {
        int start = Ordinals.first;
        int end = Ordinals.first;
        int max;

        internal void MoveStartTo(int start)
        {
            this.Start1 = start;
        }

        internal void BumpStart()
        {
            Start1++;
        }

        internal void MoveEndTo(int end)
        {
            this.End1 = end;
        }

        internal void BumpEnd()
        {
            End1++;
        }

        internal void GoToNextStartPosition()
        {
            Start1 = End1 + 1;
        }

        internal void Reset()
        {
            Start1 = Ordinals.first;
            End1 = Ordinals.first;
        }

        internal void SetLimit(int max)
        {
            Start1 = Ordinals.first;
            End1 = Ordinals.first;
            this.Max = max;
        }

        internal bool Invalid
        {
            get
            {
                return ((Start1 >= End1) || (Start1 < 0) || (End1 < 0)) || ((Start1 > Max) || (End1 > Max));
            }
        }

        internal bool Valid
        {
            get
            {
                return (Start1 <= End1) && (Start1 >= 0) && (End1 >= 0) && (Start1 <= Max) && (End1 <= Max);
            }
        }

        internal int Start
        {
            get
            {
                return Start1;
            }
        }

        internal int End
        {
            get
            {
                return End1;
            }
        }

        internal int Length
        {
            get
            {
                return End1 - Start1 + 1;
            }
        }

        internal int ExclusiveLength
        {
            get
            {
                return End1 - Start1;
            }
        }

        internal int Space
        {
            get
            {
                return Max - End1;
            }
        }

        internal bool AtLimit
        {
            get
            {
                return End1 >= Max;
            }
        }

        public int Start1 { get => start; set => start = value; }
        public int End1 { get => end; set => end = value; }
        public int Max { get => max; set => max = value; }

        internal void Copy(StringRange source)
        {
            this.Start1 = source.Start1;
            this.End1 = source.End1;
            this.Max = source.Max;
        }

        internal void MoveEndToLimit()
        {
            End1 = Max;
        }

        internal void Invalidate()
        {
            Start1 = -1;
        }

        internal void PullEnd()
        {
            End1--;
        }
    }

}
