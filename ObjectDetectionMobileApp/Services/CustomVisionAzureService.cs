using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

using ObjectDetectionMobileApp.Models;
using ObjectDetectionMobileApp.Helpers;

using Newtonsoft.Json;
using Plugin.Media.Abstractions;
using System.Collections.Generic;

namespace ObjectDetectionMobileApp.Services
{
    public static class CustomVisionAzureService
    {
        private static readonly HttpClient client = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.PredictionURL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Prediction-Key", Constants.PredictionKey);
            return client;
        }

        public async static Task<List<Prediction>> DetectObjects(MediaFile picture)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var stream = new MemoryStream();
                await picture.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(stream);

                using (var content = new ByteArrayContent(stream.ToArray()))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    var response = await client.PostAsync(Constants.PredictionProjectRequest, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var predictionResult = await response.Content.ReadAsStringAsync();
                        var customVisionResult = JsonConvert.DeserializeObject<CustomVisionResult>(predictionResult);

                        return customVisionResult.Predictions.Where(x => x.Probability > 0.3).ToList();
                    }
                    else
                    {
                        return new List<Prediction>();
                    }
                }
            }
            catch (Exception ex)
            {
                return new List<Prediction>();
            }
        }
    }
}