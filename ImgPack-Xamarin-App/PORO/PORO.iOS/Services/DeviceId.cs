using Foundation;
using PORO.iOS.Services;
using PORO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceId))]
namespace PORO.iOS.Services
{
    public class DeviceId : IDevice
    {
        public string GetIdentifier()
        {
            return UIDevice.CurrentDevice.IdentifierForVendor.AsString();
        }
    }
}