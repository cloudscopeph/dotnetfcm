using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace dotnetfcm
{
    class Program
    {
        static void Main(string[] args)
        {
            ////////////////////////////////////////////////////////////////////
            // it is expected to have 3 args.  
            // - config file (text)
            // - message
            // - title
            ////////////////////////////////////////////////////////////////////
            if(args.Length != 3)
            {
                DisplayError("Need 3 arguments: (1) config file, (2) message, (3) title");
                return;
            }

            string configPath = args[0];
            string messageBody = args[1];
            string title = args[2];
            if(!File.Exists(configPath))
            {
                DisplayError($"File not found: {configPath}");
                return;
            }

            FcmConfig config = null;
            using(var reader = new StreamReader(File.OpenRead(configPath)))
            {
                config = ReadConfig(reader);
            }
            if(config == null) { return; }  //error will be displayed, so just exit

            Task.Run(() => SendFcmPushNotification(config, title, messageBody))
                .GetAwaiter()
                .GetResult();

            Console.WriteLine("End");
        }

        private static async Task SendFcmPushNotification(FcmConfig config, string title, string messageBody)
        {
            const string BASEURL = "https://fcm.googleapis.com/fcm/send";

            var messageInformation = new FcmMessage()
            {
                notification = new Notification()
                {
                    title = title,
                    body = messageBody
                },
                data = null,
                to = config.DestinationDeviceId
            };
            //Object to JSON STRUCTURE => using Newtonsoft.Json;
            string jsonMessage = JsonConvert.SerializeObject(messageInformation);

            // Create request to Firebase API
            var request = new HttpRequestMessage(HttpMethod.Post, BASEURL);
            request.Headers.TryAddWithoutValidation("Authorization", "key=" + config.ServerKey);
            request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
            HttpResponseMessage result;
            using (var client = new HttpClient())
            {
                result = await client.SendAsync(request);
            }

            Console.WriteLine("Result = " + result.StatusCode.ToString());
        }

        private static FcmConfig ReadConfig(StreamReader reader)
        {
            FcmConfig config = null;
            try
            {
                string serverKey = reader.ReadLine();
                string destinationDeviceId = reader.ReadLine();
                reader.Close();

                config = new FcmConfig { ServerKey = serverKey, DestinationDeviceId = destinationDeviceId };
            }
            catch(Exception ex)
            {
                DisplayError("ReadConfig", ex);
            }
            finally
            {
                reader.Close();
            }

            return config;
        }

        public static void DisplayError(string title, Exception ex)
        {
            Console.WriteLine($"*** EXCEPTION: {title}");
            Console.WriteLine(ex.Message);
        }

        public static void DisplayError(string message)
        {
            Console.WriteLine($"*** EXCEPTION: {message}");
        }

        /// <summary>
        /// Contains the configuration of the FCM Message such as the server key and the destination device id
        /// </summary>
        class FcmConfig
        {
            public string DestinationDeviceId { get; set; }
            public string ServerKey { get; set; }
        }
    }
}
