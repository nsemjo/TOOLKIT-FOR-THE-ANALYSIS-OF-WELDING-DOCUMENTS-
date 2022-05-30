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
    internal class BureauVeritasPattern : PatternBase
    {
        #region fields

        private const string CertNoTitle = "Certificate N";
        private const string ProcessTitle = "Welding process";
        private const string DetailsTitle = "Weld test details";
        private const string DateTitle = "Valid until";
        
        #endregion

        #region constructors

        public BureauVeritasPattern(IWeldingProcessStorage processStorage) : base(processStorage)
        {
        }

        #endregion

        #region properties



        #endregion

        #region public methods

        #endregion

        #region protected methods



        #endregion

        #region private methods

        protected override string GetCertNo(SimplePage page)
        {
            try
            {
                var certNoBlock = page.Blocks.FirstOrDefault(b => 
                    b.Bounds.LeftTop.X > page.HalfWidth && b.Bounds.LeftBottom.Y <= page.QuarterHeight && 
                    b.Paragraphs.Any(p => CertNoTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                var row = certNoBlock?.Paragraphs.FirstOrDefault()?.Words.GetTopRow().ToList();
                if (row == null)
                    return null;
                
                string certNo;
                
                if (row.Count > 6)
                {
                    certNo = string.Join(" ", row.TakeLast(4).Select(w => w.Word));
                }
                else
                {
                    if (!(certNoBlock.GetObjectOnTheRight(page.Blocks) is SimpleBlock valueBlock))
                        return null;
                    
                    certNo = valueBlock.Paragraphs.FirstOrDefault()?.ToString().Replace(" ", "");
                
                    if (string.IsNullOrWhiteSpace(certNo))
                        return null;
                }

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
                var block = page.Blocks.FirstOrDefault(b => b.Paragraphs.Any(p =>
                    ProcessTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (block == null)
                    return null;

                var rowBlocks = block.GetObjectsOnTheRight(page.Blocks).ToList();
                
                var detailsBlock = page.Blocks.FirstOrDefault(b =>
                    b.Bounds.LeftTop.X >= page.ThirdWidth && b.Bounds.RightTop.X <= page.TwoThirdsWidth &&
                    b.Paragraphs.Any(p => DetailsTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));
                if (detailsBlock != null)
                    rowBlocks = detailsBlock.GetObjectsBelow(rowBlocks).ToList();

                string process = null;
                
                foreach (var rowBlock in rowBlocks)
                {
                    if (!(rowBlock is SimpleBlock processBlock))
                        continue;

                    process = processBlock.Paragraphs.FirstOrDefault()?.ToString().Replace(" ", "");

                    if (!string.IsNullOrWhiteSpace(process) && ProcessRegex.IsMatch(process))
                        break;
                }
                
                if (string.IsNullOrWhiteSpace(process))
                    return null;
                
                return process;
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
                    b.Bounds.LeftTop.X >= page.QuarterWidth && b.Bounds.RightTop.X <= page.ThreeQuarterWidth && 
                    b.Bounds.LeftTop.Y >= page.ThreeQuarterHeight &&
                    b.Paragraphs.Any(p => DateTitle.ApproximatelyEquals(p.ToString(), Tolerance, ComparisonOptions)));

                if (block == null)
                    return default(DateTime);

                string dateString = null;

                if (DateRegex.Match(block.ToString().Replace(" ", "")) is Match match && match.Success)
                {
                    dateString = match.Value;
                }
                else
                {
                    var columnBlocks = block.GetObjectsBelow(page.Blocks, true).ToList();

                    foreach (var columnBlock in columnBlocks)
                    {
                        if (!(columnBlock is SimpleBlock dateBlock))
                            continue;

                        dateString = dateBlock.Paragraphs.FirstOrDefault()?.ToString().Replace(" ", "");

                        if (!string.IsNullOrWhiteSpace(dateString) && DateRegex.IsMatch(dateString))
                            break;
                    }
                }

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
    }
}