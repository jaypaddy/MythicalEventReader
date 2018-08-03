using System;
using System.Collections;
using System.Collections.Generic;

namespace EventReader
{
    class Program
    {
        static void Main(string[] args)
        {


            String CosmosEndPointUri = "https://mythical.documents.azure.com:443/";
            String CosmosEventsDatabaseId = "Mythical";
            //Move all of this to Azure Key Vault.
            String CosmosKey = "<REPLACE COSMOSDBKEY>";
            String CosmosCollId = "MythicalEvents";

            //App Registered in Tenant's Azure AD
            //Move all of this to Azure Key Vault.
            String ClientId = "<REPLACE CLIENT ID REGISTERED IN Azure AD>";
            String ClientSecret = "<REPLACE CLIENT SECRET>";
            String TenantName = "<REPLACE TENANTID>.onmicrosoft.com";


            bool NotExitWhile = true;
            string givenCMD;
            string[] command;

            //I should make AvlCommands and CmdStatus a single String Array...
            string[] Avlcommands = {"readevents",    "book",   "exit" };

            Console.WriteLine("Connecting to Microsoft Graph");
            //Connect to Office365
            O365Client oClient = new O365Client(ClientId, ClientSecret, TenantName);
            //Signin to Graph
            oClient.SignInToGraph().Wait();
            Console.WriteLine("****Connected to Microsoft Graph****");
            Console.WriteLine("Enter any of the following commands... to view all commands type list");
            foreach (var cmd in Avlcommands)
            {
                Console.WriteLine($"\t{cmd}");
            }
            while (NotExitWhile)
            {
                Console.Write($"SmartSpaces:>");
                givenCMD = Console.ReadLine();
                //Parse the Command by space
                command = givenCMD.Split(' ');
                switch (command[0])
                {
                    case "list":
                        {
                            foreach (var cmd in Avlcommands)
                            {
                                Console.WriteLine($"\t{cmd}");
                            }
                            break;
                        }


                    case "readevents":
                        {
                            //Connect to CosmosDB
                            CosmosDBClient<Event> cosmosClient = new CosmosDBClient<Event>();
                            cosmosClient.Connect(CosmosEndPointUri, CosmosEventsDatabaseId, CosmosCollId, CosmosKey);
                            //
                            int nI = 0;

                            oClient.LoadRooms().Wait();
                            foreach (Room r in oClient.GetRooms(null))
                            {
                                oClient.LoadEvents(r.Address).Wait();
                                foreach (Event e in oClient.GetEvents(r.Address))
                                {
                                    cosmosClient.CreateItemAsync(e).Wait();
                                    Console.WriteLine($"{nI} - {e.location.displayName} - {e.start.dateTime.ToString()} - {cosmosClient.GetLastMsg()}");
                                    nI++;
                                }
                                Console.WriteLine($"{r.Name} - {nI} Bookings");
                            }

                            Console.WriteLine($"THE END - Press Enter to Exit");
                            Console.ReadLine();
                            break;
                        }


                    case "book":
                        {
                            int nMtgDuration = 4;
                            String tenant = "@" + TenantName;
                            String[] desks = { "desk101", "desk102", "desk103" };
                            Console.WriteLine($"Booking Date is  {DateTime.Now.ToString("yyyy-MM-dd")}");
                            Console.WriteLine($"Meeting Duration is set to {nMtgDuration} hrs.");
                            Console.WriteLine($"Please specify a Desk:");
                            int nI = 0;
                            foreach (String desk in desks)
                            {
                                Console.WriteLine($"{nI}.{desk}");
                                nI++;
                            }
                            String selDesk = Console.ReadLine();
                            int nDesk;
                            Boolean result = Int32.TryParse(selDesk, out nDesk);
                            if (!result) nDesk = 1;
                            selDesk = desks[nDesk] + tenant;

                            Console.WriteLine($"Please specify Booking start time (8-5):");
                            string mtgStartTime = Console.ReadLine();
                            int nStartTime;
                            result  = Int32.TryParse(mtgStartTime, out nStartTime);
                            if (!result) nStartTime = 9;

                            Console.WriteLine($"Please specify Booking Subject:");
                            string mtgSubject = Console.ReadLine();


                            //Not checking for Errors....
                            int nEndTime = nStartTime + 1;
                            //Format a String as follows
                            //2018-06-20 00:00:00
                            //"6/16/2018 6:30:00 AM"
                            string startDtTm = $"{DateTime.Now.ToString("yyyy-MM-dd")} {nStartTime}:00:00 AM";
                            string endDtTm = $"{DateTime.Now.ToString("yyyy-MM-dd")} {nEndTime}:00:00 AM";

                            Event calevent = new Event();

                            //Loop through the Attendees for the Meeting
                            calevent.subject = $"SmartSpaces Booking - {mtgSubject} on {startDtTm}";
                            calevent.body = new Body();
                            calevent.body.content = $"Welcome to Microsoft SmartSpaces............... ";
                            calevent.body.contentType = $"HTML";
                            calevent.start = new Start(startDtTm, "Central Standard Time");
                            calevent.end = new End(endDtTm, "Central Standard Time");

                            calevent.attendees = new List<Attendee>();
                            calevent.attendees.Add(new Attendee(new EmailAddress($"{desks[nDesk]}", $"{selDesk}"), "Resource"));
                            calevent.location = new EventLocation($"{desks[nDesk]}", null);
                            String mtgDuration = $"PT{nMtgDuration}H";

                            Console.WriteLine("Checking for Suggestions to pick from...");
                            oClient.CreateEvent2(calevent, "SOMEBODY@COMPANY.COM").Wait();
                            Console.WriteLine(oClient.GetLastMsg());
                            Console.ReadLine();
                            break;
                        }


                    case "exit":
                        {
                            Console.WriteLine("Sign Out...");
                            NotExitWhile = false;
                            break;
                        }

                }

            }

            return;
        }


        static void CreateEvent()
        {

        }
    }
}