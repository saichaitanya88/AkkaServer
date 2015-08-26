using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaServer.Controllers;

namespace AkkaServer.Actors
{
    public class RouteCoordinator : UntypedActor
    {

        #region Messages
        public class RouteCoordinatorStart { }



        #endregion
        Dictionary<string, IActorRef> Controllers { get; set; }
        Dictionary<string, string> Routes { get; set; }
        public RouteCoordinator()
        {
            Controllers = new Dictionary<string, IActorRef>();
            Props consoleReaderProps = Props.Create<HomeController>();
            Controllers.Add("Home", Context.ActorOf(consoleReaderProps));
            Routes = new Dictionary<string, string>();
            Routes.Add("home/", "Home");
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
                    if (x is ArithmeticException) return Directive.Resume;

                    //Error that we cannot recover from, stop the failing actor
                    else if (x is NotSupportedException) { 
                        return Directive.Stop; 
                    }

                    //In all other cases, just restart the failing actor
                    else return Directive.Restart;
                });
        }

        protected override void OnReceive(object message)
        {
            // Create a listener.
            HttpListener listener = new HttpListener();
            if (message is Messages.RouteCoordinatorStart)
            {
                // Add the prefixes. 
                listener.Prefixes.Add("http://localhost:80/");
                listener.Start();
                Console.WriteLine("Listening...");
                while (true)
                {
                    // Note: The GetContext method blocks while waiting for a request. 
                    HttpListenerContext context = listener.GetContext();
                    RouteRequest(context);
                }
            }
            if (message == "stop")
            {
                listener.Stop();
            }
        }

        protected void RouteRequest(HttpListenerContext context)
        {
            IActorRef ctrlRef = GetController(context);
            ctrlRef.Tell(context);
        }

        protected IActorRef GetController(HttpListenerContext context)
        {
            foreach (string key in Routes.Keys)
            {
                if (context.Request.Url.Segments[1].StartsWith(key, true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    return Controllers[Routes[key]];
                }
            }

            return null;
        }
    }
}
