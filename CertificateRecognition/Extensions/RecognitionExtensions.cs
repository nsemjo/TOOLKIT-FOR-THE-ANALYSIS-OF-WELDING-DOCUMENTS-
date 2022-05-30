using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CertificateRecognizer.Model;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Extensions
{
    internal static class RecognitionExtensions
    {
        public static string FromSymbols(this Word word)
        {
            return string.Join("", word.Symbols.Select(s => s.Text));
        }
        
        public static string FromWords(this Paragraph paragraph)
        {
            return string.Join(" ", paragraph.Words.Select(w => w.FromSymbols()));
        }

        public static Point ToPoint(this Vertex vertex)
        {
            return new Point(vertex.X, vertex.Y);
        }

        public static Rect ToRect(this BoundingPoly bounds)
        {
            return new Rect(bounds.Vertices.Select(v => v.ToPoint()));
        }

        public static bool IsBetween(this int value, int min, int max)
        {
            return min <= value && value <= max;
        }

        public static IEnumerable<SimpleWord> GetTopRow(this IEnumerable<SimpleWord> words)
        {
            var ordered = words.OrderBy(w => w.Bounds.LeftTop.Y).ToList();
            if (!ordered.Any()) return new List<SimpleWord>();
            var firstTop = ordered[0].Bounds.LeftTop.Y;
            var firstBottom = ordered[0].Bounds.LeftBottom.Y;
            var firstCenter = (firstTop + firstBottom) / 2;
            var row = ordered.Where(w => w.Bounds.LeftTop.Y < firstCenter);
            return row.OrderBy(w => w.Bounds.LeftTop.X);
        }

        public static IEnumerable<SimpleWord> GetRowAt(this SimpleParagraph paragraph, int index)
        {
            var words = paragraph.Words.ToList();
            var counter = 0;
            while (words.Any())
            {
                var row = words.GetTopRow().ToList();
                if (counter == index) return row;
                words = words.Except(row).ToList();
                counter++;
            }

            return null;
        }

        public static int GetRowCount(this SimpleParagraph paragraph)
        {
            var words = paragraph.Words.ToList();
            var counter = 0;
            while (words.Any())
            {
                var row = words.GetTopRow().ToList();
                words = words.Except(row).ToList();
                counter++;
            }
            return counter;
        }

        public static IEnumerable<PositionedObject> GetObjectsOnTheRight(this PositionedObject currentObject, IEnumerable<PositionedObject> objects, bool includeOverlapping = false)
        {
            var ordered = objects.Except(new []{currentObject}).OrderBy(o => o.Bounds.LeftTop.Y);
            
            var currentLeftTopY = currentObject.Bounds.LeftTop.Y;
            var currentLeftBottomY = currentObject.Bounds.LeftBottom.Y;

            var predicate = !includeOverlapping
                ? (Func<PositionedObject, bool>) (o => o.Bounds.LeftTop.Y >= currentLeftTopY && o.Bounds.LeftBottom.Y <= currentLeftBottomY)
                : (Func<PositionedObject, bool>) (o => o.Bounds.LeftTop.Y.IsBetween(currentLeftTopY, currentLeftBottomY) || o.Bounds.LeftBottom.Y.IsBetween(currentLeftTopY, currentLeftBottomY));

            var sameRow = ordered.Where(predicate);
            var orderedSameRow = sameRow.OrderBy(o => o.Bounds.LeftTop.X);
            var res = orderedSameRow.Where(o => o.Bounds.LeftTop.X >= currentObject.Bounds.RightTop.X);
            return res;
        }

        public static PositionedObject GetObjectOnTheRight(this PositionedObject currentObject, IEnumerable<PositionedObject> objects, bool includeOverlapping = false)
        {
            return currentObject.GetObjectsOnTheRight(objects, includeOverlapping).FirstOrDefault();
        }

        public static IEnumerable<PositionedObject> GetObjectsBelow(this PositionedObject currentObject, IEnumerable<PositionedObject> objects, bool includeOverlapping = false)
        {
            var ordered = objects.Except(new []{currentObject}).OrderBy(o => o.Bounds.LeftTop.X);

            var currentLeftTopX = currentObject.Bounds.LeftTop.X;
            var currentRightTopX = currentObject.Bounds.RightTop.X;
            
            var predicate = !includeOverlapping
                ? (Func<PositionedObject, bool>) (o => o.Bounds.LeftTop.X >= currentLeftTopX && o.Bounds.RightTop.X <= currentRightTopX)
                : (Func<PositionedObject, bool>) (o => o.Bounds.LeftTop.X.IsBetween(currentLeftTopX, currentRightTopX) || o.Bounds.RightTop.X.IsBetween(currentLeftTopX, currentRightTopX));
            
            var sameColumn = ordered.Where(predicate);
            var orderedSameColumn = sameColumn.OrderBy(o => o.Bounds.LeftTop.Y);
            var res = orderedSameColumn.Where(o => o.Bounds.LeftTop.Y >= currentObject.Bounds.LeftBottom.Y);
            return res;
        }

        public static PositionedObject GetObjectBelow(this PositionedObject currentObject, IEnumerable<PositionedObject> objects, bool includeOverlapping = false)
        {
            return currentObject.GetObjectsBelow(objects, includeOverlapping).FirstOrDefault();
        }
    }
}