using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaServer.Controllers
{
    public class BaseController : UntypedActor
    {

        public class BaseAction : UntypedActor
        {
            public HttpListenerContext context { get; set; }
            
            public void WriteJsonResponse(object ResponseBody){
                var url = context.Request.Url;
                HttpListenerResponse response = context.Response;
                string responseString = Newtonsoft.Json.JsonConvert.SerializeObject(ResponseBody);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }

            protected override void OnReceive(object message)
            {
                context = (HttpListenerContext)message;
            }
        }

        public Dictionary<string, IActorRef> Actions { get; set; }
        public Dictionary<string, string> ActionRoutes { get; set; }

        public BaseController(){
            Actions = new Dictionary<string, IActorRef>();
            ActionRoutes = new Dictionary<string, string>();
        }

        protected override void OnReceive(object message)
        {
            IActorRef Action = GetAction(message);
            Action.Tell(message);
        }

        protected IActorRef GetAction(object message)
        {
            HttpListenerContext context = (HttpListenerContext)message;
            foreach (string name in ActionRoutes.Keys)
            {
                if (context.Request.Url.Segments[2].StartsWith(name, true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    return Actions[ActionRoutes[name]];
                }
            }
            return null;
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // withinTimeRange
                x => // localOnlyDecider
                {
                    //Maybe we consider ArithmeticException to not be application critical
                    //so we just ignore the error and keep going.
                    if (x is ArithmeticException)
                    {
                        Console.WriteLine("Resume");
                        return Directive.Resume;
                    }

                    //Error that we cannot recover from, stop the failing actor
                    else if (x is NotSupportedException)
                    {
                        Console.WriteLine("Resume");
                        return Directive.Stop;
                    }

                    //In all other cases, just restart the failing actor
                    else
                    {
                        Console.WriteLine("Resume");
                        return Directive.Restart;
                    }
                });
        }
    }
}
