using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Security;
using PORO.Droid.Services;
using PORO.Services;
using Prism;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Permissions = Xamarin.Essentials.Permissions;
using Stream = System.IO.Stream;

[assembly: Xamarin.Forms.Dependency(typeof(FileService))]

namespace PORO.Droid.Services
{
    public class FileService : IFileService
    {
        public string SaveImageFromByte(byte[] imageBytes, string fileName = null)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            var filepath = ScanUltils.SaveToExternalStorage(bitmap);
            return filepath;
        }

        public string GetImageLocalFilePath(string filepath)
        {
            // no need to implement this on Android
            return filepath;
        }

        public string SaveToExternalStorage(SKBitmap bitmap, string filePath = null)
        {
            byte[] imageAsBytes;
            //SKCanvas canvas = new SKCanvas(bitmap);
            SKImage image = SKImage.FromPixels(bitmap.PeekPixels());
            SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);

            Stream stream = encoded.AsStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                stream.Dispose();
                imageAsBytes = memoryStream.ToArray();
            }
            var file = SaveImageFromByte(imageAsBytes);
            return file;
        }
        public string SaveCompressImage(SKBitmap bitmap, string filePath)
        {
            byte[] imageAsBytes;
            //SKCanvas canvas = new SKCanvas(bitmap);
            SKImage image = SKImage.FromPixels(bitmap.PeekPixels());
            SKData encoded = image.Encode(SKEncodedImageFormat.Jpeg, 20);

            System.IO.Stream stream = encoded.AsStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                stream.Dispose();
                imageAsBytes = memoryStream.ToArray();
            }
            var file = SaveImageFromByte(imageAsBytes, filePath);
            return file;
        }

        public async Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null)
        {
            try
            {
                File picturesDirectory = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures);
                File folderDirectory;

                if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
                    return false;

                if (!string.IsNullOrEmpty(folder))
                {
                    folderDirectory = new File(picturesDirectory, folder);
                    folderDirectory.Mkdirs();
                }
                else
                    folderDirectory = picturesDirectory;

                using (File bitmapFile = new File(folderDirectory, filename))
                {
                    bitmapFile.CreateNewFile();

                    using (FileOutputStream outputStream = new FileOutputStream(bitmapFile))
                        await outputStream.WriteAsync(data);

                    // Make sure it shows up in the Photos gallery promptly.
                    MediaScannerConnection.ScanFile(Platform.CurrentActivity,
                                                    new string[] { bitmapFile.Path },
                                                    new string[] { "image/png", "image/jpeg" }, null);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void OpenAppSetting()
        {
        }
    }
}