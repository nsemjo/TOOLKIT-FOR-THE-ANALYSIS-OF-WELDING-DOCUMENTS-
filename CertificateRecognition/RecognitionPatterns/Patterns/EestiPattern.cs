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
    internal class EestiPattern : PatternBase
    {
        #region fields

        private const string CertNoTitle = "Welder ' s qualification test certificate";
        private const string ProcessTitle = "Keevituse protsess / Welding process ( es )";
        private const string DetailsTitle = "Keevituse andmed / Test piece";
        private const string DateTitle = "Kehtiv kuni / Validity of approval until :";

        #endregion

        #region constructors

        public EestiPattern(IWeldingProcessStorage processStorage) : base(processStorage)
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
                    b.Bounds.LeftTop.X > page.QuarterWidth && b.Bounds.RightTop.X < page.ThreeQuarterWidth &&
                    b.Paragraphs.Any(p => CertNoTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                var paragraph = certNoBlock?.Paragraphs.LastOrDefault();
                if (paragraph == null) return null;

                var certNo = string.Join("", paragraph.Words.Select(w => w.ToString()));
                
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
                    b.Bounds.RightTop.X < page.HalfWidth &&
                    b.Bounds.LeftTop.Y < page.HalfHeight &&
                    b.Paragraphs.Any(p => ProcessTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (processTitleBlock == null)
                    return null;

                var blocksToTheRight = processTitleBlock.GetObjectsOnTheRight(page.Blocks, true);

                var processBlock = blocksToTheRight.OrderBy(b => b.Bounds.LeftTop.Y).FirstOrDefault();

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
                    b.Bounds.LeftTop.Y > page.HalfHeight && b.Bounds.LeftBottom.Y < page.ThreeQuarterHeight && 
                    b.Paragraphs.Any(p => p.Words.Length > 5 && DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (block == null)
                    return default(DateTime);

                var paragraph = block.Paragraphs.First(p =>
                    DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions));

                string dateString = null;
                
                var dateObject = paragraph.GetObjectOnTheRight(page.Blocks, true);
                
                if (DateRegex.Matches(dateObject.ToString().Replace(" ", "")).LastOrDefault() is Match match && match.Success)
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