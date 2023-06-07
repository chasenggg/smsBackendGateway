using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using smsBackendGateway.Models;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace smsBackendGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class SmsController : ControllerBase
    {
        private SerialPort serialport = new SerialPort("COM4", 115200);
        private string[] ports = SerialPort.GetPortNames();

        private readonly ILogger<SmsController> _logger;

        //database
        //private readonly TodoAppDatabaseContext _dbcontext;
        //public SmsController(TodoAppDatabaseContext context)
        //{
        //    _dbcontext = context;
     
        //}

        public SmsController(ILogger<SmsController> logger)
        {
            _logger = logger;
            this.serialport.Parity = Parity.None;
            this.serialport.DataBits = 8;
            this.serialport.StopBits = StopBits.One;
            this.serialport.Handshake = Handshake.RequestToSend;
            this.serialport.DtrEnable = true;
            this.serialport.RtsEnable = true;
            this.serialport.NewLine = System.Environment.NewLine;
        }

        private List<Sms> ParseSmsMessages(string response)
        {
            int ctr = 0;
            int index;
            int messageId = -1;
            string? date = null;
            string? textmsg = null;
            string? message = null;
            string? sender = null;

            List<Sms> messages = new List<Sms>();

            // Split the response into lines.
            string[] lines = response.Split('\n');

            foreach (string line in lines)
            {
                if (line.StartsWith("+CMGL"))
                {
                    ctr = 0;
                    string regexSender = @"D\"",\""\d+\""|D\"",\""\D+\""";
                    string regexDate = @"\""\d+\/\d+\/\d+,\d+:\d+:\d+\+\d+\""";

                    index = line.IndexOf(',');

                    messageId = index;

                    Regex rxSender = new Regex(regexSender);
                    Regex rxDate = new Regex(regexDate);

                    //sender = "+" + line.Substring(line.IndexOf('6'),12);

                    sender = line.ToString();

                    Match match = rxSender.Match(sender);

                    if (match.Success)
                    {
                        sender = "+" + match.ToString();
                    }

                    //date = line.Substring(line.IndexOf(",,"), 23); //date and time

                    date = line.ToString();

                    Match matchDate = rxDate.Match(date);

                    if (match.Success)
                    {
                        date = matchDate.ToString();
                    }

                    ctr++;
                }
                else
                {
                    if (ctr == 1)
                    {
                        //textmsg = line.ToString();
                        message = line.ToString();
                        messages.Add(new Sms("1", message, messageId, sender, date));
                        ctr = 0;
                    }
                }
            }
            return messages;
        }

        [HttpGet]
        public string GetAllMessages()
        {
            serialport.Open();

            serialport.WriteLine(@"AT" + (char)(13));
            Thread.Sleep(200);

            serialport.WriteLine("AT+CMGF=1\r");
            Thread.Sleep(200);

            serialport.WriteLine("AT+CNMI=1,1,0,0\r");
            Thread.Sleep(200);

            // List ALL messages
            serialport.WriteLine("AT+CMGL=\"ALL\"\r");
            Thread.Sleep(10000); // 10 seconds

            // Read the response from the modem.
            string response = serialport.ReadExisting();

            // Parse the response to get the list of SMS messages.
            List<Sms> messages = ParseSmsMessages(response);

            List<Sms> messageList = new List<Sms>();
            // Display the list of SMS messages.
            foreach (Sms message in messages)
            {
                //Contruct our Rows
                messageList.Add(new Sms("1", message.message, message.messageId, message.sender, message.date));
            }

            serialport.Close();

            return JsonSerializer.Serialize(messageList);
        }

        private List<Sms> ParseSmsContacts(string recipient)
        {
            int ctr = 0;
            double number;

            List<Sms> contacts = new List<Sms>();

            for(int i = 0; i < ctr; i++)
            {
                Console.WriteLine("Number: ");
                number = Convert.ToDouble(Console.ReadLine());

            }
           
            return contacts;
        }


        [HttpPost]
        public string Send([FromBody] Sms sms)
        {
            serialport.Open();

            serialport.WriteLine(@"AT" + (char)(13));
            Thread.Sleep(1000);

            serialport.WriteLine("AT+CMGF=1\r");
            Thread.Sleep(1000);

            serialport.WriteLine("AT+CMGS=\"" + sms.phoneNumber + "\"\r\n");
            Thread.Sleep(1000);

            serialport.WriteLine(sms.message + "\x1A");
            Thread.Sleep(1000);

            string recipient = serialport.ReadExisting();

            List<Sms> contacts = ParseSmsContacts(recipient);

            serialport.Close();

            return "200";
        }
    }
}
