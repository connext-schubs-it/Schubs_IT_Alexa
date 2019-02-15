using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Essensplan.Models
{
    public class SpeisePlanDB
    {
        public DateTime Datum { get; set; }
        public Gericht[] Gerichte { get; set; }
    }
}
