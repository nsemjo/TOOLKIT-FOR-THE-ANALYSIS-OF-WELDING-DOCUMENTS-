using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CertificateRecognizer.Model
{
    internal class Rect
    {
        #region fields



        #endregion

        #region constructors

        public Rect(IEnumerable<Point> points)
        {
            var array = points.ToArray();
            LeftTop = array[0];
            RightTop = array[1];
            RightBottom = array[2];
            LeftBottom = array[3];
        }

        public Rect()
        {
            
        }

        #endregion

        #region properties

        public Point LeftTop { get; set; }
        public Point RightTop { get; set; }
        public Point RightBottom { get; set; }
        public Point LeftBottom { get; set; }
        
        #endregion

        #region public methods

        protected bool Equals(Rect other)
        {
            return LeftTop.Equals(other.LeftTop) && RightTop.Equals(other.RightTop) &&
                   RightBottom.Equals(other.RightBottom) && LeftBottom.Equals(other.LeftBottom);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Rect) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LeftTop.GetHashCode();
                hashCode = (hashCode * 397) ^ RightTop.GetHashCode();
                hashCode = (hashCode * 397) ^ RightBottom.GetHashCode();
                hashCode = (hashCode * 397) ^ LeftBottom.GetHashCode();
                return hashCode;
            }
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}