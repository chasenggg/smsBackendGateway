using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;

namespace smsBackendGateway.Models
{
    public class Sms
    {
        //public int contact_id { get; set; }
        public string phoneNumber { get; set; }
        public string message { get; set; }

        public int messageId { get; set; }
        public string? sender { get; set; }
        public string? date { get; set; }

        public Sms(string phoneNumber, string message, int messageId, string sender, string dateTime)
        {
            this.phoneNumber = phoneNumber;
            this.message = message; 
            this.messageId = messageId; 
            this.sender = sender; 
            this.date = dateTime;
        }

        public Sms(){}

        public Sms(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
        }
    }

  
}
