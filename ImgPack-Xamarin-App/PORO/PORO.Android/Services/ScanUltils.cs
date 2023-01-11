using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Text;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PORO.Droid.Services
{
    public class ScanUltils
    {
        public static string SaveToExternalStorage(Bitmap bitmap)
        {
            var filePath = CreateFilePath();

            try
            {
                FileOutputStream file = new FileOutputStream(filePath);
                var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 60, stream);
                stream.Close();
            }
            catch (Exception e)
            {
            }

            return filePath;
        }
        public static string LastSaveToExternalStorage(Bitmap bitmap)
        {
            var filePath = CreateFilePath();

            try
            {
                FileOutputStream file = new FileOutputStream(filePath);
                var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                stream.Close();
            }
            catch (Exception e)
            {
            }

            return filePath;
        }

        private static string CreateFilePath(bool original = false)
        {
            //var filePath = $"{Android.OS.Environment.ExternalStorageDirectory.Path}/FotoScanSdk/";
            var filePath = $"{Android.App.Application.Context.FilesDir.AbsolutePath}";

            if (!System.IO.File.Exists(filePath))
                System.IO.Directory.CreateDirectory(filePath);

            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH.mm.ss");
            string currentDateandTime = sdf.Format(new Date());

            string fileName;

            if (original)
                fileName = "IMG" + currentDateandTime + ".jpg";
            else
                fileName = "IMG_EDT" + currentDateandTime + ".jpg";

            var localPublicFilePath = System.IO.Path.Combine(filePath, fileName);
            return localPublicFilePath;
        }
    }
}