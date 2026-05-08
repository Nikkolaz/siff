namespace SSF.Interop.SIIFNacion.Domain.Queue.Events
{
    // This is an abstract class called Event
    // The class has a single property called Timestamp that holds the time the event occurred
    public abstract class Event
    {
        // This property holds the timestamp of when the event occurred
        // The 'protected set' means that only the class itself and derived classes can set the value of this property
        public DateTime Timestamp { get; protected set; }

        // This is the constructor for the Event class
        // It sets the value of the Timestamp property to the current time
        protected Event()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
