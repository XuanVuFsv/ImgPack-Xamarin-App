using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Services
{
    public interface IEditService
    {
        void Original(string cropPath);
        string ContrastChange(int value, string path);
        string Brightness(int value, string path);
        string Whitebalance(int value, string path);

        string BlackWhite(string path);
        string GreyScale(string path);
        string Invert(string path);
        string Swap(string path);
        string Sepia(string path);
        string Polaroid(string path);
        string Sunset(string path);

        void Crop();

        void ResetService(string path);
        string SavePhoto(string path);
    }
}
