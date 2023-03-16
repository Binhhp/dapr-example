using Dapr.Actors.Runtime;
using IDemoActor;
using IDemoActor.Models;

namespace MyService
{
    public class BarberActor : Actor, IBarberActor
    {
        public BarberActor(ActorHost host) : base(host)
        {
        }

        private readonly string StateName = "CURENT_SNAPSHOT";
        public async Task AddOrUpdateBarber(Barber barber)
        {
            await this.StateManager.SetStateAsync(StateName, barber);
        }

        public async Task<Barber> GetBarber()
        {
            return await StateManager.GetStateAsync<Barber>(StateName);
        }
    }
}
