using System.ComponentModel;

namespace Essensplan.Models.Responses
{
    public enum SkillTypen
    {
        [Description("Heute gibt es folgende Menüs")] Heutiges = 0,
        [Description("Essensplan für den ")]SpeisePlan = 1,
        [Description("Details zu ")]EssensDetail = 2,
        [Description("Folgende Menüs sind in der Kalenderwoche")]WocheNachKategorie = 3,
        Ended = 12,
        Error = 13,
    }
}
