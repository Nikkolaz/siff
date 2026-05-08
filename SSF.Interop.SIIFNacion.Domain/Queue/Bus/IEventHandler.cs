using SSF.Interop.SIIFNacion.Domain.Queue.Events;

namespace SSF.Interop.SIIFNacion.Domain.Queue.Bus
{
    // This is an interface called IEventHandler that has a generic parameter TEvent
    // The interface defines a single method called Handle that takes an event of type TEvent
    // The 'in' keyword on the generic parameter indicates that TEvent is a contravariant type parameter
    // This means that a handler for a more derived type can be used to handle an event of a less derived type
    // The constraint 'where TEvent : Event' ensures that TEvent must inherit from the Event class
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : Event
    {
        // This method is called to handle an event of type TEvent
        // The method is async, which means it can run in the background
        Task Handle(TEvent @event);
    }

    // This is an empty interface called IEventHandler
    // It is used as a marker interface to indicate that a class is an event handler
    // This can be useful when using reflection to scan for event handlers
    public interface IEventHandler { }
}
