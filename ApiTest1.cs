using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtoBuf;
using TransitRealtime;


public class ApiTest1
{
  
  private const string route99Id = "6641";

    [Test]
    public async Task TransLinkApi_ReturnsSuccessfulResponse()
    {
        // 1. Read API Key from environment variable
        var apiKey = Environment.GetEnvironmentVariable("TRANSLINK_API_KEY");

        Assert.That(
            apiKey,
            Is.Not.Null.And.Not.Empty,
            "The TRANSLINK_API_KEY is missing."
        );

        // 2. Send HTTP GET request
        var url = $"https://gtfsapi.translink.ca/v3/gtfsposition?apikey={Uri.EscapeDataString(apiKey!)}";

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        using var response = await client.GetAsync(url);

        // 3. Check the HTTP Status Code
        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK),
            $"Expected HTTP 200, received {(int)response.StatusCode}."
            );

        // 4. Read the response as binary data
        var responseBytes = await response.Content.ReadAsByteArrayAsync();

        Assert.That(
        responseBytes.Length,
        Is.GreaterThan(0),
        "The API returned an empty response."
        );

        // 5. Decode Protocol Buffer
        using var stream = new MemoryStream(responseBytes);
        var feed = Serializer.Deserialize<FeedMessage>(stream);

        // 6. Validate decoded feed
        Assert.That(
            feed,
            Is.Not.Null,
            "FeedMessage is null."
            );
        Assert.That(
            feed.Header,
            Is.Not.Null,
            "Feed Header is missing."
            );
        Assert.That(
            feed.Entities.Count,
            Is.GreaterThan(0),
            "GTFS feed contains no entities."
            );

        // 7. Display basic response information
        TestContext.Out.WriteLine($"HTTP Status: {(int)response.StatusCode}");
        TestContext.Out.WriteLine($"GTFS Version: {feed.Header.GtfsRealtimeVersion}");
        TestContext.Out.WriteLine($"Total feed entities: {feed.Entities.Count}");

        // 8. Filter vehicle positions for Route 99
        var route99Records = GetRoute99Vehicles(feed);

        // 9. Identify unique vehicle IDs in the original records
        var uniqueVehicleIds = route99Records
        .Select(v => v.Vehicle!.Id)
        .Distinct()
        .ToList();

        // 10. Remove duplicate vehicle records
        var route99Vehicles = GetUniqueVehicles(route99Records);

        // 11. Verify duplicate removal
        Assert.That(
            route99Vehicles.Select(v => v.Vehicle!.Id).Distinct().Count(),
            Is.EqualTo(uniqueVehicleIds.Count),
            "Route 99 contains duplicate vehicle IDs."
        );

        var duplicatesRemoved = route99Records.Count - route99Vehicles.Count;

        TestContext.Out.WriteLine($"Duplicate records removed: {duplicatesRemoved}");

        // 12. Displayy Result
        TestContext.Out.WriteLine($"Route: 99");
        TestContext.Out.WriteLine($"Matching records: {route99Records.Count}");
        TestContext.Out.WriteLine($"Unique buses: {route99Vehicles.Count}");

        foreach (var vehicle in route99Vehicles)
        {
            TestContext.Out.WriteLine(
                $"Vehicle ID: {vehicle.Vehicle!.Id}, " +
                $"Route: 99, " +
                $"Direction: {FormatDirection(vehicle.Trip?.DirectionId)}, " +
                $"Timestamp: {FormatTimestamp(vehicle.Timestamp)}, " +
                $"Latitude: {FormatCoordinate(vehicle.Position?.Latitude)}, " +
                $"Longitude: {FormatCoordinate(vehicle.Position?.Longitude)}"
            );
        }
    }
    private static List<VehiclePosition> GetRoute99Vehicles(FeedMessage feed) 
    {
        return feed.Entities
        .Where(e => e.Vehicle?.Trip?.RouteId == route99Id)
        .Select(e => e.Vehicle!)
        .Where(v => !string.IsNullOrWhiteSpace(v.Vehicle?.Id))
        .ToList();
    }
    private static List<VehiclePosition> GetUniqueVehicles(
        List<VehiclePosition> vehicles)
    {
        return vehicles
        .GroupBy(v => v.Vehicle!.Id)
        .Select(g => g.OrderByDescending(v => v.Timestamp).First())
        .ToList();
    }
    private static string FormatTimestamp(ulong timestamp)
    {
        if (timestamp == 0)
        {
            return "Not available";
        }
        return DateTimeOffset
        .FromUnixTimeSeconds((long)timestamp)
        .ToLocalTime()
        .ToString("yyyy-MM-dd HH:mm:ss");
    }
    private static string FormatCoordinate(float? coordinate)
    {
        return coordinate.HasValue
        ? coordinate.Value.ToString("F6")
        : "Not available";
    }
    private static string FormatDirection(uint? directionId)
    {
        return directionId.HasValue
        ? directionId.Value.ToString()
        : "Not available";
    }
}