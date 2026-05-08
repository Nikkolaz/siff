using MediatR;

namespace SSF.Interop.SIIFNacion.Domain.Queue.Events
{
    // This is an abstract class called Message that implements the IRequest<bool> interface
    // The class has a single property called MessageType that holds the type of the message
    public abstract class Message : IRequest<bool>
    {
        // This property holds the type of the message
        // The 'protected set' means that only the class itself and derived classes can set the value of this property
        public string MessageType { get; protected set; }

        // This is the constructor for the Message class
        // It sets the value of the MessageType property to the name of the class
        protected Message()
        {
            MessageType = GetType().Name;
        }
    }
}
