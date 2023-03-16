﻿// ------------------------------------------------------------------------
// Copyright 2021 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

namespace ActorClient
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Dapr.Actors;
    using Dapr.Actors.Client;
    using Dapr.Actors.Communication;
    using IDemoActor;
    using IDemoActor.Models;
    using IDemoActorInterface;
    using Newtonsoft.Json;

    /// <summary>
    /// Actor Client class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var data = new MyData()
            {
                Name = "Binh",
                Phone = "0988401921",
            };

            // Create an actor Id.
            var actorId = new ActorId("BARBER_MANAGER");

            // Make strongly typed Actor calls with Remoting.
            // DemoActor is the type registered with Dapr runtime in the service.

            var proxy = ActorProxy.Create<IBarberManagerActor>(actorId, "BarberManagerActor");
            Console.WriteLine("Mak call using actor proxy to add barber.");
            await proxy.AddOrUpdateBarber(new Barber
            {
                Name = "John",
                Email = "tinhyeumaunang@gmail.com"
            });
            Console.WriteLine("Mak call using actor proxy to add barber.");
            await proxy.AddOrUpdateBarber(new Barber
            {
                Name = "Nancy",
                Email = "seetinh@gmail.com"
            });
            Console.WriteLine("Mak call using actor proxy to get list barber.");
            var barbers = await proxy.GetListBarber();

            Console.WriteLine($"List Babers {barbers.Count()}", JsonConvert.SerializeObject(barbers));

            // Making calls without Remoting, this shows method invocation using InvokeMethodAsync methods, the method name and its payload is provided as arguments to InvokeMethodAsync methods.
            Console.WriteLine("Registering the timer and reminder");
            await proxy.RegisterTimer();
            await proxy.RegisterReminder();
            Console.WriteLine("Waiting so the timer and reminder can be triggered");
            await Task.Delay(6000);

            // Track the reminder.
            var timer = new Timer(async state =>
            {
                var data = await proxy.GetListBarber();
                Console.WriteLine($"Received data: {data.Count()}");
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            await Task.Delay(TimeSpan.FromSeconds(21));
            await timer.DisposeAsync();

            Console.WriteLine("Deregistering timer. Timers would any way stop if the actor is deactivated as part of Dapr garbage collection.");
            await proxy.UnregisterTimer();
            Console.WriteLine("Deregistering reminder. Reminders are durable and would not stop until an explicit deregistration or the actor is deleted.");
            await proxy.UnregisterReminder();
            Console.ReadLine();
        }
    }
}
