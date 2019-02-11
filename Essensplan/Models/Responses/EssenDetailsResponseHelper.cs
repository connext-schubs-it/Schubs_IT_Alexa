using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive.Templates.Types;
using AssistServer.Extension;
using AssistServer.Models.Api.Alexa.Response;
using Essensplan.Extensions;
using Essensplan.Klassen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Essensplan.Models.Responses
{
    public class EssenDetailsResponseHelper : AlexaResponseHelper
    {
        public static SkillResponse GetEssenDetailsResponse(SkillRequest request, List<SpeisePlan> wochenPlan, int kategorie, DateTime? tag, int id)
        {
            var pageToken = SkillTypen.EssensDetail;
            var tagesPlan = new List<SpeisePlan>();
            var date = new DateTime();
            var menue = new SpeisePlan();

            if (id != -1)
            {
                date = tag.Value.Date;
                tagesPlan = wochenPlan.FindAll(p => p.Datum.Date == date);

                if (tagesPlan != null)
                    menue = tagesPlan.Find(m => m.Kategorie == kategorie);
            }
            else            
                menue = wochenPlan.Find(p => p.Id == id);            

            if (menue != null)
            {                
                var content = CreateContent(menue);

                var speech = CreateSpeech(menue, date);
                var card = CreateCard(menue, date);
                var template = CreateTemplate(pageToken, content, kategorie, date);

                return CreateBodySkillResponse(request, pageToken, template, speech, card, date, null);
            }
                        
            return CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.NoEssensDetails.ToDescription(), "", null, date, false);                        
        }

        private static IOutputSpeech CreateSpeech(SpeisePlan menue, DateTime tag)
        {
            var text = "";
            var kategorie = ((MenueKategorien)menue.Kategorie).ToDescription();
            if (tag.Date == DateTime.Now.Date)
                text = $"{kategorie} ist heute: {menue.Beschreibung}.";
            else
                text = $"{kategorie} ist am {tag.Date.SayAsDate()}: {menue.Beschreibung}.";

            return new SsmlOutputSpeech { Ssml = $"<speak>{text}</speak>" };
        }

        private static ICard CreateCard(SpeisePlan menue, DateTime tag)
        {
            var url = skillParameter.FirstOrDefault(t => t.Typ == SkillTypen.EssensDetail)?.UrlCard;
            var title = ((MenueKategorien)menue.Kategorie).ToDescription();
            var content = $"{menue.Beschreibung}, für {menue.Preis} €";

            if (tag != null)
                title += $" für {tag.Date.ToShortDateString()}";

            var cardImage = new CardImage
            {
                SmallImageUrl = url,
                LargeImageUrl = url
            };

            return new StandardCard
            {
                Title = ((MenueKategorien)menue.Kategorie).ToDescription(),
                Content = content,
                Image = cardImage
            };
        }

        private static BodyTemplate2 CreateTemplate(SkillTypen typ, SkillBodyContent content, int kategorie, DateTime tag)
        {
            var title = ((MenueKategorien)kategorie).ToDescription();            

            if (tag != null)                
                title += $" für {tag.Date.ToShortDateString()}";

            return AddBodyContent(typ, content, title);
        }

        private static SkillBodyContent CreateContent(SpeisePlan menue)
        {
            return new SkillBodyContent
            {
                Primaer = TextStyle.SetFont3(menue.Beschreibung),
                Sekundaer = TextStyle.SetFont2($"Preis: {menue.Preis.ToString("F2")} €"),
                Tertiaer = "",
                ImageUrl = "",
            };
        }
    }
}
