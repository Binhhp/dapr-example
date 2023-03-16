using Dapr.Actors;
using IDemoActor.Models;

namespace IDemoActor
{
    public interface IBarberActor : IActor
    {
        Task AddOrUpdateBarber(Barber barber);
        Task<Barber> GetBarber();
    }
}
