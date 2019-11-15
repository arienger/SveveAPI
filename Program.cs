using System;


namespace SveveDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            string username = "brukernavn";
            string password = "passord";            
            string to = "12345678";  // comma-separated list of mobile numbers and/or group names
            string message = "Test av SMS via Sveve HTTP API\nNy linje";
            string from = "Avsender";  // mobile number or text (max. 11 characters)

            SveveAPI sveveAPI = new SveveAPI(username, password);  // construct api object

            try
            {
                // Send SMS through HTTP API
                SveveAPI.Result result = sveveAPI.sendSMS(to, message, from);  // send SMS

                // Outout send SMS response                               
                Console.WriteLine("Antall meldinger sent OK: " + result.msgOkCount);
                Console.WriteLine("Antall SMS enheter brukt: " + result.stdSmsCount);
                foreach (var msgId in result.ids)
                {
                    Console.WriteLine("SMS id: " + msgId); 
                }

                foreach (var fatalErr in result.fatalErrors)
                {
                    Console.WriteLine("Fatal error: " + fatalErr);
                }

                foreach (var err in result.errors)
                {
                    Console.WriteLine("Error: " + err.number + " - " + err.error);
                }

            }
            catch (System.Net.WebException ex)
            {
                // Catch error occurred while connecting to server
                Console.WriteLine("Error sending SMS: " + ex.Message);
            }

            Console.ReadKey();
        }
    }

}
