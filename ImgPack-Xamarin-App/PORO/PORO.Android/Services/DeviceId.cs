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

[assembly: Xamarin.Forms.Dependency(typeof(DeviceId))]
namespace PORO.Droid.Services
{
    public class DeviceId : IDevice
    {
        public string GetIdentifier()
        {
            return Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
        }
    }
}