using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using AssistServer.Extension;
using AssistServer.Models.Api.Alexa.Response;
using Essensplan.Extensions;
using Essensplan.Klassen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Essensplan.Models.Responses
{
    public class WocheNachKategorieResponseHelper : AlexaResponseHelper
    {
        private static CultureInfo cInfo = new CultureInfo("de-De");

        public static SkillResponse GetWocheNachKategorieResponse(SkillRequest request, List<SpeisePlan> wochenPlan, int kategorie, int kw)
        {            
            var items = new List<ListItem>();
            var pageToken = SkillTypen.WocheNachKategorie;
            var plaene = wochenPlan.FindAll(p => p.Kategorie == kategorie);

            if (plaene != null)
            {
                foreach (var menue in plaene)
                {
                    var tag = cInfo.DateTimeFormat.GetDayName(menue.Datum.DayOfWeek);
                    var item = AddListItemWithImage(pageToken.ToString(), menue.Id, TextStyle.SetFont2(menue.Beschreibung), tag, "", "");
                    items.Add(item);
                }

                var preis = plaene[0].Preis;
                var speech = CreateSpeech(pageToken, plaene, kategorie, kw);
                var title = CreateTitle(pageToken, kategorie, preis, kw);
                var card = CreateCard(pageToken, plaene, title);
                return CreateListSkillResponse(request, pageToken, items, speech, card, title, DateTime.Now, null);
            }

            return CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.NoSpeisePlan.ToDescription(), "", null, DateTime.Now, false);
        }

        private static IOutputSpeech CreateSpeech(SkillTypen typ, List<SpeisePlan> items, int kategorie, int kw)
        {
            var text = "";
            if (items.Count > 0)
            {
                var i = 0;
                var missingDates = GetMissingDays(items);
                var menue = ((MenueKategorien)kategorie).ToDescription();
                text += $"{typ.ToDescription()} {kw} für {menue} eingeplant. ";

                foreach (var item in items)
                {
                    if (i == 0)
                        text += item.Beschreibung;
                    else if (i == items.Count - 1)
                        text += $", sowie {item.Beschreibung}. ";
                    else
                        text += $". {item.Beschreibung}";

                    i++;
                }

                if (missingDates.Count > 0)                
                    text += CreateMissingDatesSpeech(missingDates, menue);               
            }
            else
                FehlerTypen.NoSpeisePlan.ToDescription();

            return new SsmlOutputSpeech { Ssml = $"<speak>{text}</speak>" };
        }

        private static ICard CreateCard(SkillTypen typ, List<SpeisePlan> items, string title)
        {           
            var url = skillParameter.FirstOrDefault(t => t.Typ == typ)?.UrlCard;
            var content = "";

            if (items.Count > 0)
            {
                var i = 1;
                foreach (var item in items)
                {
                    content += $"{i}. {item.Beschreibung}\n";
                    i++;
                }
            }
            else
                content = FehlerTypen.NoSpeisePlan.ToDescription();

            var cardImage = new CardImage
            {
                SmallImageUrl = url,
                LargeImageUrl = url
            };

            return new StandardCard
            {
                Content = content,
                Title = title,
                Image = cardImage
            };
        }

        private static string CreateTitle(SkillTypen typ, int kategorie, double preis, int kw)
        {
            var menue = ((MenueKategorien)kategorie).ToDescription();
            var text = $"{skillParameter.FirstOrDefault(t => t.Typ == typ)?.CardTitle} {kw}";

            return $"{menue} {text} ({preis.ToString("F2")} €)";
        }

        private static string CreateMissingDatesSpeech(List<DateTime> missingDates, string menue)
        {
            var dates = "";
            var i = 0;
            foreach (var date in missingDates)
            {
                var tag = cInfo.DateTimeFormat.GetDayName(date.DayOfWeek);
                if (i == 0)
                    dates += tag;
                else if (i == missingDates.Count - 1)
                    dates += $", und {tag}. ";
                else
                    dates += $". {tag}";

                i++;
            }

            return $"Am {dates} ist für {menue} nichts geplant.";
        }

        private static List<DateTime> GetMissingDays(List<SpeisePlan> items)
        {
            var dates = new List<DateTime>();
            var missingDates = new List<DateTime>();
            var daysOfWeek = items[0].Datum.GetDaysOfWeek();

            foreach (var item in items)
                dates.Add(item.Datum);

            foreach (var day in daysOfWeek)
            {
                if (!dates.Contains(day.Date))
                    missingDates.Add(day);
            }

            return missingDates;
        }
    }
}
