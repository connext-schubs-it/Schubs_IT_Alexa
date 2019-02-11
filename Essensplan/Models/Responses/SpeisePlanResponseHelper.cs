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
    public class SpeisePlanResponseHelper : AlexaResponseHelper
    {
        public static SkillResponse GetSpeisePlanResponse(SkillRequest request, List<SpeisePlan> wochenPlan, DateTime? tag)
        {
            var items = new List<ListItem>();
            string datum = tag.ToString();
            //var date = new DateTime(2019, 01, 7);
            var date = (DateTime)tag;
            var today = String.Format("{0:MM/dd/yyyy}", date) == String.Format("{0:MM/dd/yyyy}", DateTime.Now.Date);
            var pageToken = today ? SkillTypen.Heutiges : SkillTypen.SpeisePlan;
            var tagesPlan = wochenPlan.FindAll(p => String.Format("{0:MM/dd/yyyy}", p.Datum.Date) == String.Format("{0:MM/dd/yyyy}", date));

            if (tagesPlan != null)
            {
                foreach (var menue in tagesPlan)
                {
                    var item = AddListItemWithImage(pageToken.ToString(), menue.Id, TextStyle.SetFont2(menue.Beschreibung), $"{menue.Preis.ToString("F2")} €", "", "");
                    items.Add(item);
                }

                var speech = CreateSpeech(pageToken, tagesPlan, date, today);
                var card = CreateCard(pageToken, tagesPlan, date);
                var title = CreateTitle(pageToken, date, today);

                return CreateListSkillResponse(request, pageToken, items, speech, card, title, date, null);
            }

            return CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.NoSpeisePlan.ToDescription(), "", null, date, false);
        }

        private static IOutputSpeech CreateSpeech(SkillTypen typ, List<SpeisePlan> items, DateTime tag, bool today)
        {
            var text = "";
            if (items.Count > 0)
            {
                if (today)
                    text += $"{typ.ToDescription()}. ";
                else
                    text += $"{typ.ToDescription()} {tag.SayAsDateYear()}. ";

                text += CreateEssensPlanSpeech(items);                
            }
            else
                text += FehlerTypen.NoSpeisePlan.ToDescription();

            return new SsmlOutputSpeech { Ssml = $"<speak>{text}</speak>" };
        }

        private static ICard CreateCard(SkillTypen typ, List<SpeisePlan> items, DateTime tag)
        {
            var url = skillParameter.FirstOrDefault(t => t.Typ == typ)?.UrlCard;
            var title = $"{skillParameter.FirstOrDefault(t => t.Typ == typ)?.CardTitle} {tag.ToShortDateString()}";
            var content = "";

            if (items.Count > 0)
            {
                var i = 1;
                foreach (var item in items)
                {
                    content += $"{i}. {item.Beschreibung} für {item.Preis} €\n";
                    i++;
                }
            }
            else
                content += FehlerTypen.NoSpeisePlan.ToDescription();

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

        private static string CreateTitle(SkillTypen typ, DateTime tag, bool today)
        {
            var title = skillParameter.FirstOrDefault(t => t.Typ == typ)?.CardTitle;
            if (!today)
                title += $" {tag.ToShortDateString()}";

            return title;
        }

        private static string CreateEssensPlanSpeech(List<SpeisePlan> items)
        {
            var menue_1 = items.Find(i => i.Kategorie == (int)MenueKategorien.Menue_1).Beschreibung;
            var menue_2 = items.Find(i => i.Kategorie == (int)MenueKategorien.Menue_2);
            var vegetarisch = items.Find(i => i.Kategorie == (int)MenueKategorien.Vegetarisch).Beschreibung;
            var suppe = items.Find(i => i.Kategorie == (int)MenueKategorien.Suppe).Beschreibung;

            var text = $"{menue_1}";
            if (menue_2 != null)
                text += $", oder { menue_2.Beschreibung}. ";
            else
                text += ". ";

            text += $"Das vegetarische Menü ist {vegetarisch}, ";
            text += $"und als Vorspeise gibt es {suppe}.";

            return text;
        }
    }
}
