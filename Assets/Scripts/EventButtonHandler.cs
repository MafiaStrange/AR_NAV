using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class EventButtonHandler : MonoBehaviour, IPointerClickHandler
{
    private List<Event> associatedEvents = new List<Event>();
    private bool isClicked = false;

    public void AddEventDetails(Event e)
    {
        associatedEvents.Add(e);
        UpdateButtonDisplay();
    }

    private void UpdateButtonDisplay()
    {
        TMP_Text buttonText = GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            if (isClicked)
            {
                // Display all the events
                buttonText.text = string.Join("\n", associatedEvents.Select(eventObj => GetEventSummary(eventObj)));
            }
            else
            {
                // Display summary view
                string[] eventVenue = (string[]) associatedEvents.Select(eventObj => eventObj.event_venue);
                buttonText.text = eventVenue[0];
            }
        }
    }

    private string GetEventSummary(Event e)
    {
        return $"{e.event_name}\n{e.event_date} {e.event_time} - {e.event_time_end}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle the button click event
        isClicked = !isClicked;
        UpdateButtonDisplay();
    }
}
