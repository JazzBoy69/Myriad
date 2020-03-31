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
            double width = 600;
            double height = 287;
            var imageSize = new ImageSize(filename, width, height);
            Assert.AreEqual(width, imageSize.Width);
            Assert.AreEqual(height, imageSize.Height);
            Assert.AreEqual(filename, imageSize.Filename);
            var writer = SQLServerWriterProvider<ImageSize>.Writer(DataOperation.CreateImageSize);
            writer.BeginTransaction();
            writer.WriteData(imageSize);
            writer.Commit();
            var reader = SQLServerReaderProvider<string>.Reader(DataOperation.ReadImageSize,
                   filename);
            ImageSize sizeFromDb = reader.GetClassDatum<ImageSize>();
            Assert.AreEqual(width, sizeFromDb.Width);
            Assert.AreEqual(height, sizeFromDb.Height);
            var delete = SQLServerWriterProvider<ImageSize>.Writer(DataOperation.DeleteImageSize);
            delete.BeginTransaction();
            delete.DeleteData(imageSize.Filename);
            delete.Commit();
            reader.Open();
            sizeFromDb = reader.GetClassDatum<ImageSize>();
            Assert.IsNull(sizeFromDb);
        }
    }
}
