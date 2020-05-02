using System.Threading.Tasks;
using System.Collections.Generic;

using ObjectDetectionMobileApp.Models;
using ObjectDetectionMobileApp.Services;

using SkiaSharp;
using Xamarin.Forms;
using Plugin.Media.Abstractions;

namespace ObjectDetectionMobileApp.ViewModels
{
    public class ObjectDetectionViewModel : BaseViewModel
    {
        public Command<bool> TakePhotoCommand { get; set; }
        public Command<bool> DetectObjectsCommand { get; set; }

        private MediaFile photo;

        public MediaFile Photo
        {
            get { return photo; }
            set { photo = value; OnPropertyChanged(); OnPropertyChanged("PhotoStream"); }
        }

        private SKBitmap image;

        public SKBitmap Image
        {
            get { return image; }
            set { image = value; OnPropertyChanged(); }
        }

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; OnPropertyChanged(); }
        }

        public ImageSource PhotoStream
        {
            get => this.photo != null ? ImageSource.FromStream(photo.GetStreamWithImageRotatedForExternalStorage) : null;
        }

        private List<Prediction> objectInfo;

        public List<Prediction> Predictions
        {
            get { return objectInfo; }
            set { objectInfo = value; OnPropertyChanged(); }
        }

        public ObjectDetectionViewModel()
        {
            TakePhotoCommand = new Command<bool>(async (useCamera) => await GetPhoto(useCamera));
            DetectObjectsCommand = new Command<bool>(async (useLocal) => await DetectObjects(useLocal));
        }

        private async Task GetPhoto(bool useCamera)
        {
            Predictions = new List<Prediction>();
            Photo = await ImageService.TakePhoto(useCamera);
            Image = SKBitmap.Decode(Photo.GetStreamWithImageRotatedForExternalStorage());
        }

        private async Task DetectObjects(bool useLocal)
        {
            if (Photo != null)
            {
                IsBusy = true;

                if (!useLocal)
                    Predictions = await CustomVisionAzureService.DetectObjects(photo);

                IsBusy = false;
            }
        }
    }
}