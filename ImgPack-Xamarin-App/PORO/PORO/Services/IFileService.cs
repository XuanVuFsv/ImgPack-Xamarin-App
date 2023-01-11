using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PORO.Services
{
    public interface IFileService
    {
        string SaveImageFromByte(byte[] imageBytes, string fileName = null);
        Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null);
        string GetImageLocalFilePath(string filepath);
        string SaveToExternalStorage(SKBitmap bitmap, string filePath = null);
        string SaveCompressImage(SKBitmap bitmap, string filePath);
    }
}
