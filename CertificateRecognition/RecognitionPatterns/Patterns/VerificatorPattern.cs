using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CertificateRecognizer.Extensions;
using CertificateRecognizer.Model;
using CertificateService;
using FuzzyString;

namespace CertificateRecognizer.RecognitionPatterns.Patterns
{
    internal class VerificatorPattern : PatternBase
    {
        #region fields

        private const string CertNoTitle = "Certificate N";
        private const string ProcessTitle = "Welding process ( es ) :";
        private const string DetailsTitle = "Test piece";
        private const string DateTitle = "Validity of qual . until * :";

        #endregion

        #region constructors

        public VerificatorPattern(IWeldingProcessStorage processStorage) : base(processStorage)
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
                    b.Paragraphs.Any(p => 
                        b.Bounds.LeftBottom.Y < page.QuarterHeight && 
                        b.Bounds.LeftTop.X > page.TwoThirdsWidth &&
                        CertNoTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

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
                    b.Paragraphs.Any(p => 
                        b.Bounds.LeftTop.X < page.QuarterWidth &&
                        b.Bounds.LeftTop.Y < page.ThirdHeight &&
                        ProcessTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (processTitleBlock == null)
                    return null;

                var paragraphs = processTitleBlock.Paragraphs;

                var titleParagraph = paragraphs.First(p =>
                    ProcessTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions));

                var blocksToTheRight = titleParagraph.GetObjectsOnTheRight(paragraphs, true);
                
                var detailsBlock = paragraphs.FirstOrDefault(b => 
                    b.Bounds.LeftTop.X < page.ThirdWidth &&
                    b.Bounds.LeftTop.Y < page.ThirdHeight &&
                    DetailsTitle.ApproximatelyEquals(b.ToString(), Tolerance, ComparisonOptions));

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
                    b.Paragraphs.Any(p => 
                        b.Bounds.LeftTop.Y > page.HalfHeight && b.Bounds.LeftBottom.Y < page.ThreeQuarterHeight && 
                        b.Bounds.LeftBottom.X > page.HalfWidth && 
                        DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (block == null)
                    return default(DateTime);

                var paragraph = block.Paragraphs.First(p =>
                    DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions));

                string dateString = null;
                
                var dateObject = paragraph.GetObjectOnTheRight(block.Paragraphs, true);
                
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