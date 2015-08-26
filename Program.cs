using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Actor;
using AkkaServer.Actors;

namespace AkkaServer
{
    class Program
    {
        public static ActorSystem actorSystem;
        static void Main(string[] args)
        {
            actorSystem = ActorSystem.Create("myActorSystem");
            Props props = Props.Create<RouteCoordinator>();
            var routerActor = actorSystem.ActorOf(props, "RouteCoordinator");

            routerActor.Tell(new Messages.RouteCoordinatorStart());

            actorSystem.AwaitTermination();
        }
    }
}
