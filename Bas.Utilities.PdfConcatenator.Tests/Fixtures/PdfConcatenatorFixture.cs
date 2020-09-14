namespace Bas.Utilities.PdfConcatenator.Tests.Fixtures
{
    using System;
    using Bas.Utilities;

    public class PdfConcatenatorFixture : SetupTests
    {
        public PdfConcatenator pdfConcatenator;

        public PdfConcatenatorFixture()
        {
            this.pdfConcatenator = new PdfConcatenator();
        }
    }
}