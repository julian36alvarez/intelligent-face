using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;


using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Reflection;
namespace MediaSample
{
  public partial class MediaPage : ContentPage
  {
		private const string subscriptionKey = "d14322ac965c470c9504df8d7fc74652";
		private const string faceEndpoint =
			"https://westcentralus.api.cognitive.microsoft.com";
		private readonly IFaceClient faceClient = new FaceClient(
			new ApiKeyServiceClientCredentials(subscriptionKey),
			new System.Net.Http.DelegatingHandler[] { });
		//IList<DetectedFace> faceList;   // The list of detected faces.
		String[] faceDescriptions;     // The list of descriptions for the detected faces.
		public MediaPage()
    {
      InitializeComponent();
			
				faceClient.Endpoint = faceEndpoint;
			
			
		}
		private async void BtnPick_Clicked(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();
			try
			{
				var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
				{
					PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
				});
				if (file == null) return;
				imgChoosed.Source = ImageSource.FromStream(() => {
					var stream = file.GetStream();
					return stream;
				});
				Esperar.IsVisible = true;
				Esperar.IsRunning = true;


				lblResult.Text = null;
				var faceList = await Task.Run(async () => await GetImageDescription(file.GetStream()));
				Esperar.IsVisible = false;
				Esperar.IsRunning = false;


				file.Dispose();
				Title = "Detecting...";
				Title = String.Format(
					"Detection Finished. {0} face(s) detected", faceList.Count);
				faceDescriptions = new String[faceList.Count];
				for (int i = 0; i < faceList.Count; ++i)
				{
					DetectedFace face = faceList[i];
					faceDescriptions[i] = FaceDescription(face);
					lblResult.Text = faceDescriptions[i] + "\r\n";

					
						
					
				}
			}
			catch (Exception ex)
			{
				string test = ex.Message;
			}
		}

		//public async Task<AnalysisResult> GetImageDescription(Stream imageStream)
		//{
			//VisionServiceClient visionClient = new VisionServiceClient("afb2168d56a147d7b3b7728aa0a9558a", "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0");
			//VisualFeature[] features = {
				//VisualFeature.Tags,
				//VisualFeature.Categories,
				//VisualFeature.Description
			//};
			//return await visionClient.AnalyzeImageAsync(imageStream, features.ToList(), null);
		//}


		public async Task<IList<DetectedFace>> GetImageDescription(Stream imageStream)
		{
			IList<FaceAttributeType> faceAttributes =
		new FaceAttributeType[]
		{
			FaceAttributeType.Gender, FaceAttributeType.Age,
			FaceAttributeType.Smile, FaceAttributeType.Emotion,
			FaceAttributeType.Glasses, FaceAttributeType.Hair
		};

			IList<DetectedFace> faceList =
				await faceClient.Face.DetectWithStreamAsync(
					imageStream, true, false, faceAttributes);
			 return faceList;
		}

		public async void BtnTake_Clicked(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();
			try
			{
				if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
				{
					await DisplayAlert("No Camera", ":( No camera available.", "OK");
					return;
				}
				var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
				{
					Directory = "Sample",
					Name = "xamarin.jpg"
				});
				if (file == null)
					return;
				imgChoosed.Source = ImageSource.FromStream(() =>
				{
					var stream = file.GetStream();
					return stream;
				});
				Esperar.IsVisible = true;
                Esperar.IsRunning = true;


				lblResult.Text = null;
				var faceList = await Task.Run(async() => await GetImageDescription(file.GetStream()));
                Esperar.IsVisible = false;
				Esperar.IsRunning = false;
				
				
				file.Dispose();
				
				Title = "Detecting...";
				Title = String.Format(
					"Detection Finished. {0} face(s) detected", faceList.Count);
				faceDescriptions = new String[faceList.Count];
				for (int i = 0; i < faceList.Count; ++i)
				{
					DetectedFace face = faceList[i];
					faceDescriptions[i] = FaceDescription(face);
					lblResult.Text = faceDescriptions[i] + "\r\n";
				}
			}
			catch (Exception ex)
			{
				string test = ex.Message;
			}
		}

		// Creates a string out of the attributes describing the face.
		private string FaceDescription(DetectedFace face)
		{
			StringBuilder sb = new StringBuilder();
			emocionLoad.Source = null;
			sb.Append(" Genero: ");
			string generoT= face.FaceAttributes.Gender.ToString();
			
			if (generoT == "Male") {
				sb.Append("Hombre");
			}else if (generoT == "Female")
			{
				sb.Append("Mujer");
			}
				
			sb.Append("\r\n ");
			sb.Append("Edad: ");
			sb.Append(face.FaceAttributes.Age);
			sb.Append("\r\n ");
			sb.Append("Nivel de Sonrisa: ");
			sb.Append(String.Format("Sonrie en un {0:F1}%, ", face.FaceAttributes.Smile * 100));
			sb.Append("\r\n ");
			// Add the emotions. Display all emotions over 10%.
			sb.Append("Emocion: ");
			Emotion emotionScores = face.FaceAttributes.Emotion;
			if (emotionScores.Anger >= 0.1f)
				sb.Append(String.Format(" Enojo {0:F1}%, \r\n", emotionScores.Anger * 100));
			if (emotionScores.Contempt >= 0.1f)
				sb.Append(String.Format(" Desprecio {0:F1}%, \r\n", emotionScores.Contempt * 100));
			if (emotionScores.Disgust >= 0.1f)
				sb.Append(String.Format(" Asco {0:F1}%, \r\n", emotionScores.Disgust * 100));
			if (emotionScores.Fear >= 0.1f)
				sb.Append(String.Format(" Miedo {0:F1}%, \r\n", emotionScores.Fear * 100));
			if (emotionScores.Happiness >= 0.1f)
				sb.Append(String.Format(" Alegria {0:F1}%, \r\n", emotionScores.Happiness * 100));
			if (emotionScores.Neutral >= 0.1f)
				sb.Append(String.Format(" Neutral {0:F1}%, \r\n", emotionScores.Neutral * 100));
			if (emotionScores.Sadness >= 0.1f)
				sb.Append(String.Format(" Triste {0:F1}%, \r\n", emotionScores.Sadness * 100));
			if (emotionScores.Surprise >= 0.1f)
				sb.Append(String.Format(" Sorprendido {0:F1}%, \r\n", emotionScores.Surprise * 100));

			// Add glasses.
			sb.Append(" Gafas: ");
			//sb.Append(face.FaceAttributes.Glasses);
			string gafas = face.FaceAttributes.Glasses.ToString();

			if (gafas == "NoGlasses")
			{
				sb.Append("Sin Gafas");
			}
			else 
			{
				sb.Append("Con Gafas");
				//emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("gafas.jpg") : ImageSource.FromFile("Images/gafas.jpg");
			}
			sb.Append("\r\n ");

			// Add hair.
			sb.Append("Cabello: ");

			// Display baldness confidence if over 1%.
			if (face.FaceAttributes.Hair.Bald >= 0.01f)
				sb.Append(String.Format("bald {0:F1}% ", face.FaceAttributes.Hair.Bald * 100));

			// Display all hair color attributes over 10%.
			IList<HairColor> hairColors = face.FaceAttributes.Hair.HairColor;
			foreach (HairColor hairColor in hairColors)
			{
				if (hairColor.Confidence >= 0.1f)
				{
					sb.Append(hairColor.Color.ToString());
					sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
				}
			}

			if (gafas != "NoGlasses")
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("gafas.jpg") : ImageSource.FromFile("Images/gafas.jpg");
			}
			else if (emotionScores.Anger >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("enojado.jpg") : ImageSource.FromFile("Images/enojado.jpg");
			}
			else if (emotionScores.Contempt >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("side.jpg") : ImageSource.FromFile("Images/side.jpg");
			}
			else if (emotionScores.Disgust >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("asco.png") : ImageSource.FromFile("Images/asco.png");
			}
			else if (emotionScores.Fear >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("mieg.jpg") : ImageSource.FromFile("Images/mieg.jpg");
			}
			else if (emotionScores.Happiness >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("feliz.jpg") : ImageSource.FromFile("Images/feliz.jpg");
			}
			else if (emotionScores.Sadness >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("triste.jpg") : ImageSource.FromFile("Images/triste.jpg");
			}
			else if (emotionScores.Neutral >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("giphy.gif") : ImageSource.FromFile("Images/giphy.gif");
			}
			else if (emotionScores.Surprise >= 0.5f)
			{
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("sorp.png") : ImageSource.FromFile("Images/sorp.png");
			}
			else {
				emocionLoad.Source = Device.RuntimePlatform == Device.Android ? ImageSource.FromFile("giphy.gif") : ImageSource.FromFile("Images/giphy.gif");
			}

			// Return the built string.
			return sb.ToString();
		}
	}
}
