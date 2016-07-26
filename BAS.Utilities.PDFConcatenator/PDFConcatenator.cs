/*
 * Author: James Boyd, Description: A library to simplify concatenation of multiple PDFs into a single document and/or create a PDF document from an image.
 * Copyright (C) 2016 Brock & Scott, PLLC
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.

 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text;
using Drawing = System.Drawing;

namespace BAS.Utilities
{

    /// <summary>
    /// Class designed to concatenate multiple pdf documents and/or images together into a single PDF document, which is available as a byte[] or can be written to a file path
    /// </summary>
    public class PDFConcatenator
    {
        /// <summary>
        /// Protected storage for the full bytes of the document
        /// </summary>
        protected byte[] _fileBytes = null;
        /// <summary>
        /// Used to enforce locks on access to _fileBytes
        /// </summary>
        protected object _lock = new object();
        protected const string LICENSE_TXT = @"BAS.Utilities.PDFConcatenator: Copyright (C) 2016 Brock & Scott, PLLC
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.
Please see https://www.gnu.org/licenses/agpl-3.0.txt or LICENSE.txt for the full license.";
        public PDFConcatenator()
        {
            Console.WriteLine(LICENSE_TXT);
        }

        /// <summary>
        /// Creates a new PDF document containing an image, and appends it to the final PDF
        /// </summary>
        /// <param name="img">The image to save within the PDF document</param>
        /// <param name="format">The image format to save the image as</param>
        public void Add(Drawing.Image img, Drawing.Imaging.ImageFormat format = null)
        {
            byte[] buffer = null;
            format = format == null ? Drawing.Imaging.ImageFormat.Jpeg : format;
            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(new Rectangle(img.Width, img.Height)))
                {
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        writer.CompressionLevel = PdfStream.BEST_COMPRESSION;
                        doc.Open();
                        writer.Open();
                        doc.NewPage();
                        writer.CloseStream = false;
                        Image itxtImg = Image.GetInstance(img, format);
                        itxtImg.CompressionLevel = PdfStream.BEST_COMPRESSION;
                        var pageSize = doc.PageSize;
                        itxtImg.Alignment = Element.ALIGN_CENTER;
                        doc.Add(itxtImg);
                    }
                }
                buffer = RetrieveAllBytesFromMemoryStream(ms);
            }
            AddDocumentToFinalDoc(buffer);
        }
        /// <summary>
        /// Adds a PDF document (from the given path) to the final PDF document
        /// </summary>
        /// <param name="path">The path of PDF document to add to the final PDF</param>
        /// <exception cref="IOException">Thrown when the given path is not accessible or cannot be found</exception>
        public void Add(string path)
        {
            if (!File.Exists(path))
                throw new IOException($"Unable to access file at: {path}");
            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    fs.ReadByte();
                }
            }
            catch (Exception)
            {
                throw new IOException($"Unable to access file at: {path}");
            }
            Add(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Adds a PDF document (as an array of bytes) to the final PDF document
        /// </summary>
        /// <param name="document">The bytes of the PDF document to add</param>
        /// <exception cref="ArgumentException">Thrown if the provided byte[] is not a valid PDF document</exception>
        public void Add(byte[] document)
        {
            PdfReader reader = null;
            try
            {
                using (reader = new PdfReader(document)) { }
            }
            catch (Exception)
            {
                throw new ArgumentException("Provided byte[] is not a pdf document");
            }
            AddDocumentToFinalDoc(document);
        }

        /// <summary>
        /// Returns the current PDF document (the concatenated version of all the existing documents)
        /// </summary>
        /// <returns>The concatenated PDF document as an array of bytes</returns>
        public byte[] GetDocument()
        {
            byte[] buffer = null;
            lock (_lock)
            {
                buffer = new byte[_fileBytes.Length];
                Array.Copy(_fileBytes, buffer, _fileBytes.Length);
            }
            return buffer;
        }
        /// <summary>
        /// Writes the current concatenated PDF document to the given path.
        /// </summary>
        /// <param name="path">The path to write the document to</param>
        /// <exception cref="IOException">Thrown if the file at path cannot be read</exception>
        public void WriteDocument(string path)
        {
            byte[] buffer = null;
            buffer = GetDocument();
            try
            {
                File.WriteAllBytes(path, buffer);
            }
            catch (Exception)
            {
                throw new IOException($"Unable to access file: {path}");
            }
        }
        /// <summary>
        /// Appends this document to the existing PDF document
        /// </summary>
        /// <param name="tmpDoc">The document to append</param>
        private void AddDocumentToFinalDoc(byte[] tmpDoc)
        {
            lock (_lock)
            {
                using (var reader = new PdfReader(tmpDoc))
                {
                    PdfReader initialReader = null;
                    if (_fileBytes != null)
                    {
                        initialReader = new PdfReader(_fileBytes);
                    }
                    using (var ms = new MemoryStream())
                    {
                        var doc = new Document();
                        using (var copier = new PdfCopy(doc, ms))
                        {
                            copier.Open();
                            copier.CompressionLevel = PdfStream.BEST_COMPRESSION;
                            doc.Open();
                            copier.CloseStream = false;
                            if (initialReader != null)
                                copier.AddDocument(initialReader);
                            copier.AddDocument(reader);
                        }
                        doc.Close();
                        _fileBytes = RetrieveAllBytesFromMemoryStream(ms);
                    }
                }
            }
        }
        /// <summary>
        /// Retrieve the current bytes from a memory stream
        /// </summary>
        /// <param name="ms">The memory stream to read</param>
        /// <returns>A byte[] representing all the bytes currently held in this memory stream</returns>
        /// <see cref="MemoryStream"/>
        private byte[] RetrieveAllBytesFromMemoryStream(MemoryStream ms)
        {
            byte[] buffer = new byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            for (long i = 0; i < ms.Length; i++)
                buffer[i] = (byte)ms.ReadByte();
            return buffer;
        }
        
    }
}
