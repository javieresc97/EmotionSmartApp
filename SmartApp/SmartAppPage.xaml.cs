using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace SmartApp
{
    public partial class SmartAppPage : ContentPage
    {
        EmotionServiceClient emotionService;

        public SmartAppPage()
        {
            InitializeComponent();
            emotionService = new EmotionServiceClient("c39cb5f0495e40089e04bd0059c59a49");
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var photo = await GetPhotoIfPossible();
            if (photo == null)
                return;

            OutputImage.Source = ImageSource.FromStream(photo.GetStream);
            EmotionResultMessage.Text = "Espera un momento...";
            await AnalizePhotoAndShowResult(photo);
        }

        private async Task AnalizePhotoAndShowResult(MediaFile photo)
        {
            try
            {
                using (var photoStream = photo.GetStream())
                {
                    Emotion[] emotionResult = await emotionService.RecognizeAsync(photoStream);
                    //  Any faces?
                    if (emotionResult.Any())
                    {
                        var firstFace = emotionResult.FirstOrDefault();
                        var orderedDetectedEmotions = firstFace.Scores.ToRankedList();
                        var higherDetectedEmotion = orderedDetectedEmotions.FirstOrDefault().Key;
                        EmotionResultMessage.Text = $"Emoción detectada: {higherDetectedEmotion}";
                    }
                    photo.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task<MediaFile> GetPhotoIfPossible()
        {
            await CrossMedia.Current.Initialize();

            MediaFile photo = null;
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }
            else
            {
                await DisplayAlert("Alerta", "No se puede acceder a la cámara", "OK");
            }
            return photo;
        }
    }
}
