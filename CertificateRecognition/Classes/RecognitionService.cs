using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CertificateRecognizer.Infrastructure;
using CertificateRecognizer.Model;
using CertificateRecognizer.RecognitionPatterns;
using CertificateService;
using Domain.Classes;
using Domain.Model.Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NReco.PdfRenderer;
using Utils;

namespace CertificateRecognizer.Classes
{
    internal sealed class RecognitionService : IRecognitionService
    {
        #region fields

        private const string PdfExtension = ".PDF";

        private static readonly string[] ValidImageExtensions =
        {
            ".PNG",
            ".JPG",
            ".JPEG",
            ".TIFF",
            ".BMP",
        };

        private long _maxFileSize;
        private const int MaxPagesCount = 3;

        private readonly ImageAnnotatorClient _client;
        private readonly PdfToImageConverter _converter;

        private readonly IRecognitionPattern _pattern;

        private readonly ILogger<RecognitionService> _logger;

        #endregion

        #region constructors

        public RecognitionService(IWeldingProcessStorage processStorage, IConfiguration configuration, ILogger<RecognitionService> logger)
        {
            _logger = logger;
            var credentialFileName = configuration["GoogleCredentialFileName"];
            var asm = Assembly.GetExecutingAssembly();
            var path = Path.Combine(Path.GetDirectoryName(asm.Location), credentialFileName);
            var cred = GoogleCredential.FromFile(path).CreateScoped(ImageAnnotatorClient.DefaultScopes);
            var channel = new Channel(ImageAnnotatorClient.DefaultEndpoint.Host,
                ImageAnnotatorClient.DefaultEndpoint.Port, cred.ToChannelCredentials());
            _client = ImageAnnotatorClient.Create(channel);
            _converter = new PdfToImageConverter();
            _pattern = new CompositePattern(processStorage);
            _maxFileSize = Convert.ToInt64(configuration["SizeLimits:Certificate"]) * 1024 * 1024;
        }

        #endregion

        #region properties



        #endregion

        #region public methods

        public async Task<Certificate> RecognizeFileAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;
            
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > _maxFileSize)
                return null;

            var content = await File.ReadAllBytesAsync(path);
            var extension = Path.GetExtension(path).ToUpper();

            if (string.Compare(extension, PdfExtension, StringComparison.Ordinal) == 0)
                return await RecognizePdfAsync(content);

            if (ValidImageExtensions.Any(e => string.Compare(e, extension, StringComparison.Ordinal) == 0))
                return await RecognizeImageAsync(content, path);
            
            return null;
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods

        private async Task<Certificate> RecognizeImageAsync(byte[] content, string path)
        {
            var image = Image.FromBytes(content);
            
            var text = await _client.DetectDocumentTextAsync(image).ConfigureAwait(false);
            if (text == null) return null;
            var simpleText = new SimpleText(text);

            var cert = _pattern.TryRecognize(simpleText) ?? new Certificate();

            try
            {
                cert.Base64Thumbnail = ImageConverter.CreateThumbnailBase64(path);
                cert.Base64Pages.Add(ImageConverter.ConvertPictureToBase64(path));
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, "Error while editing picture");
            }
            
            return cert;
        }

        public async Task<Certificate> RecognizePdfAsync(byte[] content)
        {
            string[] files;
            using (var stream = new MemoryStream(content, false))
            {
                var tempDir = Path.GetTempPath();
                files = await Task.Run(() => _converter.GenerateImages(stream, 1, MaxPagesCount, ImageFormat.Jpeg, tempDir));
            }
            
            TextAnnotation text = null;

            string base64Thumbnail = null;
            var base64Pages = new List<string>();
            
            foreach (var path in files)
            {
                Image image;
                using (var file = new TempFile(path))
                {
                    image = Image.FromFile(file.Path);

                    if (base64Thumbnail == null)
                        base64Thumbnail = ImageConverter.CreateThumbnailBase64(files.FirstOrDefault());
                    
                    base64Pages.Add(ImageConverter.ConvertPictureToBase64(path));
                }
                
                TextAnnotation bufferText = null;
                try
                {
                    bufferText = await _client.DetectDocumentTextAsync(image).ConfigureAwait(false);
                }
                catch
                {
                    //ignore
                }

                if (text == null)
                {
                    text = bufferText;
                }
                else
                {
                    if (bufferText != null)
                        text.Pages.AddRange(bufferText.Pages);
                }
#if DEBUG
                DebugDrawer.SaveToFile(image, text);
#endif
            }

            SimpleText simpleText = null;
            if (text != null)
                simpleText = new SimpleText(text);

            var cert = _pattern.TryRecognize(simpleText) ?? new Certificate();
            
            cert.Base64Thumbnail = base64Thumbnail;
            cert.Base64Pages.AddRange(base64Pages);

            return cert;
        }

        #endregion
    }
}