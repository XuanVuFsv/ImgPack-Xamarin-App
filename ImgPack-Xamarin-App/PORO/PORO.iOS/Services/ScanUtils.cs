using Foundation;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UIKit;

namespace PORO.iOS.Services
{
    public class ScanUtils
    {
        #region CreateFilePath

        public static string CreateFilePath(string fileName = "")
        {
            string libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
            var filePath = Path.Combine(libraryPath, "nfta-images");
            InitStorageUpload(filePath);

            var localPublicFilePath = Path.Combine(filePath, fileName);

            return localPublicFilePath;
        }

        #endregion

        #region InitStorageUpload

        public static bool InitStorageUpload(string filePath)
        {
            if (File.Exists(filePath)) return false;

            Directory.CreateDirectory(filePath);
            return true;
        }

        #endregion

        #region FirstSavetoCache

        public static string FirstSavetoCache(byte[] imageBytes, string fileName = null)
        {
            //save file
            var date = DateTime.Now;
            var imageName = string.IsNullOrEmpty(fileName) ? $"Temp {date:yyyy-MM-dd HH.mm.ss} {date.Ticks}.png" : fileName;

            // create fileService directory
            var filePath = CreateFilePath(fileName: imageName);

            //Copy the private fileService's data to the EXTERNAL PUBLIC location
            File.WriteAllBytes(filePath, imageBytes);

            return filePath;
        }

        public static string SaveToExternalStorage(SKBitmap bitmap, string filePath)
        {
            byte[] imageBytes;
            using (NSData jpgImage = SKImage.FromBitmap(bitmap).ToUIImage().AsJPEG(1f))
            {
                imageBytes = jpgImage.ToArray();

                var imageName = $"EDITED_{Path.GetFileName(filePath)}";
                filePath = CreateFilePath(fileName: imageName);

                File.WriteAllBytes(filePath, imageBytes);

                return filePath;
            }
        }

        #endregion
    }
}