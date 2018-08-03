using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace EventReader
{
    class O365Client
    {
        private ConfidentialClientApplication ConfClientApp;
        private WebClient wClient;
        private const string AUTHORITY = "https://login.microsoftonline.com/{TENANTNAME}/v2.0";


        //findRoomLists requires a userID and hence picked a user: 6ed27880-3d21-4713-bc60-c5b95e6ca908
        private const string RoomListsEP = "https://graph.microsoft.com/beta/users/<REPLACE User GUID>/findRoomLists";
        private const string RoomsEP = "https://graph.microsoft.com/beta/users/<REPLACE User GUID>/findRooms";
        private const string EventsEP = "https://graph.microsoft.com/v1.0/users/{UPN}/events?$select=id,organizer,attendees,start,end,location&$top=500";
        private const string CreateEventEP = "https://graph.microsoft.com/v1.0/users/{UPN}/events";

        //

        private static string[] _Graphscopes = new string[] { "https://graph.microsoft.com/.default" };
        private string lastMsg;
        private AuthenticationResult _authResult = null;
        private Boolean bGraphSignedIn = false;

        public RoomList roomList; 
        public Dictionary<String,RoomList> rooms;
        public Dictionary<String, EventList> RoomEvents;


        public string GetLastMsg()
        {
            return lastMsg;
        }

        public O365Client(string clientid, string clientSecret, string tenantname)
        {
            String authority = AUTHORITY.Replace("{TENANTNAME}", tenantname);
            ConfClientApp = new ConfidentialClientApplication(clientid, authority, "urn:ietf:wg:oauth:2.0:oob", new ClientCredential(clientSecret), null,null);
        }

        public async Task<Boolean> SignInToGraph()
        {
            try
            {
                _authResult = await ConfClientApp.AcquireTokenForClientAsync(_Graphscopes, true);
                bGraphSignedIn = true;
            }
            catch (Exception ex)
            {
                lastMsg = $"Error Acquiring Token:{System.Environment.NewLine}{ex}";
                bGraphSignedIn = false;
            }

            if (bGraphSignedIn)
            {
                //Spin up WebClient
                wClient = new WebClient(_authResult.AccessToken);
            }


            return bGraphSignedIn;
        }

        public async Task<int> LoadRoomList()
        {
            string retMsg;
            int nRoomCount = 0;

            roomList = new RoomList();
            try
            {
                retMsg = await wClient.GET(RoomListsEP);
                //If the response from Graph is an Error an Exception will be thrown...
                this.roomList = JsonConvert.DeserializeObject<RoomList>(retMsg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return roomList.Rooms.Count;
        }

        public async Task<int> LoadRooms()
        {
            string retMsg;
            int nRoomCount = 0;
            Boolean bRoomListFound = false;

            rooms = new Dictionary<String, RoomList>();
            if (this.roomList == null)
            {
                int nRlistcount = await LoadRoomList();
                if (nRlistcount >= 1 )
                {
                    //We have Room lists
                    //Iterate through it...
                    bRoomListFound = true;
                    retMsg = await wClient.GET(RoomListsEP);
                    //If the response from Graph is an Error an Exception will be thrown...
                    this.roomList = JsonConvert.DeserializeObject<RoomList>(retMsg);
                    if (this.roomList != null)
                    {
                        foreach (var rlo in this.roomList.Rooms)
                        {
                            retMsg = await wClient.GET($"{RoomsEP}/{rlo.Address}");
                            RoomList r = JsonConvert.DeserializeObject<RoomList>(retMsg);
                            rooms.Add(rlo.Name, r);
                            nRoomCount += r.Rooms.Count;
                        }
                    }
                }
            }

            if (!bRoomListFound)
            {
                try
                {
                    retMsg = await wClient.GET(RoomsEP);
                    RoomList r = JsonConvert.DeserializeObject<RoomList>(retMsg);
                    rooms.Add("ALL", r);
                    nRoomCount += r.Rooms.Count;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }


            return nRoomCount;
        }


        public List<Event> GetEvents(String upn)
        {
            return RoomEvents[upn].events;
        }

        public List<Room> GetRooms(String roomlist)
        {
            if (roomlist == null)
            {
                roomlist = "ALL";
            }
            return this.rooms[roomlist].Rooms;
        }

        public async Task<int> LoadEvents(String upn)
        {
            String retMsg;
            String iterativeEP;

            EventList el = new EventList();
            iterativeEP = EventsEP.Replace("{UPN}", upn);
            RoomEvents = new Dictionary<string, EventList>();
            //
            try
            {
                while (true)
                {
                    retMsg = await wClient.GET(iterativeEP);
                    Console.WriteLine($"{iterativeEP}");
                    //If there is an Error, an exception will be thrown....
                    EventList tempel = JsonConvert.DeserializeObject<EventList>(retMsg);
                    //For each event, change the TimeZone to CST
                    foreach (Event e in tempel.events)
                    {

                        DateTime dt = DateTime.Parse(e.start.dateTime);
                        dt = ConvertDtfromUTCtoCST(dt);
                        e.start.timeZone = "Central Standard Time";
                        e.start.dateTime = dt.ToString();

                        dt = DateTime.Parse(e.end.dateTime);
                        dt = ConvertDtfromUTCtoCST(dt);
                        e.end.timeZone = "Central Standard Time";
                        e.end.dateTime = dt.ToString();

                    }
                    el.events.AddRange(tempel.events);
                    if (tempel.NextPageLink == null)
                        break;
                    else
                        iterativeEP = tempel.NextPageLink;
                }
                RoomEvents.Add(upn, el);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return 0;
        }

        private DateTime ConvertDtfromUTCtoCST(DateTime utcDtTm)
        {
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                TimeZoneInfo ustZone = TimeZoneInfo.Utc;
                DateTime cstTime = TimeZoneInfo.ConvertTime(utcDtTm, ustZone, cstZone);
                return cstTime;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        public async Task<string> CreateEvent2(Event evt, String upn)
        {
            String endPoint = CreateEventEP.Replace("{UPN}", upn);
            //Generate JSON for CalEvent;
            String calEventJSON = JsonConvert.SerializeObject(evt);
            var payload = new StringContent(calEventJSON, Encoding.UTF8, "application/json");

            string retMsg = await wClient.POST_CST(endPoint, payload);

            if (!retMsg.Contains("error"))
            {
                lastMsg = "";
                //Convert JSON to .NET
                CreateEventResponse evtResponse = JsonConvert.DeserializeObject<CreateEventResponse>(retMsg);
                retMsg = $"Created Meeting with Subject {evtResponse.subject}";
            }
            lastMsg = retMsg;
            return retMsg;

        }


    }
}
