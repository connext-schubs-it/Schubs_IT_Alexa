using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AssistServer.Extension;
using AssistServer.Extension.NewFolder;
using AssistServer.Models.Api.Alexa.Response;
using Essensplan.Extensions;
using Essensplan.Klassen;
using Essensplan.Models;
using Essensplan.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Essensplan.Controllers
{
    [Route("api/[controller]")]
    public class AlexaController : ControllerBase
    {
        private readonly string api = "http://10.100.252.20/api/speiseplan/kw/"; // Denise Testdatenbank
        private readonly int defaultValue = -1;

      // ##############################################################################################################
      /// <summary>
      /// Gibt Speisepläne an hand der Kalenderwoche und des Jahres zurück
      /// </summary>
      /// <param name="kw">Legt die Kalenderwoche des Jahres fest in der die Speisepläne angezeigt werden</param>
      /// <param name="year">Legt die das Jahr fest in der die Speisepläne angezeigt werden</param>
      /// <returns></returns>
      private async Task<List<SpeisePlan>> GetSpeisePlaene(int kw, int year)
        {
            var client = new HttpClient();
            var speisePlaene = new List<SpeisePlan>();
            var path = $"{api}{kw}/{year}";
            path = "https://cx-schubsit.connext.de/api/speiseplan/kw/7"; // Markus Datenbank. Pläne müssen mit dem SpeiseplanConverter konvertiert werden
            //path = "https://cx-hotel.connext.de:8080/api/speiseplan/kw/2/2019";  // Dennis Datenbank
            //var path = $"{api}7/2019";
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var speisePlaeneDB = JsonConvert.DeserializeObject<List<SpeisePlanDB>>(await response.Content.ReadAsStringAsync());
                speisePlaene = SpeisePlanConverter(speisePlaeneDB);
                speisePlaene = speisePlaene.FindAll(s => s.Kategorie != (int)MenueKategorien.Salat_1);
                speisePlaene = speisePlaene.FindAll(s => s.Kategorie != (int)MenueKategorien.Salat_2);
            }

            return speisePlaene;
        }

        // ##############################################################################################################
        /// <summary>
        /// Entscheidet von welche Art die Anfrage ist und ruft die entsprechende Methode dafür auf
        /// </summary>
        /// <param name="anfrage">Enthält die Anfrage vom Amazon Alexa Server</param>
        /// <returns></returns>
        [HttpPost]
        public dynamic Alexa([FromBody]SkillRequest anfrage)
        {
            try
            {
                if (anfrage.Context.System.ApiAccessToken == null)
                    return new BadRequestResult();

                var antwort = AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, false);
                var requestType = anfrage.GetRequestType();

                if (requestType == typeof(LaunchRequest))
                    antwort = StartVerwalter(anfrage);

                else if (requestType == typeof(IntentRequest))
                    antwort = KommandoVerwalter(anfrage);

                else if (requestType == typeof(SessionEndedRequest))
                    antwort = SitzungBeendenVerwalter(anfrage);

                else if (requestType == typeof(DisplayElementSelectedRequest))
                    antwort = ElementKlickVerwalter(anfrage);

                return antwort;
            }
            catch (Exception e)
            {
                CreateErrorLog(e);
                return null;
            }
        }

      // ##############################################################################################################
      /// <summary>
      /// Entscheidet welche Art des Kommandos gesprochen wurde
      /// </summary>
      /// <param name="anfrage">Enthält die Anfrage vom Amazon Alexa Server vom Typ Kommando</param>
      /// <returns></returns>
      private SkillResponse KommandoVerwalter(SkillRequest anfrage)
        {
            var antwort = AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, false);
            var intentRequest = (IntentRequest)anfrage.Request;

            if (intentRequest.Intent.Name.Equals("SpeisePlanKommando"))
                antwort = SpeisePlanKommando(anfrage);
            else if (intentRequest.Intent.Name.Equals("TagUndKategorieKommando"))
                antwort = TagUndKategorieKommando(anfrage);
            else if (intentRequest.Intent.Name.Equals("WocheNachKategorieKommando"))
                antwort = WocheNachKategorieIntent(anfrage);
            else if (intentRequest.Intent.Name.Equals("PreisKommando"))
                antwort = PreisIntent(anfrage);
            else if (intentRequest.Intent.Name.Equals("AMAZON.CancelIntent"))
                antwort = SitzungBeendenVerwalter(anfrage);
            else if (intentRequest.Intent.Name.Equals("AMAZON.StopIntent"))
                antwort = AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Stop, SkillTypen.Stop.ToDescription(), "", null, DateTime.Now, false);

            return antwort;
        }

      // ##############################################################################################################
      /// <summary>
      /// Enthält die Antwort des Alexa Skills welche beim Starten des Skills gegeben wird
      /// </summary>
      /// <param name="anfrage">Enthält die Anfrage vom Amazon Alexa Server vom Typ Start</param>
      /// <returns></returns>
      private SkillResponse StartVerwalter(SkillRequest anfrage)
        {
            string text = "Herzlich Willkommen!";
            string title = "Connext Campus";
            string speech = "Willkommen beim Connext Campus. Was kann ich für Sie tun?";
            return AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Willkommen, text, title, speech, DateTime.Now, false);
        }

      // ##############################################################################################################
      /// <summary>
      /// Regelt die Antwort beim Beenden des Alexa Skills
      /// </summary>
      /// <param name="request">Enthält die Anfrage vom Amazon Alexa Server vom Typ SitzungBeenden</param>
      /// <returns></returns>
      private SkillResponse SitzungBeendenVerwalter(SkillRequest request)
        {
            return AlexaAntwortHelfer.GibEinfacheAntwort(request, SkillTypen.Ended, AlexaAntwortHelfer.Ended, FehlerTypen.Ended.ToDescription(), null, DateTime.Now, true);
        }

      // ##############################################################################################################
      /// <summary>
      /// Verwaltet die touchinteraktion mit dem Alexa Skill
      /// </summary>
      /// <param name="request">Enthält die Anfrage vom Amazon Alexa Server vom Typ ElementKlick</param>
      /// <returns></returns>
      private SkillResponse ElementKlickVerwalter(SkillRequest request)
        {
            var kw = 0;
            var year = DateTime.Now.Year;
            var id = request.GetItemById();
            var tag = request.GetSessionDate();

            if (tag != null)
                kw = tag.GetWeekOfYear();
            else
                kw = DateTime.Now.GetWeekOfYear();

            var speisePlan = GetSpeisePlaene(kw, year).Result;
            return EssenDetailsResponseHelper.GetEssenDetailsResponse(request, speisePlan, -1, tag, id);
        }

      // ##############################################################################################################
      /// <summary>
      /// Gibt die richtige Antwort auf eine Anfrage vom Typ SpeiseplanKommando
      /// </summary>
      /// <param name="anfrage">Enthält die Anfrage vom Amazon Alexa Server vom Typ SpeiseplanKommando</param>
      /// <returns></returns>
      private SkillResponse SpeisePlanKommando(SkillRequest anfrage)
        {
            var tag = anfrage.GetDateTime(SlotValues.Tag.ToString());
            var kw = DateTime.Now.GetWeekOfYear();
            var year = DateTime.Now.Year;
            var numberKW = DateTime.Now.GetNumberOfWeeks();

            if (tag == null)
                tag = DateTime.Now;
            if (tag.Value.DayOfWeek < DateTime.Now.DayOfWeek)
                kw++;
            if (kw > numberKW)
            {
                kw = 1;
                year++;
            }

            var speisePlan = GetSpeisePlaene(kw, year).Result;
            if (speisePlan != null)
                return SpeisePlanAntwortHelfer.GetSpeisePlanResponse(anfrage, speisePlan, tag);
            else
                return AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Error, FehlerTypen.NoSpeisePlan.ToDescription(), "", null, tag.Value.Date, false);
        }

      // ##############################################################################################################
      /// <summary>
      /// 
      /// </summary>
      /// <param name="anfrage">Enthält die Anfrage vom Amazon Alexa Server</param>
      /// <returns></returns>
      private SkillResponse TagUndKategorieKommando(SkillRequest anfrage)
        {
            var intentRequest = (IntentRequest)anfrage.Request;

            if (intentRequest.DialogState.Equals("STARTED"))
            {
                return ResponseBuilder.DialogDelegate(anfrage.Session, intentRequest.Intent);
            }
            else if (!intentRequest.DialogState.Equals("COMPLETED"))
            {
                return ResponseBuilder.DialogDelegate(anfrage.Session);
            }
            else
            {
                var numberKW = DateTime.Now.GetNumberOfWeeks();
                var kw = DateTime.Now.GetWeekOfYear();
                var year = DateTime.Now.Year;
                var kategorie = anfrage.GetSlotValueInt(SlotValues.Kategorie.ToString(), defaultValue);
                var tag = anfrage.GetDateTime(SlotValues.Tag.ToString());

                if (tag == null)
                    tag = DateTime.Now;
                if (tag.Value.DayOfWeek < DateTime.Now.DayOfWeek)
                    kw++;
                if (kw > numberKW)
                {
                    kw = 1;
                    year++;
                }

                var speisePlan = GetSpeisePlaene(kw, year).Result;
                if (kategorie != defaultValue)
                    return EssenDetailsResponseHelper.GetEssenDetailsResponse(anfrage, speisePlan, kategorie, tag, 0);
                else
                    return AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, tag.Value.Date, false);
            }
        }

        // ##############################################################################################################
        private SkillResponse WocheNachKategorieKommando(SkillRequest anfrage)
        {
            var intentRequest = (IntentRequest)anfrage.Request;

            if (intentRequest.DialogState.Equals("STARTED"))
            {
                return ResponseBuilder.DialogDelegate(anfrage.Session, intentRequest.Intent);
            }
            else if (!intentRequest.DialogState.Equals("COMPLETED"))
            {
                return ResponseBuilder.DialogDelegate(anfrage.Session);
            }
            else
            {
                var numberKW = DateTime.Now.GetNumberOfWeeks();
                var kw = DateTime.Now.GetWeekOfYear();
                var year = DateTime.Now.Year;
                var kategorie = anfrage.GetSlotValueInt(SlotValues.Kategorie.ToString(), defaultValue);
                var nextWeek = anfrage.GetSlotValueInt(SlotValues.NextWeek.ToString(), defaultValue) == defaultValue;

                if (!nextWeek)
                    kw++;
                if (kw > numberKW)
                {
                    kw = 1;
                    year++;
                }

                var speisePlan = GetSpeisePlaene(kw, year).Result;
                if (kategorie != defaultValue)
                    return WocheNachKategorieAntwortHelfer.GetWocheNachKategorieResponse(anfrage,speisePlan, kategorie, kw);
                else
                    return AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, false);
            }
        }

      // ##############################################################################################################
      /// <summary>
      /// Gibt die richtige Antwort auf die Frage nach den preisen
      /// </summary>
      /// <param name="anfrage">Enthält die Anfrage vom Amazon Alexa Server vom Typ PreisIntent</param>
      /// <returns></returns>
      private SkillResponse PreisIntent(SkillRequest anfrage)
        {
            string speech = "Die Vorspeise kostet 2 Euro. Menü 1 kostet 7,90 Euro. Menü 2 kostet 6,90 Euro. Das vegetarische Menü kostet 4,90 Euro.";
            return AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Preis, speech, "Preise", speech, DateTime.Now, false);
        }

        // ##############################################################################################################
        /// <summary>
        /// Konvertiert die Daten der DB in verarbeitbare Elemente
        /// </summary>
        /// <param name="heutigeMenues"></param>
        /// <returns></returns>
        private SkillResponse FalschesKommando(SkillRequest anfrage)
        {
            string speech = "Ich konnte Sie leider nicht verstehen. Um den heutigen Speiseplan zu erfahren, sagen sie: Was gibt es heute zu essen?";
            return AlexaAntwortHelfer.GibEinfacheAntwort(anfrage, SkillTypen.Error, speech, "", speech, DateTime.Now, false);
        }

        // ##############################################################################################################
        private List<SpeisePlan> SpeisePlanConverter(List<SpeisePlanDB> heutigeMenues)
        {
            var result = new List<SpeisePlan>();

            foreach (SpeisePlanDB tmp in heutigeMenues)
            {
                foreach (Gericht gericht in tmp.Gerichte)
                {
                    var speise = new SpeisePlan();
                    speise.Beschreibung = gericht.Bezeichnung;
                    speise.Id = gericht.ID;
                    var values = Enum.GetValues(typeof(MenueKategorienDB));
                    foreach (MenueKategorienDB z in values)
                    {
                        if (gericht.Kategorie.Equals(z.ToDescription()))
                        {
                            speise.Kategorie = z.AsInt();
                        }
                    }

                    speise.Preis = Convert.ToDouble(gericht.Preis);
                    speise.Datum = tmp.Datum;
                    result.Add(speise);
                }
            }
            return result;
        }

        // ##############################################################################################################
        private void CreateErrorLog(Exception e)
        {
            var path = @"C:\Users\gew\Documents\GitHub\Schubs_IT_Alexa\ErrorLog.txt";

            using (var writer = new StreamWriter(path, true))
            {                
                writer.WriteLine("===========Start=============");
                writer.WriteLine("Error Type: " + e.GetType().FullName);
                writer.WriteLine("Error Message: " + e.Message);
                writer.WriteLine("Stack Trace: " + e.StackTrace);
                writer.WriteLine("============End==============");
                writer.WriteLine("\n");
            }
        }
    }
}
