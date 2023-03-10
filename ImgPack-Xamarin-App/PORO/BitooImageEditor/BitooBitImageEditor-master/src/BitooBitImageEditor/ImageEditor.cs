using BitooBitImageEditor.EditorPage;
using BitooBitImageEditor.Helper;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using NativeMedia;
using System.Linq;

namespace BitooBitImageEditor
{
    /// <summary>main class <see cref="BitooBitImageEditor"/> </summary>
    public class ImageEditor
    {
        private static readonly Lazy<ImageEditor> lazy = new Lazy<ImageEditor>(() => new ImageEditor());
        private ImageEditor()
        {
            IPlatformHelper platform = DependencyService.Get<IPlatformHelper>();
            if (!(platform?.IsInitialized ?? false))
                throw new Exception("BitooBitImageEditor must be initialized on the platform");
        }

        /// <summary>returns an instance of <see cref="ImageEditor"/></summary>
        public static ImageEditor Instance { get => lazy.Value; }
        internal IImageHelper ImageHelper => DependencyService.Get<IImageHelper>();
        /// <summary>"True" - if the editor is currently running</summary>
        public static bool IsOpened { get; private set; } = false;

        private const string defaultFolderName = "BitooBitImages";
        private string folderName;
        private bool mainPageIsChanged = false;
        private Page mainPage;
        private TaskCompletionSource<byte[]> taskCompletionEditImage;
        private bool imageEditLock;
        private bool imageSetLock;
        private ImageCropPage cropPage;

        /// <summary>name of the folder for saving images </summary>
        public string FolderName
        {
            get => string.IsNullOrWhiteSpace(folderName) ? defaultFolderName : folderName;
            set => folderName = value;
        }
        private bool ImageEditLock
        {
            get => imageEditLock;
            set => imageEditLock = IsOpened = value;
        }


        /// <summary>method for saving images</summary>
        /// <param name="data">image</param>
        /// <param name="imageName">file name of the image</param>
        /// <returns>returns "true" if the image was saved</returns>
        public async Task<bool> SaveImage(byte[] data, string imageName) => await ImageHelper.SaveImageAsync(data, imageName, FolderName);

        /// <summary>Returns the edited image
        /// <para> if <paramref name="bitmap"/> is null, the user can select an image from the gallery</para></summary>
        /// <param name="bitmap">original image</param>
        /// <param name="config">сonfigurator image editor</param>
        /// <returns>edited image</returns>
        public async Task<byte[]> GetCropperImage(SKBitmap bitmap, ImageEditorConfig config)
        {
            if (!ImageEditLock)
            {
                ImageEditLock = true;
                if (bitmap == null)
                {
                    var result = (await MediaGallery.PickAsync(1, MediaFileType.Image))?.Files?.FirstOrDefault();
                    if (result != null)
                        using (Stream stream = await result.OpenReadAsync())
                            bitmap = stream != null ? SKBitmap.Decode(stream) : null;
                }

                if (config == null)
                    config = new ImageEditorConfig();

                var data = bitmap != null ? await PushImageCropperPage(bitmap, config) : null;
                ImageEditLock = false;
                return data;
            }
            else
                return null;
        }
        internal void SetImage(SKBitmap bitmap = null)
        {
            if (!imageSetLock)
            {
                imageSetLock = true;
                if (bitmap != null)
                {
                    taskCompletionEditImage.SetResult(SkiaHelper.SKBitmapToBytes(bitmap));
                }
                else
                    taskCompletionEditImage.SetResult(null);
            }
        }

        private async Task<byte[]> PushImageCropperPage(SKBitmap bitmap, ImageEditorConfig config)
        {
            try
            {
                taskCompletionEditImage = new TaskCompletionSource<byte[]>();

                if (bitmap != null)
                {
                    cropPage = new ImageCropPage(bitmap, config);
                    await Application.Current.MainPage.Navigation.PushModalAsync(cropPage, animated: false);
                    //if (Device.RuntimePlatform == Device.Android)
                    //{
                    //    await Application.Current.MainPage.Navigation.PushModalAsync(cropPage, animated: false);
                    //}
                    //else
                    //{
                    //    mainPage = Application.Current.MainPage;
                    //    Application.Current.MainPage = cropPage;
                    //    mainPageIsChanged = true;
                    //}
                }
                else
                    taskCompletionEditImage.SetResult(null);

                byte[] data = await taskCompletionEditImage.Task;

                if (mainPageIsChanged)
                    Application.Current.MainPage = mainPage;
                else
                    await Application.Current.MainPage.Navigation.PopModalAsync(animated: false);

                mainPage = null;
                ImageEditLock = false;
                imageSetLock = false;
                return data;
            }
            catch (Exception ex)
            {
                SetImage(null);
                return null;
            }
        }
    }
}
