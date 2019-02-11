using System.ComponentModel;

namespace Essensplan.Models.Responses
{
    public enum FehlerTypen
    {
        [Description("Kein Speiseplan vorhanden du Penner")] NoSpeisePlan = 1,
        [Description("Kein Menü vorhanden")]NoEssensDetails = 2,
        [Description("Fehler aufgetreten")] Fehler = 3,
        [Description("Ihre Anfrage konnte nicht verarbeitet werden")] FehlerAnfrage = 4,
        [Description("Connext-Campus beendet")] Ended = 5,
    }
}
