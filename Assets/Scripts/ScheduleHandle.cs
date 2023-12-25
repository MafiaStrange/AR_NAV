using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using ARLocation;
using UnityEngine.InputSystem.HID;
using System.Runtime.CompilerServices;

[Serializable]
public class Event
{
    public int id;
    public string event_date;
    public string event_name;
    public string event_details;
    public string event_time;
    public string event_time_end;
    public string event_venue;
    public string latitude;
    public string longitude;
}
public class Loc_obj
{
    double[] location;
    GameObject obj; 
}

public class ScheduleHandle : MonoBehaviour
{
    // public GameObject anchor_obj;
    private static readonly HttpClient client = new HttpClient();
    private List<double[]> locations = new List<double[]>();
    private List<GameObject> loc_obj = new List<GameObject>();
    // private List<GameObject> anchors = new List<GameObject>();
    [SerializeField] private GameObject eventButtonPrefab;
    private float updateIntervalSeconds = 60.0f; // Set your desired update interval

    void Start()
    {
        StartCoroutine(UpdateEventsPeriodically());
    }

    IEnumerator UpdateEventsPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateIntervalSeconds);
            yield return StartCoroutine(FetchAndDisplayEvents());
        }
    }

    IEnumerator FetchAndDisplayEvents()
    {
        TaskCompletionSource<String> tcs = new TaskCompletionSource<String>();
        // start the asynchronous operation
        Task.Run(async () =>
        {
            try
            {
                var responseString = await client.GetStringAsync("https://maadhav17.pythonanywhere.com/get_events");
                tcs.SetResult(responseString);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });

        // wait for the asynchronous operation to complete in each frame
        if(!tcs.Task.IsCompleted)
        {
            yield return null;
        }

        // Handle the result or exception
        if (tcs.Task.IsFaulted)
        {
            Debug.LogError($"Error fetching events : {tcs.Task.Exception}");
        }
        else
        {
            List<Event> events = new List<Event>();

            foreach (string item in tcs.Task.Result.Split("|"))
            {
                var eve = JsonUtility.FromJson<Event>(item);
                events.Add(eve);
            }

            UpdateUIWithEvents(events);
        }
    }

    void UpdateUIWithEvents(List<Event> events)
    {
        ClearExistingEvents();

        //        foreach (Event e in events)
        //        {
        //            double[] location = { double.Parse(e.latitude), double.Parse(e.longitude) };
        //
        //            if (!locations.Any(elem => elem.SequenceEqual(location)))
        //            {
        //                locations.Add(location);
        //                GameObject eventButton = GameObject.Instantiate(eventButtonPrefab, anchors[ind].transform.GetChild(0));
        //
        //                Ray ray = new Ray(anchors[ind].transform.position, Vector3.down);
        //                RaycastHit hit;
        //
        //                if (Physics.Raycast(ray, out hit))
        //                {
        //                    eventButton.transform.position = hit.point + new Vector3(0, 0.1f, 0);
        //                }
        //                else
        //                {
        //                    eventButton.transform.position = anchors[ind].transform.position + new Vector3(0, 1.0f, 0);
        //                }
        //
        //                EventButtonHandler buttonHandler = eventButton.GetComponent<EventButtonHandler>();
        //                buttonHandler.AddEventDetails(e);
        //
        //                anchors.Add(eventButton);
        //            }
        //
        //            int ind = locations.FindIndex(elem => elem.SequenceEqual(location));
        //            var lower_lim = DateTime.Parse(e.event_date + " " + e.event_time);
        //            var upper_lim = DateTime.Parse(e.event_date + " " + e.event_time_end);
        //
        //            // Handle instantiation or updating based on your time conditions
        //        }
        //
        //        foreach (var anchor in anchors)
        //        {
        //            anchor.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        //        }





        // foreach (Event e in events)
        // {
        //     double[] location = { double.Parse(e.latitude), double.Parse(e.longitude) };
        // 
        //     // Check if a button already exists in that location
        //     GameObject existingButton = GetButtonAtLocation(location);
        // 
        //     if (existingButton != null)
        //     {
        //         // Button already exists, update it or handle accordingly
        //         EventButtonHandler buttonHandler = existingButton.GetComponent<EventButtonHandler>();
        //         buttonHandler.AddEventDetails(e);
        //     }
        //     else
        //     {
        //         GameObject eventButton = eventButtonPrefab;
        //         PlaceButton(location, eventButton);
        //         //eventButton.transform.position = Physics.Raycast(ray, out hit)
        //         //    ? hit.point + new Vector3(0, 0.1f, 0) // Set the position above the surface hit point
        //         //    : new Vector3((float)location[0], 1.0f, (float)location[1]); // If no surface is hit, use default height
        // 
        //         // Attach the EventButtonHandler script to handle button clicks and pass event details
        //         eventButton.GetComponent<EventButtonHandler>().AddEventDetails(e);
        //     }
        // }

        foreach (Event e in events)
        {
            double[] location = {double.Parse(e.latitude),double.Parse(e.longitude)};
            if (!locations.Any(elem => elem.SequenceEqual(location)))
            {
                locations.Add(location);
                // converting latitude and longitude into vector3 format
                Vector3 vec = GeoToVector3(location);

                // placing a button prefab at the given location
                GameObject eventButton = Instantiate(eventButtonPrefab, vec, Quaternion.identity);
                loc_obj.Add(eventButton);

                PlaceButton(location, eventButton);

                // updating the text information of the button
                eventButton.GetComponent<EventButtonHandler>().AddEventDetails(e);
            }
            else
            {
                int ind = locations.FindIndex(elem => elem.SequenceEqual(location));
                GameObject existingButton = loc_obj[ind];
                EventButtonHandler buttonHandler = existingButton.GetComponent<EventButtonHandler>();
                buttonHandler.AddEventDetails(e);
            }
        }

    }
    // private GameObject GetButtonAtLocation(double[] location)
    // {
    //     // Iterate through all buttons in the scene
    //     GameObject[] allButtons = GameObject.FindGameObjectsWithTag("Button");
    // 
    //     PlaceButton(location);
    // 
    //     foreach (var button in allButtons)
    //     {
    //         // Vector3 buttonPos = new Vector3((float)location[0], button.transform.position.y, (float)location[1]);
    //         if (distance(location[0], location[1],button.))
    // 
    //         // Adjust the comparison based on your requirements
    //         // if (Vector3.Distance(button.transform.position, buttonPos) < 0.1f)
    //         // {
    //         //   return button; // Return the existing button if found
    //         // }
    //     }
    // 
    //     return null; // No button found at the specified location
    // }

    void ClearExistingEvents()
    {
        // Implement logic to clear or reset existing events based on your needs
        locations.Clear();

        // Destroy all buttons with the specified tag
        GameObject[] allButtons = GameObject.FindGameObjectsWithTag("Button");
        foreach (var button in allButtons)
        {
            Destroy(button);
        }
    }

    void PlaceButton(double[] location, GameObject eventButton)
    {
        // Instantiate the event button prefab directly
        //GameObject eventButton = Instantiate(eventButtonPrefab);

        // Use raycasting to determine the height above the surface
        Ray ray = new Ray(GeoToVector3(location), Vector3.down);
        RaycastHit hit;

        var loc = new Location()
        {
            Latitude = (float)location[0],
            Longitude = (float)location[1],
            // Altitude = Physics.Raycast(ray, out hit) ? (hit.point.y + 0.1f) : 0.1f,
            Altitude = 1.0f,
            AltitudeMode = AltitudeMode.GroundRelative
        };

        // var opts = new PlaceAtLocation.PlaceAtOptions()
        // {
        //     HideObjectUntilItIsPlaced = false,
        //     MovementSmoothing = 0.1f,
        //     UseMovingAverage = true
        // };

        eventButton.GetComponent<PlaceAtLocation>().Location.Latitude = loc.Latitude;
        eventButton.GetComponent<PlaceAtLocation>().Location.Longitude = loc.Longitude;
        eventButton.GetComponent<PlaceAtLocation>().Location.Altitude = loc.Altitude;
        eventButton.GetComponent<PlaceAtLocation>().Location.AltitudeMode = loc.AltitudeMode;
        //PlaceAtLocation.AddPlaceAtComponent(eventButton, loc, opts);
        // return eventButton;
    }

    public double distance(double[] location1, double[] location2)
    {
        double lat1 = location1[0];
        double lon1 = location1[1];
        double lat2 = location2[0];
        double lon2 = location2[1];
        double Rad = 6372.8; // Earth's Radius In kilometers
        double dLat = (Math.PI / 180) * (lat2 - lat1);
        double dLon = (Math.PI / 180) * (lon2 - lon1);
        lat1 = (Math.PI / 180) * (lat1);
        lat2 = (Math.PI / 180) * (lat2);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        double c = 2 * Math.Asin(Math.Sqrt(a));
        return Rad * c * 1000;
    }

    // Function to convert latitude and longitude to a Vector3 position
    public Vector3 GeoToVector3(double[] location) 
    {
        double latitude = location[0];
        double longitude = location[1];
        double EarthRadius = 6372.8;
        float x = (float)(EarthRadius * Mathf.Cos((float)latitude * Mathf.Deg2Rad) * Mathf.Cos((float)longitude * Mathf.Deg2Rad));
        float y = (float)(EarthRadius * Mathf.Sin((float)latitude * Mathf.Deg2Rad));
        float z = (float)(EarthRadius * Mathf.Cos((float)latitude * Mathf.Deg2Rad) * Mathf.Sin((float)longitude * Mathf.Deg2Rad));

        return new Vector3(x, y, z);
    }

}
