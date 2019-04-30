using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using RestSharp;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// The web server code is from the Simple C# Web Server example here - https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
// The web server is an example for development purposes only - you will need to create your own server that meets your 
// standards for your project and environment. 

namespace WebServer
{
   public class WebServer
   {
      private readonly HttpListener _listener = new HttpListener();
      private readonly Func<HttpListenerRequest, string> _responderMethod;
 
      public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
      {
         if (!HttpListener.IsSupported)
         {
            throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
         }
             
         // URI prefixes are required eg: "http://localhost:8080/test/"
         if (prefixes == null || prefixes.Count == 0)
         {
            throw new ArgumentException("URI prefixes are required");
         }
         
         if (method == null)
         {
            throw new ArgumentException("responder method required");
         }
 
         foreach (var s in prefixes)
         {
            _listener.Prefixes.Add(s);
         }
 
         _responderMethod = method;
         _listener.Start();
      }
 
      public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
         : this(prefixes, method)
      {
      }
 
      public void Run()
      {
         ThreadPool.QueueUserWorkItem(o =>
         {
            Console.WriteLine("Webserver running...");
            try
            {
               while (_listener.IsListening)
               {
                  ThreadPool.QueueUserWorkItem(c =>
                  {
                     var ctx = c as HttpListenerContext;
                     try
                     {
                        if (ctx == null)
                        {
                           return;
                        }
                            
                        var rstr = _responderMethod(ctx.Request);
                        var buf = Encoding.UTF8.GetBytes(rstr);
                        ctx.Response.ContentLength64 = buf.Length;
                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                     }
                     catch
                     {
                        // ignored
                     }
                     finally
                     {
                        // always close the stream
                        if (ctx != null)
                        {
                           ctx.Response.OutputStream.Close();
                        }
                     }
                  }, _listener.GetContext());
               }
            }
            catch (Exception ex)
            {
               // ignored
            }
         });
      }
 
      public void Stop()
      {
         _listener.Stop();
         _listener.Close();
      }
   }

   // Code for your server interacting with TeleSign's Voice API is provided here.  
   internal class Program
   {
      // Defining events that TeleSign can send back. There are more than these, but these are all that are needed for 
      // this example. You can learn more about events here: https://enterprise.telesign.com/api-reference/apis/voice/api-reference

      const string INCOMING_CALL = "incoming_call";
      const string SPEAK_COMPLETED = "speak_completed";
      public static string SendResponse(HttpListenerRequest request)
      {

         // Here you grab an incoming request and convert it to a string you can process and retrieve the kind of event TeleSign 
         // sends you. 
         string post_body;
         using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
             post_body = reader.ReadToEnd();
         }

         string postBodyEvent;
         postBodyEvent = ParseResponse(post_body, "event");

         // This checks if an incoming event from TeleSign is an incoming call event. If so, it sends an action string to TeleSign that
         // a message be spoken and digits collected from the customer hearing the message. 

         if (postBodyEvent == INCOMING_CALL)
         {
            Action actionObject = new Action();
            string action = actionObject.Speak("Hello, how can we help you today? Press 1 for Customer Service. Press 2 for the Finance Department.", "1", "en-US");
            return action;
         }

         // This checks if an incoming event from TeleSign is a speak completed event. If so, it checks if the customer pressed 
         // 1 or 2. If the customer pressed 1, it sends an action string to TeleSign that it should connect the customer to the 
         // Customer Service Department. If the customer pressed 2, it sends an action string to TeleSign that it should connect
         // the customer to the Finance Department. 

         if (postBodyEvent == SPEAK_COMPLETED)
         {
            JObject collectDigits = JObject.Parse(post_body);
            string value = (string)collectDigits["data"]["collected_digits"]; 
            string callerIdNum = "Your CallerID number";
            string customerServiceNum = "Your Customer Service Number";
            string financeDeptNum = "Finance Department Number"; 

            if (value == "1")
            {
               //Dial Customer Service 
               Action actionObject = new Action();
               string action = actionObject.Dial(customerServiceNum, callerIdNum);
               return action;
            }
            if (value == "2")
            {
               //Dial the Finance Department
               Action actionObject = new Action();
               string action = actionObject.Dial(financeDeptNum, callerIdNum);
               return action;
            }

            
         }

         return "Unknown Code Path";
      }

      // This is part of the server code, and shows you your server is running and gives you a way to easily stop it.  
      private static void Main(string[] args)
      {
         var ws = new WebServer(SendResponse, "http://localhost:8080/test/");
         ws.Run();
         Console.WriteLine("A simple webserver. Press a key to quit.");
         Console.ReadKey();
         ws.Stop();
      }

      // This is an optional method you can use to print out response details to see if a response is working or not. 
      static void debugPrint(IRestResponse response)
      {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(response))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(response);
                Console.WriteLine("{0}={1}", name, value);
            }
      }

      // This is for retrieving event information. 
      static string ParseResponse(string apiResponse, string nameOfKey)
      {
         JObject o = JObject.Parse(@apiResponse);
         string value = (string)o[nameOfKey];   
         return value;         
      }


      // This class sets up the string you'll need to send to represent an action POST request to TeleSign. 
      // It adds information needed to create the request to a JSON object, then converts the JSON object to a string 
      // that can be sent. 
      public class Action 
      {
            public string Dial(string phoneNumber, string callerIdNumber)
                {
                    string dialString = @"
                        {
                            ""method"": ""dial"",
                            ""params"": {
                                ""to"": ""number"",
                                ""caller_id_number"": ""caller_id""

                        }
                    }";
                    JObject dialObject = JObject.Parse(@dialString);
                    dialObject["params"]["to"] = phoneNumber;  
                    dialObject["params"]["caller_id_number"] = callerIdNumber;
                    dialString = dialObject.ToString();
                return dialString;
                }
            
            public string Speak(string message, string collectDigit = "0", string languageTag = "en-US")
            {
                if (collectDigit == "0") { 
                    string speakString = @"
                     {
                        ""method"": ""speak"",
                        ""params"": {
                        ""tts"": {
                            ""message"": ""your message"",
                            ""language"": ""en-US""
                         },
                        }
                     }";
                    JObject speakObject = JObject.Parse(@speakString); 
                    speakObject["params"]["tts"]["message"] = message;
                    speakObject["params"]["tts"]["language"] = languageTag;
                    speakString = speakObject.ToString();
                    return speakString;
                }

                string speakCollectString = @"
                {
                  ""method"": ""speak"",
                  ""params"": {
                    ""tts"": {
                       ""message"": ""your message"",
                        ""language"": ""en-US""
                    },
                    ""collect_digits"": {
                    ""max"": 1
                    }
                  }
                }";
                JObject speakCollectObject = JObject.Parse(@speakCollectString); 
                speakCollectObject["params"]["tts"]["message"] = message;
                speakCollectObject["params"]["tts"]["language"] = languageTag;
                speakCollectObject["params"]["collect_digits"]["max"] = collectDigit;
                speakCollectString = speakCollectObject.ToString();

            return speakCollectString;
 
          }
         
        } 

   }

}      