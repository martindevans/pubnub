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
allows subscriptions to multiple channels. 

Here is an example of publishing a message:

``` c#

var pubnub = new Pubnub();

```

Here is an example of subscribing to a channel:

``` csharp

var pubnub = new Pubnub();


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