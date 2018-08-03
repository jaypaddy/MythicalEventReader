using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EventReader
{
    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class EventLocation
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public EventLocation(string name, string loctype)
        {
            displayName = name;
            locationType = loctype;
        }
    }

    public class Event
    {

        [JsonProperty(PropertyName = "id")]
        public string id
        { get; set; }
        [JsonProperty(PropertyName = "subject")]
        public string subject { get; set; }

        [JsonProperty(PropertyName = "body")]
        public Body body { get; set; }

        [JsonProperty(PropertyName = "start")]
        public Start start { get; set; }

        [JsonProperty(PropertyName = "end")]
        public End end { get; set; }




        [JsonProperty(PropertyName = "location")]
        public EventLocation location { get; set; }

        [JsonProperty(PropertyName = "attendees")]
        public List<Attendee> attendees { get; set; }

        [JsonProperty(PropertyName = "organizer")]
        public Organizer organizer { get; set; }

    }

    public class Organizer
    {
        [JsonProperty(PropertyName = "emailAddress")]
        public EmailAddress eAddr { get; set; }
    }
    public class EventList
    {
        [JsonProperty(PropertyName = "@odata.context")]
        public string odatacontext { get; set; }

        [JsonProperty(PropertyName = "@odata.nextlink")]
        public string NextPageLink { get; set; }

        [JsonProperty(PropertyName = "value")]
        public List<Event> events { get; set; }

        public EventList()
        {
            events = new List<Event>();
        }
    }

    public class EmailAddress
    {
        public string name { get; set; }
        public string address { get; set; }

        public EmailAddress(string _name, string _address)
        {
            this.name = _name;
            this.address = _address;
        }
    }

    public class Attendee
    {
        public string type { get; set; }
        public EmailAddress emailAddress { get; set; }
        public Attendee(EmailAddress _emailAddress, string _type)
        {
            emailAddress = _emailAddress;
            type = _type;
        }
    }

    public class Start
    {
        public string dateTime { get; set; }
        public string timeZone { get; set; }

        public Start(string _dateTime, string _timeZone)
        {
            dateTime = _dateTime;
            timeZone = _timeZone;
        }
    }

    public class End
    {
        public string dateTime { get; set; }
        public string timeZone { get; set; }

        public End(string _dateTime, string _timeZone)
        {
            dateTime = _dateTime;
            timeZone = _timeZone;
        }
    }

    public class ResponseStatus
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class Status
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }


    public class CreateEventResponse
    {
        public string odatacontext { get; set; }
        public string odataetag { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public List<object> categories { get; set; }
        public string originalStartTimeZone { get; set; }
        public string originalEndTimeZone { get; set; }
        public string iCalUId { get; set; }
        public int reminderMinutesBeforeStart { get; set; }
        public bool isReminderOn { get; set; }
        public bool hasAttachments { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string sensitivity { get; set; }
        public bool isAllDay { get; set; }
        public bool isCancelled { get; set; }
        public bool isOrganizer { get; set; }
        public bool responseRequested { get; set; }
        public object seriesMasterId { get; set; }
        public string showAs { get; set; }
        public string type { get; set; }
        public string webLink { get; set; }
        public object onlineMeetingUrl { get; set; }
        public ResponseStatus responseStatus { get; set; }
        public Body body { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public Location location { get; set; }
        public object recurrence { get; set; }
        public List<Attendee> attendees { get; set; }
        public Organizer organizer { get; set; }
        public class Address
        {
            public string street { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string countryOrRegion { get; set; }
            public string postalCode { get; set; }
        }
        public class Location
        {
            public string displayName { get; set; }
            public string locationEmailAddress { get; set; }
            public Address address { get; set; }
        }
    }
}
