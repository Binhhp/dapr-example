using Dapr.Actors.Client;
using Dapr.Actors;
using Dapr.Actors.Runtime;
using IDemoActor;
using IDemoActor.Models;
using System.Text.Json;

namespace MyService
{
    public class BarberManagerActor : Actor, IBarberManagerActor, IRemindable
    {
        public BarberManagerActor(ActorHost host) : base(host)
        {
        }
        protected override Task OnActivateAsync()
        {
            return Task.CompletedTask;
        }
        protected override Task OnDeactivateAsync()
        {
            return Task.CompletedTask;
        }
        private readonly string StateName = "CURENT_SNAPSHOT";
        public async Task AddOrUpdateBarber(Barber baber)
        {
            var guid = Guid.NewGuid().ToString();
            var actorId = new ActorId(guid);
            var proxy = ActorProxy.Create<IBarberActor>(actorId, "BarberActor");
            await proxy.AddOrUpdateBarber(baber);
            var state = await StateManager.TryGetStateAsync<List<string>>(StateName);
            var barberIds = new List<string>();
            if (state.HasValue)
            {
                barberIds = state.Value;
                barberIds.Add(guid);
            }
            else
            {
                barberIds.Add(guid);
            }
            await StateManager.SetStateAsync(StateName, barberIds);
        }

        public async Task<IEnumerable<Barber>> GetListBarber()
        {
            var barbers = new List<Barber>();
            var state = await StateManager.TryGetStateAsync<List<string>>(StateName);
            if(state.HasValue)
            {
                foreach(var barberId in state.Value)
                {
                    var proxy = ActorProxy.Create<IBarberActor>(new ActorId(barberId), "BarberActor");
                    var barberInfo = await proxy.GetBarber();
                    if(barberInfo != null) barbers.Add(barberInfo);
                }
            }
            return barbers;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var actorState = await StateManager.GetStateAsync<List<string>>(StateName);
            var barberId = Guid.NewGuid().ToString();
            actorState.Add(barberId);
            await StateManager.SetStateAsync(StateName, actorState);
            var proxy = ActorProxy.Create<IBarberActor>(new ActorId(barberId), "BarberActor");
            await proxy.AddOrUpdateBarber(new Barber
            {
                Name = Faker.Name.FullName(),
                Email = Faker.Internet.Email()
            });
        }


        public async Task RegisterReminder()
        {
            await this.RegisterReminderAsync("TestReminder", null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public async Task RegisterReminderWithTtl(TimeSpan ttl)
        {
            await this.RegisterReminderAsync("TestReminder", null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), ttl);
        }

        public async Task RegisterReminderWithRepetitions(int repetitions)
        {
            await this.RegisterReminderAsync("TestReminder", null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), repetitions);
        }

        public async Task RegisterReminderWithTtlAndRepetitions(TimeSpan ttl, int repetitions)
        {
            await this.RegisterReminderAsync("TestReminder", null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1), repetitions, ttl);
        }

        public Task UnregisterReminder()
        {
            return this.UnregisterReminderAsync("TestReminder");
        }

        class TimerParams
        {
            public int IntParam { get; set; }
            public string StringParam { get; set; }
        }

        /// <inheritdoc/>
        public Task RegisterTimer()
        {
            var timerParams = new TimerParams
            {
                IntParam = 100,
                StringParam = "timer test",
            };

            var serializedTimerParams = JsonSerializer.SerializeToUtf8Bytes(timerParams);
            return this.RegisterTimerAsync("TestTimer", nameof(this.TimerCallback), serializedTimerParams, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        }

        public Task RegisterTimerWithTtl(TimeSpan ttl)
        {
            var timerParams = new TimerParams
            {
                IntParam = 100,
                StringParam = "timer test",
            };

            var serializedTimerParams = JsonSerializer.SerializeToUtf8Bytes(timerParams);
            return this.RegisterTimerAsync("TestTimer", nameof(this.TimerCallback), serializedTimerParams, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), ttl);
        }

        public Task UnregisterTimer()
        {
            return this.UnregisterTimerAsync("TestTimer");
        }
        public async Task TimerCallback(byte[] data)
        {
            var state = await StateManager.GetStateAsync<List<string>>(StateName);
            var barberId = Guid.NewGuid().ToString();
            state.Add(barberId);
            await StateManager.SetStateAsync(StateName, state);
            var proxy = ActorProxy.Create<IBarberActor>(new ActorId(barberId), "BarberActor");
            await proxy.AddOrUpdateBarber(new Barber
            {
                Name = Faker.Name.FullName(),
                Email = Faker.Internet.Email()
            });
        }
    }
}
