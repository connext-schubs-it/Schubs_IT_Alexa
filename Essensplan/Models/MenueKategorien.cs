using System.ComponentModel;

namespace Essensplan.Models
{
    public enum MenueKategorien
    {
        [Description("Suppe")] Suppe = 0,
        [Description("Menü 1")] Menue_1 = 1,
        [Description("Menü 2")] Menue_2 = 2,
        [Description("Vegetarisch")] Vegetarisch = 3,
        [Description("Beilagensalat")] Salat_1 = 4,
        [Description("Großer Salat")] Salat_2 = 5,
    }
}
