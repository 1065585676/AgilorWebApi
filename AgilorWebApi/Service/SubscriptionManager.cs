using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Agilor.Interface;

namespace AgilorWebApi.Service
{
    public class SubscriptionManager
    {
        private ACI aci = null;
        public SubscriptionManager(ref ACI aci)
        {
            this.aci = aci;
        }



    }
}