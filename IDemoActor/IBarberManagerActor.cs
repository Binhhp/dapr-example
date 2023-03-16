using Dapr.Actors;
using IDemoActor.Models;

namespace IDemoActor
{
    public interface IBarberManagerActor : IActor
    {
        Task AddOrUpdateBarber(Barber baber);
        Task<IEnumerable<Barber>> GetListBarber();
        Task RegisterReminder();
        Task RegisterReminderWithTtl(TimeSpan ttl);
        Task UnregisterReminder();
        Task RegisterTimer();
        Task RegisterTimerWithTtl(TimeSpan ttl);
        Task UnregisterTimer();
    }
}
