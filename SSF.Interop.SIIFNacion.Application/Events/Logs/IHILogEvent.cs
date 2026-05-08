using SSF.Interop.SIIFNacion.Domain.Queue.Events;

namespace SSF.Interop.SIIFNacion.Application.Events.Logs
{
    // This is a class called IhiLogEvent that inherits from the Event class
    // The class represents a logging event and contains properties for various log fields
    public class IHILogEvent : Event
    {
        // This property holds the CUS (Transaction ID) associated with the event
        public Guid CUS { get; set; }

        // This property holds the Client Private Transaction ID associated with the event
        public string TransactionId { get; set; } = string.Empty;

        // This property holds the API Sender ID associated with the event
        public int SenderId { get; set; }

        // This property holds the API Receiver ID associated with the event
        public int ReceiverId { get; set; }

        // This property holds the log message associated with the event
        public string? Message { get; set; }

        // This property holds the message layout associated with the event
        public string? MessageLayout { get; set; }

        // This property holds the request payload associated with the event
        public string? Request { get; set; }

        // This property holds the response payload associated with the event
        public string? Response { get; set; }

        // This property holds the log level associated with the event
        public string? Level { get; set; }

        // This property holds the exception details associated with the event
        public string? Exception { get; set; }

        // This property holds any additional properties and methods associated with the event
        public string? Property { get; set; }

        public int ClientId { get; set; }

        // This is the Constructor for the IhiLogEvent class
        // It initializes the properties of the class with the values passed in as arguments
        public IHILogEvent(Guid cus, string transactionId, int senderId, int receiverId, string? message, string? messageLayout, string? request, string? response, string? level, string? exception, string? property, int clientId)
        {
            CUS = cus;
            TransactionId = transactionId;
            SenderId = senderId;
            ReceiverId = receiverId;
            Message = message;
            MessageLayout = messageLayout;
            Request = request;
            Response = response;
            Level = level;
            Exception = exception;
            Property = property;
            ClientId = clientId;
        }
    }
}
