using System;
using System.Linq;
using System.Text.RegularExpressions;
using CertificateRecognizer.Model;
using CertificateService;
using Domain.Model.Certificates;
using FuzzyString;

namespace CertificateRecognizer.RecognitionPatterns.Patterns
{
    internal abstract class PatternBase : IRecognitionPattern
    {
        #region fields

        protected const string ProcessPattern = @"\d{3}(\/\d{3})?";

        protected readonly IWeldingProcessStorage _processStorage;

        protected readonly FuzzyStringComparisonOptions[] ComparisonOptions;

        protected readonly FuzzyStringComparisonTolerance Tolerance;

        protected readonly Regex ProcessRegex;
        protected readonly Regex DateRegex;

        #endregion

        #region constructors

        protected PatternBase(IWeldingProcessStorage processStorage)
        {
            _processStorage = processStorage;
            
            ComparisonOptions = new []
            {
                FuzzyStringComparisonOptions.UseOverlapCoefficient,
                FuzzyStringComparisonOptions.UseLongestCommonSubstring,
                FuzzyStringComparisonOptions.UseLongestCommonSubsequence,
                FuzzyStringComparisonOptions.UseSorensenDiceDistance,
                FuzzyStringComparisonOptions.UseRatcliffObershelpSimilarity
            };

            Tolerance = FuzzyStringComparisonTolerance.Normal;
            
            ProcessRegex = new Regex(ProcessPattern);
            DateRegex = new Regex(DatePattern);
        }

        #endregion

        #region properties

        protected virtual string DatePattern => @"\d{2}\/\d{2}\/\d{4}";
        protected virtual string DateFormat => "dd/MM/yyyy";

        #endregion

        #region public methods

        public virtual Certificate TryRecognize(SimpleText text)
        {
            try
            {
                if (text.Pages.Length > 1) return null;
                var page = text.Pages[0];

                var cert = new Certificate
                {
                    CertNo = GetCertNo(page),
                    ProcessId = TryGetProcess(GetProcess(page)),
                    ExpiryDate = GetValidDate(page),
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

        protected abstract string GetCertNo(SimplePage page);
        protected abstract string GetProcess(SimplePage page);
        protected abstract DateTime GetValidDate(SimplePage page);

        protected Guid TryGetProcess(string code)
        {
            if (code != null && code.Any(c => !char.IsDigit(c)))
            {
                var symbol = code.First(c => !char.IsDigit(c));
                code = code.Substring(0, code.IndexOf(symbol));
            }

            var process = _processStorage.GetProcess(code);

            return process?.Id ?? Guid.Empty;
        }

        #endregion

        #region private methods



        #endregion
    }
}