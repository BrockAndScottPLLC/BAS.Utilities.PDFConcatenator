namespace Bas.Utilities.PdfConcatenator.Tests
{
    using System.IO;
    using AutoFixture;

    public class SetupTests
    {
        public SetupTests()
        {
            this.ArtifactsFolder = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Artifacts{Path.DirectorySeparatorChar}";
            this.TestFile1 = $"{this.ArtifactsFolder}Test1.pdf";
            this.TestFile2 = $"{this.ArtifactsFolder}Test2.pdf";
        }

        public IFixture Fixture { get; }

        public string ArtifactsFolder { get; }
        public string TestFile1 { get; }
        public string TestFile2 { get; }

    }
}