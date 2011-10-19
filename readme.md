PUBNUB C# API
=================

This API is a client for the PUBNUB messaging and broadcast system, which is 
basically a HTTP reliable long lived connection/comet based message bus for any device or application. The 
API's provided didn't quite suit the services we had, and we wanted to understand 
more about the system so we produced this API.

To get started -- checkout http://www.pubnub.com, get yourself an account and 
start sending messages.


Usage
-----

This API is built for clean simple access to PUBNUB which automatically deals with threading and
allows subscriptions to multiple channels. It's only depedency is JSON.NET (http://james.newtonking.com/pages/json-net.aspx / 
http://json.codeplex.com/)

Here is an example of setting up configuration: 

``` csharp
using com.pubnub.api;

var config = new PubnubConfiguration();
config.SubscribeKey = "your-subscribe-key";
config.PublishKey = "your-publish-key";
config.SecretKey = "your-secret-key";

// Apply the configuration to an instance of the pubnub class
var pubnub = new Pubnub(config);

```

Here is an example of publishing a message:

``` csharp

// using alternative config method
var pubnub = new Pubnub("your-publish-key", "your-subscribe-key", "your-secret-key");

// Send an anonymous object, returns a boolean based on the server return code
var success = pubnub.Publish("example_channel", new
{
    Name = "John Appleseed",
    Age = 30, 
    Height = "6.1"
});

```

Here is an example of subscribing to a channel:

``` csharp

// setup pubnub
var pubnub = new Pubnub("your-publish-key", "your-subscribe-key", "your-secret-key");

// setup an event handler
pubnub.MessageRecieved += (s, e) =>
{
    // it's up to us to determine if we want a fire and forget model
    // any processing done here will block the resubscribe of the pubnub,
    // channel we might want this, in this case we don't so it sets off a 
    // task to do all the processing that results from the recieved message,
    // freeing the pubnub subscribe thread to reconnect to the server and get 
    // any new messages.
    Task.Factory.StartNew(() =>
    {
        switch (e.Channel)
        {
            case "my_channel":
                Console.WriteLine("Do something with the message");
                Console.WriteLine(e.Message);
                break;

            default:
                break;
        }
    });
};

// returns a boolean if the subscribe was successful, this class
// prevents multiple subscriptions to the same channel. (Not globally
// just within each instance)
var success = pubnub.Subscribe("my_channel");

```

For more info, refer to the docs!

Copyright and license
---------------------

Copyright 2011 PressF12 Pty Ltd

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this work except in compliance with the License.
You may obtain a copy of the License in the LICENSE file, or at:

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.