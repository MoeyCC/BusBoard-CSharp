using Newtonsoft.Json;
using BusBoard.Models;

namespace BusBoard;

internal class Program
{
    async static Task BusBoard()
    {
        string postcodeInput = "N70DP";

        //Get postcode data
        HttpClient postcodeClient = new HttpClient();
        postcodeClient.BaseAddress = new Uri("https://api.postcodes.io/postcodes/");
        string postcodeJson = await postcodeClient.GetStringAsync(postcodeInput);
        PostcodeResponse postcode = JsonConvert.DeserializeObject<PostcodeResponse>(postcodeJson); 
        float postcodeLongitude = postcode.Result.Longitude;
        float postcodeLatitude = postcode.Result.Latitude;
        
        //Get nearby stop points
        HttpClient tflClient = new HttpClient();
        tflClient.BaseAddress = new Uri("https://api.tfl.gov.uk/StopPoint/");
        string stopPointsJson = await tflClient.GetStringAsync($"?lat={postcodeLatitude}&lon={postcodeLongitude}&stopTypes=NaptanPublicBusCoachTram");
        StopPointsResponse stopPointsResponse = JsonConvert.DeserializeObject<StopPointsResponse>(stopPointsJson); 
        StopPoint firstStopPoint = stopPointsResponse.StopPoints.OrderBy(s => s.Distance).First();

        //get next bus info
        string arrivalsJson = await tflClient.GetStringAsync($"{firstStopPoint.NaptanId}/Arrivals");
        List<ArrivalPrediction> predictions = JsonConvert.DeserializeObject<List<ArrivalPrediction>>(arrivalsJson);
        List<ArrivalPrediction> orderedPredictions = predictions.OrderBy(p => p.TimeToStation).ToList();

        foreach (var prediction in orderedPredictions)
        {
            Console.WriteLine($"Bus {prediction.LineName} will be arriving in {prediction.TimeToStation/60} minutes bound for {prediction.DestinationName}");            
        } 

        if (predictions is null)
        {
            throw new Exception("Unable to retrieve predictions from JSON response");
        }
    }
    
    async static Task Main(string[] args)
    {
        await BusBoard();
    }
}