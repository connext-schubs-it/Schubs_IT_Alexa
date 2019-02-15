using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Essensplan.Models
{
    public class Gericht
    {
        public int ID { get; set; }
        public string Bezeichnung { get; set; }
        public string ImagePfad { get; set; }
        public string Kategorie { get; set; }
        public decimal Preis { get; set; }
    }
}
