using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CertificateRecognizer.Extensions;
using CertificateRecognizer.Model;
using CertificateService;
using FuzzyString;

namespace CertificateRecognizer.RecognitionPatterns.Patterns
{
    internal class BtiPattern : PatternBase
    {
        #region fields

        private const string CertNoTitle = "CERTIFICATE";
        private const string ProcessTitle = "Welding process ( es )";
        private const string DetailsTitle = "Weld test details";
        private const string DateTitle = "Validity of approval :";

        #endregion

        #region constructors

        public BtiPattern(IWeldingProcessStorage processStorage) : base(processStorage)
        {
        }

        #endregion

        #region properties

        protected override string DatePattern => @"\d{2}\.\d{2}.\d{4}";
        protected override string DateFormat => @"dd.MM.yyyy";

        #endregion

        #region public methods

        #endregion

        #region protected methods

        protected override string GetCertNo(SimplePage page)
        {
            try
            {
                var certNoBlock = page.Blocks.FirstOrDefault(b =>
                    b.Bounds.LeftBottom.Y < page.QuarterHeight &&
                    b.Bounds.LeftTop.X < page.HalfWidth &&
                    b.Paragraphs.Any(p => CertNoTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                var paragraph = certNoBlock?.Paragraphs.FirstOrDefault();
                if (paragraph == null) return null;

                var row = paragraph.Words.Skip(1).Take(3).ToList();

                var certNo = string.Join(" ", row);

                return certNo;
            }
            catch
            {
                return null;
            }
        }

        protected override string GetProcess(SimplePage page)
        {
            try
            {
                var processTitleBlock = page.Blocks.FirstOrDefault(b => 
                    b.Bounds.RightTop.X < page.ThirdWidth &&
                    b.Bounds.RightBottom.Y > page.QuarterHeight && b.Bounds.RightBottom.Y < page.HalfHeight &&
                    b.Paragraphs.Any(p => ProcessTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (processTitleBlock == null)
                    return null;

                var blocksToTheRight = processTitleBlock.GetObjectsOnTheRight(page.Blocks, true);

                var detailsBlock = page.Blocks.FirstOrDefault(b => 
                    b.Bounds.LeftTop.X > page.QuarterWidth && b.Bounds.RightTop.X < page.HalfWidth &&
                    b.Bounds.LeftTop.Y > page.QuarterHeight && b.Bounds.LeftBottom.Y < page.HalfHeight &&
                    b.Paragraphs.Any(p => DetailsTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                var processBlock = detailsBlock != null
                    ? detailsBlock.GetObjectBelow(blocksToTheRight, true)
                    : blocksToTheRight.FirstOrDefault();

                var process = processBlock?.ToString();
                
                if (!string.IsNullOrWhiteSpace(process) && ProcessRegex.IsMatch(process))
                    return process;
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        protected override DateTime GetValidDate(SimplePage page)
        {
            try
            {
                var block = page.Blocks.FirstOrDefault(b => 
                    b.Bounds.LeftTop.Y > page.HalfHeight && b.Bounds.LeftTop.X > page.HalfWidth && 
                    b.Paragraphs.Any(p => DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (block == null)
                    return default(DateTime);

                var paragraph = block.Paragraphs.First(p =>
                    DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions));

                string dateString = null;
                
                if (DateRegex.Matches(paragraph.ToString().Replace(" ", "")).LastOrDefault() is Match match && match.Success)
                    dateString = match.Value;

                if (string.IsNullOrWhiteSpace(dateString))
                    return default(DateTime);

                var date = DateTime.ParseExact(dateString, DateFormat, new DateTimeFormatInfo());

                return date;
            }
            catch
            {
                return default(DateTime);
            }
        }

        #endregion

        #region private methods



        #endregion
    }
}