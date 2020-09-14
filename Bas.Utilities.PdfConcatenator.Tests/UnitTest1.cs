
namespace Bas.Utilities.PdfConcatenator.Tests
{
    using System;
    using System.IO;
    using Bas.Utilities;
    using Bas.Utilities.PdfConcatenator.Tests.Fixtures;
    using Xunit;

    public class UnitTest1 : SetupTests, IClassFixture<PdfConcatenatorFixture>
    {
        private readonly PdfConcatenator pdfConcatenator;

        public UnitTest1(PdfConcatenatorFixture pdfConcatenatorFixture)
        {
            this.pdfConcatenator = pdfConcatenatorFixture.pdfConcatenator;
        }

        [Fact]
        public void Test1()
        {
            using (var stream = new FileStream(this.TestFile1, FileMode.Open, FileAccess.Read))
            using (var stream2 = new FileStream(this.TestFile2, FileMode.Open, FileAccess.Read))
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                this.pdfConcatenator.Add(ms.ToArray());

                using var ms2 = new MemoryStream();
                stream2.CopyTo(ms2);
                this.pdfConcatenator.Add(ms2.ToArray());

                var documentOutput = this.pdfConcatenator.GetDocument();
                File.WriteAllBytes($"{this.ArtifactsFolder}/TestOutput{DateTime.Now:yyyyMMddHHmmssfff}.pdf",documentOutput);
            }
        }
    }
}
