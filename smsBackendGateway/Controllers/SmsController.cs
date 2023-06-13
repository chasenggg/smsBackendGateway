using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using smsBackendGateway.Models;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NuGet.Protocol.Plugins;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace smsBackendGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class SmsController : ControllerBase
    {
        private SerialPort serialport = new SerialPort("COM4", 115200);
        private string[] ports = SerialPort.GetPortNames();

        private readonly ILogger<SmsController> _logger;
        private readonly SmsContext _dbContext;

        public SmsController(ILogger<SmsController> logger, SmsContext DbContext)
        {
            _logger = logger;
            _dbContext = DbContext;
            this.serialport.Parity = Parity.None;
            this.serialport.DataBits = 8;
            this.serialport.StopBits = StopBits.One;
            this.serialport.Handshake = Handshake.RequestToSend;
            this.serialport.DtrEnable = true;
            this.serialport.RtsEnable = true;
            this.serialport.NewLine = System.Environment.NewLine;

            string connectionString = "Data Source=uphmc-dc33; Initial Catalog=ITWorksSMS; TrustServerCertificate=True; User ID=dalta; Password=dontshareit";
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

                try
                {
                    string connectionString = "Data Source=uphmc-dc33; Initial Catalog=ITWorksSMS; TrustServerCertificate=True; User ID=dalta; Password=dontshareit";

                    SqlConnection connection = new SqlConnection(connectionString);
                    // Open the connection
                    connection.Open();

                    SqlCommand command;
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    command = connection.CreateCommand();

                    //String sql = "INSERT INTO contacts (contact_fname, contact_no ) VALUES (3, '" +message.sender + "')";
                    String sql = "";


                    command = new SqlCommand(sql, connection);
                    adapter.InsertCommand = new SqlCommand(sql, connection);
                    adapter.InsertCommand.ExecuteNonQuery();


                    // Close the connection
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    // Handle any errors that occurred during the connection process
                    Console.WriteLine("An error occurred while connecting to the database: " + ex.Message);
                }
            }
            return messages;
        }

        [HttpGet]
        [Route("ReceiveMessage")]
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

        [HttpGet]
        [Route("Select")]
        public string Test()
        {
            string connectionString = "Data Source=uphmc-dc33; Initial Catalog=ITWorksSMS; TrustServerCertificate=True; User ID=dalta; Password=dontshareit";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM contacts", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return reader.GetValue(2).ToString();
                            }
                        }
                    }


                    // Close the connection
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    // Handle any errors that occurred during the connection process
                    Console.WriteLine("An error occurred while connecting to the database: " + ex.Message);
                }
            }
            return "200";
        }

        [HttpGet]
        [Route("Insert")]
        public string Test2()
        {
            string connectionString = "Data Source=uphmc-dc33; Initial Catalog=ITWorksSMS; TrustServerCertificate=True; User ID=dalta; Password=dontshareit";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    SqlCommand command;
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    String sql = ("INSERT INTO contacts (employee_no, contact_lname, contact_fname, contact_mname, contact_no) VALUES('777H', 'ABAD', 'JUNALD', 'ORO', '+639176321177')");

                    command = new SqlCommand(sql, connection);
                    adapter.InsertCommand = new SqlCommand(sql, connection);
                    adapter.InsertCommand.ExecuteNonQuery();

                    // Close the connection
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    // Handle any errors that occurred during the connection process
                    Console.WriteLine("An error occurred while connecting to the database: " + ex.Message);
                }
            }
            return "200";
        }

        private List<Sms> ParseSmsContacts(string response)
        {
            int messageId = -1;
            string? date = null;
            string? message = null;
            string? sender = null;

            List<Sms> contacts = new List<Sms>();

            string[] number = response.Split(';');

            foreach (string phoneNumbers in number)
            {
                Console.WriteLine(sender = phoneNumbers.ToString());

            }
            contacts.Add(new Sms("1", message, messageId, sender, date));
            return contacts;
        }

        [HttpPost]
        [Route("SendMessage")]
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

            string response = serialport.ReadExisting();

            List<Sms> contacts = ParseSmsContacts(response);

            List<Sms> contactsList = new List<Sms>();
            foreach (Sms contact in contacts)
            {
                contactsList.Add(new Sms(contact.phoneNumber));
            }

            serialport.Close();

            try
            {
                string connectionString = "Data Source=uphmc-dc33; Initial Catalog=ITWorksSMS; TrustServerCertificate=True; User ID=dalta; Password=dontshareit";

                SqlConnection connection = new SqlConnection(connectionString);
                // Open the connection
                connection.Open();

                SqlCommand command;
                SqlDataAdapter adapter = new SqlDataAdapter();

                command = connection.CreateCommand();

                String sql = "INSERT INTO sms_queue (contact_id, sms_message) VALUES (3, '" + sms.message + "')";


                command = new SqlCommand(sql, connection);
                adapter.InsertCommand = new SqlCommand(sql, connection);
                adapter.InsertCommand.ExecuteNonQuery();

                // Close the connection
                connection.Close();
            }
            catch (SqlException ex)
            {
                // Handle any errors that occurred during the connection process
                Console.WriteLine("An error occurred while connecting to the database: " + ex.Message);
            }

            return JsonSerializer.Serialize(contactsList);

        }

    }
}
