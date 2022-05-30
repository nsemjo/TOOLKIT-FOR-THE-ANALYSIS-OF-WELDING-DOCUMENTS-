using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CertificateRecognizer.Model;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Orientation
{
    internal static class OrientationHelper
    {
        #region fields

        private const int MinWordLength = 5;

        #endregion

        #region constructors

        

        #endregion

        #region properties



        #endregion

        #region public methods

        public static Orientation GetOrientation(Page page)
        {
            var counters = new Dictionary<Orientation, int>
            {
                [Orientation.Normal] = 0,
                [Orientation.Rotation90] = 0,
                [Orientation.Rotation180] = 0,
                [Orientation.Rotation270] = 0,
            };
            
            foreach (var block in page.Blocks)
            foreach (var paragraph in block.Paragraphs)
            foreach (var word in paragraph.Words)
            {
                if (word.Symbols.Count < MinWordLength) 
                    continue;

                var (centerX, centerY) = (word.BoundingBox.Vertices.Average(v => v.X),
                    word.BoundingBox.Vertices.Average(v => v.Y));

                var x0 = word.BoundingBox.Vertices[0].X;
                var y0 = word.BoundingBox.Vertices[0].Y;

                if (x0 < centerX) {
                    if (y0 < centerY) {
                        //       0 -------- 1
                        //       |          |
                        //       3 -------- 2
                        counters[Orientation.Normal]++;
                    } else {
                        //       1 -------- 2
                        //       |          |
                        //       0 -------- 3
                        counters[Orientation.Rotation270]++;
                    }
                } else {
                    if (y0 < centerY) {
                        //       3 -------- 0
                        //       |          |
                        //       2 -------- 1
                        counters[Orientation.Rotation90]++;
                    } else {
                        //       2 -------- 3
                        //       |          |
                        //       1 -------- 0
                        counters[Orientation.Rotation180]++;
                    }
                }
            }

            return counters.OrderByDescending(p => p.Value).First().Key;
        }

        public static void CorrectOrientation(SimplePage page, Orientation orientation)
        {
            if (orientation == Orientation.Normal) return;
            
            var oldPageCenter = new Point((int) page.HalfWidth, (int) page.HalfHeight);

            CorrectPageDimensions(page, orientation);
            
            var newPageCenter = new Point((int) page.HalfWidth, (int) page.HalfHeight);

            foreach (var block in page.Blocks)
            {
                RotateObject(block, orientation, oldPageCenter, newPageCenter);
                foreach (var paragraph in block.Paragraphs)
                {
                    RotateObject(paragraph, orientation, oldPageCenter, newPageCenter);
                    foreach (var word in paragraph.Words)
                    {
                        RotateObject(word, orientation, oldPageCenter, newPageCenter);
                    }
                }
            }
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods

        private static void CorrectPageDimensions(SimplePage page, Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Rotation90:
                case Orientation.Rotation270:
                    var t = page.Width;
                    page.Width = page.Height;
                    page.Height = t;
                    break;
            }
        }

        private static void RotateObject(PositionedObject obj, Orientation orientation, Point oldPageCenter, Point newPageCenter)
        {
            var oldBounds = obj.Bounds;
            var newBounds = new Rect();
            double rotationAngle = 0;
            
            switch (orientation)
            {
                case Orientation.Rotation90:
                    rotationAngle = -90;
                    break;
                case Orientation.Rotation180:
                    rotationAngle = 180;
                    break;
                case Orientation.Rotation270:
                    rotationAngle = 90;
                    break;
            }
            
            newBounds.LeftTop = RotatePoint(oldBounds.LeftTop, oldPageCenter, rotationAngle, newPageCenter);
            newBounds.RightTop = RotatePoint(oldBounds.RightTop, oldPageCenter, rotationAngle, newPageCenter);
            newBounds.RightBottom = RotatePoint(oldBounds.RightBottom, oldPageCenter, rotationAngle, newPageCenter);
            newBounds.LeftBottom = RotatePoint(oldBounds.LeftBottom, oldPageCenter, rotationAngle, newPageCenter);

            obj.Bounds = newBounds;
        }

        /// <summary>
        /// Rotates one point around another and corrects coordinates to new center position
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="oldCenterPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <param name="newCenterPoint">The new center point.</param>
        /// <returns>Rotated point</returns>
        private static Point RotatePoint(Point pointToRotate, Point oldCenterPoint, double angleInDegrees, Point newCenterPoint)
        {
            var angleInRadians = angleInDegrees * (Math.PI / 180);
            var cosTheta = Math.Cos(angleInRadians);
            var sinTheta = Math.Sin(angleInRadians);

            const double delta = 0.01;
            if (Math.Abs(cosTheta) < delta)
                cosTheta = 0;
            if (Math.Abs(sinTheta) < delta)
                sinTheta = 0;

            var x = (int) (cosTheta * (pointToRotate.X - oldCenterPoint.X) -
                           sinTheta * (pointToRotate.Y - oldCenterPoint.Y) + oldCenterPoint.X);
            var y = (int) (sinTheta * (pointToRotate.X - oldCenterPoint.X) +
                           cosTheta * (pointToRotate.Y - oldCenterPoint.Y) + oldCenterPoint.Y);
            
            var dx = newCenterPoint.X - oldCenterPoint.X;
            var dy = newCenterPoint.Y - oldCenterPoint.Y;

            x += dx;
            y += dy;

            return new Point
            {
                X = x,
                Y = y,
            };
        }
        
        #endregion
    }
}