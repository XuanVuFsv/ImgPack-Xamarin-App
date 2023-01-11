using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PORO.Untilities
{
    public class DeviceExtension
    {
        public static Task BeginInvokeOnMainThreadAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task<T> BeginInvokeOnMainThreadAsync<T>(Func<T> a)
        {
            var tcs = new TaskCompletionSource<T>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var result = a();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
