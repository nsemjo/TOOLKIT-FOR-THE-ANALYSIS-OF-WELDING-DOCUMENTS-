using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CertificateRecognizer.Extensions;
using CertificateRecognizer.Model;
using CertificateService;
using Domain.Model.Certificates;
using FuzzyString;

namespace CertificateRecognizer.RecognitionPatterns.Patterns
{
    internal class DnvGlPattern : PatternBase
    {
        #region fields

        private const string CertNoTitle = "Certificate No :";
        private const string ProcessTitle = "Welding process";
        private const string DetailsTitle = "Weld test details";
        private const string DateTitle = "This Certificate is valid until";

        #endregion

        #region constructors

        public DnvGlPattern(IWeldingProcessStorage processStorage) : base(processStorage)
        {
        }

        #endregion

        #region properties

        protected override string DatePattern => @"\d{4}-\d{2}-\d{2}";
        protected override string DateFormat => @"yyyy-MM-dd";

        #endregion

        #region public methods

        public override Certificate TryRecognize(SimpleText text)
        {
            try
            {
                if (text.Pages.Length < 2) return null;
                
                var page0 = text.Pages[0];
                var page1 = text.Pages[1];

                var cert = new Certificate
                {
                    CertNo = GetCertNo(page0),
                    ProcessId = TryGetProcess(GetProcess(page0)),
                    ExpiryDate = GetValidDate(page1),
                };

                return cert;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region protected methods

        protected override string GetCertNo(SimplePage page)
        {
            try
            {
                var certNoBlock = page.Blocks.FirstOrDefault(b => 
                    b.Bounds.LeftTop.X > page.HalfWidth && b.Bounds.LeftBottom.Y <= page.QuarterHeight && 
                    b.Paragraphs.Any(p => CertNoTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                var paragraph = certNoBlock?.Paragraphs.FirstOrDefault();
                var rowCount = paragraph?.GetRowCount();
                if (rowCount != 4) return null;

                var row = paragraph.GetRowAt(1);
                if (row == null)
                    return null;
                
                var certNo = string.Join("", row.Select(w => w.Word));
                
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
                    b.Bounds.LeftTop.Y > page.ThirdHeight && b.Bounds.LeftBottom.Y < page.TwoThirdsHeight &&
                    b.Paragraphs.Any(p => ProcessTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (processTitleBlock == null)
                    return null;

                var blocksToTheRight = processTitleBlock.GetObjectsOnTheRight(page.Blocks, true);
                
                var detailsBlock = page.Blocks.FirstOrDefault(b => 
                    b.Bounds.LeftTop.X > page.QuarterWidth && b.Bounds.RightTop.X < page.HalfWidth &&
                    b.Bounds.LeftTop.Y > page.ThirdHeight && b.Bounds.LeftBottom.Y < page.TwoThirdsHeight &&
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
                    b.Bounds.LeftTop.Y > page.ThirdHeight && b.Bounds.LeftBottom.Y < page.TwoThirdsHeight && 
                    b.Bounds.RightTop.X < page.HalfWidth &&
                    b.Paragraphs.Any(p => DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (block == null)
                    return default(DateTime);

                string dateString = null;

                if (DateRegex.Match(block.ToString().Replace(" ", "")) is Match match && match.Success)
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