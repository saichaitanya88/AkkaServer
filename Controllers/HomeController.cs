using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaServer.Controllers
{
    public class HomeController : BaseController
    {
        #region Messages
        public class Number : BaseAction
        {
            protected override void OnReceive(object message)
            {
                base.OnReceive(message);
                WriteJsonResponse("Number!");
            }
        }
        public class String : BaseAction
        {

            protected override void OnReceive(object message)
            {
                base.OnReceive(message);
                WriteJsonResponse("String!");
            }
        }
        #endregion

        public HomeController() : base()
        {
            Props props = Props.Create<Number>();
            Actions.Add("Number", Context.ActorOf(Props.Create<Number>()));
            ActionRoutes.Add("number", "Number");
            Actions.Add("String", Context.ActorOf(Props.Create<String>()));
            ActionRoutes.Add("string", "String");
        }

        protected override void OnReceive(object message)
        {
            base.OnReceive(message);
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }
}
