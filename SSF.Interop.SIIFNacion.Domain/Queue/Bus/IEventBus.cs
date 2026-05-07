using SSF.Interop.SIIFNacion.Domain.Queue.Events;

namespace SSF.Interop.SIIFNacion.Domain.Queue.Bus
{  // This is an interface called IEventBus
   // It defines the methods that any implementation of the event bus must provide

    public interface IEventBus
    {  
        // Este método envía un comando llamando al controlador correspondiente
        // El método es async, lo que significa que puede ejecutarse en segundo plano
        Task SendCommand<T>(T command) where T : Command.Command;

        // Este método publica un evento a todos los suscriptores
        Task Publish<T>(T @event, string Exchange) where T : Event;

    }
}
