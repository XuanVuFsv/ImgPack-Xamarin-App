using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PORO.Droid.Services;
using PORO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(ScanService))]
namespace PORO.Droid.Services
{
    public class ScanService : IScanService
    {
        //public async Task<string> ScanAsync()
        //{
        //    var optionsDefault = new MobileBarcodeScanningOptions();
        //    var optionsCustom = new MobileBarcodeScanningOptions();

        //    var scanner = new MobileBarcodeScanner()
        //    {
        //        TopText = "Scan the QR Code",
        //        BottomText = "Please Wait",
        //    };

        //    var scanResult = await scanner.Scan(optionsCustom);
        //    return scanResult.Text;
        //}
        public Task<string> ScanAsync()
        {
            throw new NotImplementedException();
        }
    }
}