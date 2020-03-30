using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Myriad.Data;

namespace Myriad.Tests
{
    class ImageElementTests
    {
        [Test]
        public void AddImageSizeToDb()
        {
            string filename = "Ge0605.jpg";
            double width = 200;
            double height = 100;
            var imageSize = new ImageSize(filename, width, height);
            Assert.AreEqual(width, imageSize.Width);
            Assert.AreEqual(height, imageSize.Height);
            Assert.AreEqual(filename, imageSize.Filename);
            var writer = SQLServerWriterProvider<ImageSize>.Writer(DataOperation.WriteImageSize);
            writer.BeginTransaction();
            writer.WriteData(imageSize);
            writer.Commit();
            var reader = SQLServerReaderProvider<string>.Reader(DataOperation.ReadImageSize,
                   filename);
            ImageSize sizeFromDb = reader.GetClassDatum<ImageSize>();
            Assert.AreEqual(width, sizeFromDb.Width);
            Assert.AreEqual(height, sizeFromDb.Height);
        }
    }
}
