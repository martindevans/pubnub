using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using com.pubnub.api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Example
{
    class Program
    {
        const string ChatChannel = "com_pubnub_api_chat_example";
        static bool Running = true;
        static Guid Uuid = Guid.NewGuid();
        static AutoResetEvent handle;
        static readonly object sync = new object();
        static Dictionary<Guid, string> people = new Dictionary<Guid, string>();
        static Dictionary<Guid, DateTime> peopleHistory = new Dictionary<Guid, DateTime>();

        static void Main(string[] args)
        {
            Console.WriteLine("What is your chat name?");
            var chatName = Console.ReadLine();

            // Setup a Chatroom
            var pubnub = new Pubnub("demo", "demo", string.Empty);

            // Setup an event handler
            pubnub.MessageRecieved += (s, e) =>
            {
                Task.Factory.StartNew(() =>
                {
                    if (e.Channel == Program.ChatChannel)
                    {
                        var message = JsonConvert.DeserializeObject<ChatMessage>(e.Message);
                        if (message.FromUuid != Uuid)
                        {
                            switch (message.Type)
                            {
                                case "ping":
                                    UpdateTime(message.FromUuid);
                                    break;

                                case "leaving":
                                    if (people.ContainsKey(message.FromUuid))
                                        lock (sync)
                                            people.Remove(message.FromUuid);

                                    break;

                                case "joined":
                                    if (!people.ContainsKey(message.FromUuid))
                                    {
                                        lock (sync)
                                            people.Add(message.FromUuid, message.From);
                                        Console.WriteLine(message.From + " has joined the chat");
                                    }
                                    UpdateTime(message.FromUuid);
                                    break;

                                case "message":
                                    UpdateTime(message.FromUuid);
                                    Console.WriteLine(string.Format("{0} said:\n{1}",
                                                                    message.From,
                                                                    message.Message));

                                    break;
                            }
                        }
                    }                                                        
                });
            };

            pubnub.Publish(Program.ChatChannel, new ChatMessage
            {
                FromUuid = Uuid,
                From = chatName,
            });

            if (!pubnub.Subscribe(Program.ChatChannel))
                throw new Exception("Could not subscribe to channel: " + Program.ChatChannel);

            pubnub.Publish(Program.ChatChannel, new ChatMessage
            {
                Type = "joined",
                From = chatName,
                FromUuid = Uuid
            });

            // to keep all the other clients up to date, let's
            // remind them we're here every 30 seconds, otherwise 
            // they'll forget we exist within 60 seconds
            Task.Factory.StartNew(() => {

                using (handle = new AutoResetEvent(false))
                {
                    while (Running)
                    {
                        // let everyone know that we're still here
                        pubnub.Publish(Program.ChatChannel, new ChatMessage
                        {
                            Type = "ping",
                            From = chatName                
                        });

                        // check our list of people, and if there is anyone
                        // who hasn't checked in over 60 seconds.
                        foreach (var kvp in peopleHistory)
                        {
                            if ((DateTime.Now - kvp.Value).TotalSeconds >= 60)
                                lock (sync)
                                    if (people.ContainsKey(kvp.Key))
                                        people.Remove(kvp.Key);
                        }

                        // now remove any people from the history list, who 
                        // are not in the regular list 
                        lock (sync)
                        {
                            var list = peopleHistory.Keys.ToList();
                            for (var i = 0; i < peopleHistory.Keys.Count; i++)
                                if (!people.ContainsKey(list[i]))
                                    peopleHistory.Remove(list[i]);                            
                        }
                        handle.WaitOne(new TimeSpan(0, 0, 30));
                    }
                }

            });

        start:
            var read = Console.ReadLine();
            switch (read.ToLower())
            {
                case "!pm":
                    // send a 'private' message to someone (not really private
                    // as the message is delivered to all clients, but only 
                    // displayed on one)


                    goto start;

                case "!q":
                case "!quit":
                    break;

                default:
                    // by default let's send a message to the chat channel
                    // Send a disconnection message
                    pubnub.Publish(Program.ChatChannel, new ChatMessage
                    {
                        Type = "message",
                        From = chatName,
                        FromUuid = Uuid,
                        Message = read
                    });

                    goto start;
            }

            // Stop the announcement loop
            Running = false;
            if (handle != null)
                handle.Set();

            // Send a disconnection message
            pubnub.Publish(Program.ChatChannel, new ChatMessage
            {
                Type = "leaving",
                From = chatName,
                FromUuid = Uuid
            });

        }

        static void UpdateTime(Guid remoteUuid)
        {
            lock(sync)
            {
                if (!peopleHistory.ContainsKey(remoteUuid))
                    peopleHistory.Add(remoteUuid, DateTime.Now);
                else 
                    peopleHistory[remoteUuid] = DateTime.Now;
            }
        }

    }
}
