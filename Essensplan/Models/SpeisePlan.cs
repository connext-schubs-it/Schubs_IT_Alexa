using System;

namespace Essensplan.Klassen
{
    public class SpeisePlan
    {
        public int Id { get; set; }
        public string Beschreibung { get; set; }
        public double Preis { get; set; }
        public int Kategorie { get; set; }
        public DateTime Datum { get; set; }
    }
}
