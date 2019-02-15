using System.ComponentModel;

namespace Essensplan.Models.Responses
{
    public enum SkillTypen
    {
        [Description("Heute gibt es folgende Menüs")] Heutiges = 0,
        [Description("Essensplan für ")]SpeisePlan = 1,
        [Description("Details zu ")]EssensDetail = 2,
        [Description("Folgende Menüs sind in der Kalenderwoche")]WocheNachKategorie = 3,
        [Description("Willkommen beim Connext Campus.")]Willkommen = 4,
        [Description("Was kann ich für Sie tun?")]Stop = 5,
        [Description("Die Menüs kosten")]Preis = 6,
        Ended = 12,
        Error = 13,
    }
}
