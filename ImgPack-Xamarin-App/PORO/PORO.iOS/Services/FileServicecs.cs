using Foundation;
using PORO.iOS.Services;
using PORO.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(FileService))]

namespace PORO.iOS.Services
{
    public class FileService : IFileService
    {

        #region SaveImageFromByte
        public string SaveImageFromByte(byte[] imageBytes, string fileName = null)
        {
            var date = DateTime.Now;
            var imageName = $"Temp {date:yyyy-MM-dd HH.mm.ss} {date.Ticks}.png";
            var filePath = CreateFilePath(fileName: imageName);

            File.WriteAllBytes(filePath, imageBytes);

            return filePath;
        }

        public string CreateFilePath(string fileName = "")
        {
            string imageDirPath = GetImageDirPath();

            Directory.CreateDirectory(imageDirPath);

            if (!Directory.Exists(imageDirPath))
            {
                Directory.CreateDirectory(imageDirPath);
            }

            var filePath = Path.Combine(imageDirPath, fileName);

            return filePath;
        }

        public string GetImageLocalFilePath(string filepath)
        {
            string imageDirPath = GetImageDirPath();

            return Path.Combine(imageDirPath, Path.GetFileName(filepath));
        }

        private string GetImageDirPath()
        {
            string libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
            return Path.Combine(libraryPath, "nfta-images");
        }
        public static string CreatePath(string fileName = "")
        {
            string libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
            var filePath = Path.Combine(libraryPath, "nfta-images");
            InitStorageUpload(filePath);

            var localPublicFilePath = Path.Combine(filePath, fileName);

            return localPublicFilePath;
        }
        public static bool InitStorageUpload(string filePath)
        {
            if (File.Exists(filePath)) return false;

            Directory.CreateDirectory(filePath);
            return true;
        }
        #endregion

        public string SaveToExternalStorage(SKBitmap bitmap, string filePath = null)
        {
            var filepath = ScanUtils.SaveToExternalStorage(bitmap, filePath);
            return filepath;
        }

        public string SaveCompressImage(SKBitmap bitmap, string filePath)
        {
            return filePath;
        }

        public Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null)
        {
            NSData nsData = NSData.FromArray(data);
            UIImage image = new UIImage(nsData);
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            image.SaveToPhotosAlbum((UIImage img, NSError error) => taskCompletionSource.SetResult(error == null));

            return taskCompletionSource.Task;
        }

        public void OpenAppSetting()
        {
            var url = new NSUrl(UIKit.UIApplication.OpenSettingsUrlString);
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}