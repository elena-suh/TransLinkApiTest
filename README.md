
# TransLink GTFS-Realtime API Test

## Overview
Automation test for Live API Integration and Vehicle Position.

The Test:
- Sends a GET request to the TransLink real-time API.
- Validates HTTP 200 response.
- Decode the binary response as a GTFS-Real-time protocol buffer.
- Filters vehicle positions for Route 99.
- Removes duplicate vehicle IDs.
- Verifies that vehicle IDs are unique.
- Displays vehicle ID, route, direction, timestamp, latitude, longitude.

## Technology
-	C#
-	.NET
-	NUnit
-	HttpClient
-	GTHS-Realtime Protocol Buffers
-	GTFSRealtimeBindings
	
## Prerequisites
-	.NET 10 SDK
-	Valid TransLink API key

## Configuration
Register to get API key
https://www.translink.ca/about-us/doing-business-with-translink/app-developer-resources/register

Set the API key as an environment variable
via Windows Command Prompt (CMD):
set TRANSLINK_API_KEY=YOUR_API_KEY

The API key is read from the TRANSLINK_API_KEY environment variable and is not stored in the source code.

## Run
Run the test in the same terminal window:
	dotnet test

## Route 99
The public Route 99 (099) maps to the TransLink GTFS route_id 6641.
The route ID was identified by checking the TransLink GTFS static data:
-	Download the GTFS Static Data from the developer API portal:
https://www.translink.ca/about-us/doing-business-with-translink/app-developer-resources/gtfs/gtfs-data 

-	Open the routes.txt
-	Search for 99 (see route_short_name is 099, route_long_name B-Line)
-	See that he corresponding route_id is 6641
The test uses route_id = 6641 to identify Route 99 vehicle position in the GTFS-real-time feed.

## Test Coverage
The test validates:
-	API key availability
-	HTTP response status
-	GTFS-Real-time Protocol Buffer decoding
-	Feed/header validity
-	Route 99 filtering
-	Unique vehicle IDs
-	Vehicle information reporting
-	Missing GPS/direction/timestamp handling

## Assumptions / Known Issues
-	The API provides live data, so vehicle counts and positions change between test run.
-	Direction reported as the GTFS direction_id (0 or 1).
-	GtfsRealtimeBindings targets older .NET framework versions and may produce a NU1701 compatibility Warning. The test runs successfully on the configured .NET 10 environment. 





