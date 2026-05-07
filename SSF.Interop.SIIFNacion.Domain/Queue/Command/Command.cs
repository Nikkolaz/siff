using SSF.Interop.SIIFNacion.Domain.Queue.Events;

namespace SSF.Interop.SIIFNacion.Domain.Queue.Command
{
    // This is an abstract class called Command that inherits from the Message class
    // The class has a single property called Timestamp that holds the time the command was created
    public abstract class Command : Message
    {
        // This property holds the timestamp of when the command was created
        // The 'protected set' means that only the class itself and derived classes can set the value of this property
        public DateTime Timestamp { get; protected set; }

        // This is the constructor for the Command class
        // It sets the value of the Timestamp property to the current time
        protected Command()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
