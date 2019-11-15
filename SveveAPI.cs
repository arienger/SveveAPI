using System;
using System.Collections.Generic;
using System.Web; 
using System.Net;
using System.Text;
using System.Xml;


namespace SveveDemo
{
    public class SveveAPI
    {
        private string username;
        private string password;
        private string SEND_MSG_URL = "https://sveve.no/SMS/SendMessage";

        /// <summary>
        /// Constructor with username and password to Sveve SMS gateway
        /// </summary>
        public SveveAPI(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Send SMS message through the Sveve HTTP API
        /// </summary>
        /// <returns>Returns an object with the following parameters: msgOkCount, stdSmsCount, ids, fatalErrors, errors</returns>
        /// <param name="to">Message destination address. Mobile number and/or group name</param>
        /// <param name="message">Message text</param
        /// <param name="from">Message sender address. Mobile number or small text, e.g. company name (max. 11 characters)</param>
        public Result sendSMS(string to, string message, string from)
        {
            // Build the URL request for sending SMS
            string url = SEND_MSG_URL +
                "?user=" + HttpUtility.UrlEncode(username) +
                "&passwd=" + HttpUtility.UrlEncode(password) +
                "&to=" + HttpUtility.UrlEncode(to, Encoding.GetEncoding("UTF-8")) +
                "&msg=" + HttpUtility.UrlEncode(message, Encoding.GetEncoding("UTF-8")) +
                "&from=" + HttpUtility.UrlEncode(from);

            // Send the SMS by submitting the URL request to the server. The response is saved as an XML string
            string serverResponse = DownloadString(url);

            // Converts the XML response from the server into a structured response object
            return ParseServerResult(serverResponse);
        }

        /// <summary>
        /// Downloads the URL from the server, and returns the response as string
        /// </summary>
        /// <param name="URL"></param>
        /// <returns>Returns the http/xml response as string</returns>
        /// <exception cref="WebException">WebException is thrown if there is a connection problem</exception>
        private string DownloadString(string url)
        {
            using (System.Net.WebClient wlc = new System.Net.WebClient())  // Create WebClient instance
            {                
                try
                {
                    // Download and return the xml response
                    return wlc.DownloadString(url);
                }
                catch (WebException ex)
                {
                    // Failed connecting to server
                    throw new WebException("Error occurred while connecting to server: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Parses the XML code and returns a Result object.
        /// </summary>
        /// <param name="serverResponse">XML data from a request through HTTP API</param>
        /// <returns>Returns a Result object with the parsed data.</returns>
        private Result ParseServerResult(string serverResponse)
        {
            XmlDocument xmlDoc = new XmlDocument();            
            xmlDoc.LoadXml(serverResponse);
            Result result = new Result();
            XmlNode response = xmlDoc.GetElementsByTagName("sms")[0].FirstChild;  // get the <response> element

            foreach (XmlNode node in response.ChildNodes)
            {
                if (node.Name == "msg_ok_count")
                {
                    result.msgOkCount = int.Parse(node.InnerText);
                }
                else if (node.Name == "std_sms_count")
                {
                    result.stdSmsCount = int.Parse(node.InnerText);
                }
                else if (node.Name == "ids")
                {
                    foreach(XmlNode idNode in node.ChildNodes)
                    {
                        result.ids.Add(int.Parse(idNode.InnerText));
                    }
                }
                else if (node.Name == "errors")
                {
                    foreach (XmlNode errorNode in node.ChildNodes)
                    {
                        if (errorNode.Name == "fatal")
                        {
                            result.fatalErrors.Add(errorNode.InnerText);
                        }
                        else if (errorNode.Name == "error")
                        {
                            long number = long.Parse(errorNode.ChildNodes[0].InnerText);
                            string numberErrMsg = errorNode.ChildNodes[1].InnerText;
                            NumberError error = new NumberError() { number = number, error = numberErrMsg };
                            result.errors.Add(error);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The Result object from the SendSMS function
        /// </summary>
        public class Result
        {
            public int msgOkCount { get; set; }
            public int stdSmsCount { get; set; }
            public List<int> ids = new List<int>();
            public List<string> fatalErrors = new List<string>();
            public List<NumberError> errors = new List<NumberError>();
        }
        public class NumberError
        {
            public long number { get; set; }
            public string error { get; set; }
        }

    }
}
