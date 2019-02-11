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
        private readonly string api = "https://cx-hotel.connext.de:8080/api/speiseplan/kw/";
        private readonly int defaultValue = -1;

        private async Task<List<SpeisePlan>> GetSpeisePlaene(int kw, int year)
        {
            var client = new HttpClient();
            var speisePlaene = new List<SpeisePlan>();
            //var path = $"{api}{kw}/{year}";
            var path = $"{api}2/2019";
            var response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                speisePlaene = JsonConvert.DeserializeObject<List<SpeisePlan>>(await response.Content.ReadAsStringAsync());
                speisePlaene = speisePlaene.FindAll(s => s.Kategorie != (int)MenueKategorien.Salat_1);
                speisePlaene = speisePlaene.FindAll(s => s.Kategorie != (int)MenueKategorien.Salat_2);
            }

            return speisePlaene;
        }

        [HttpPost]
        public dynamic Alexa([FromBody]SkillRequest request)
        {
            try
            {
                if (request.Context.System.ApiAccessToken == null)
                    return new BadRequestResult();

                var response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, false);
                var requestType = request.GetRequestType();

                if (requestType == typeof(LaunchRequest))
                    response = LaunchRequestHandler(request);

                else if (requestType == typeof(IntentRequest))
                    response = IntentRequestHandler(request);

                else if (requestType == typeof(SessionEndedRequest))
                    response = SessionEndedRequestHandler(request);

                else if (requestType == typeof(DisplayElementSelectedRequest))
                    response = SelectedRequestHandler(request);

                return response;
            }
            catch (Exception e)
            {
                CreateErrorLog(e);
                return null;
            }
        }        

        private SkillResponse IntentRequestHandler(SkillRequest request)
        {
            var response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, false);
            var intentRequest = (IntentRequest)request.Request;

            if (intentRequest.Intent.Name.Equals("SpeisePlanIntent"))
                response = SpeisePlanIntent(request);
            else if (intentRequest.Intent.Name.Equals("EssenDetailsIntent"))
                response = EssenDetailsIntent(request);
            else if (intentRequest.Intent.Name.Equals("WocheNachKategorieIntent"))
                response = WocheNachKategorieIntent(request);
            else if (intentRequest.Intent.Name.Equals("AMAZON.StopIntent") || intentRequest.Intent.Name.Equals("AMAZON.CancelIntent"))
                response = SessionEndedRequestHandler(request);

            return response;
        }

        private SkillResponse LaunchRequestHandler(SkillRequest request)
        {
            return SpeisePlanIntent(request);
        }

        private SkillResponse SessionEndedRequestHandler(SkillRequest request)
        {
            return AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Ended, AlexaResponseHelper.Ended, FehlerTypen.Ended.ToDescription(), null, DateTime.Now, true);
        }

        private SkillResponse SelectedRequestHandler(SkillRequest request)
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

        private SkillResponse SpeisePlanIntent(SkillRequest request)
        {            
            var tag = request.GetDateTime(SlotValues.Tag.ToString());
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
                return SpeisePlanResponseHelper.GetSpeisePlanResponse(request, speisePlan, tag);
            else
                return AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.NoSpeisePlan.ToDescription(), "", null, tag.Value.Date, false);
        }

        private SkillResponse EssenDetailsIntent(SkillRequest request)
        {
            var intentRequest = (IntentRequest)request.Request;

            if (intentRequest.DialogState.Equals("STARTED"))
            {
                return ResponseBuilder.DialogDelegate(request.Session, intentRequest.Intent);
            }
            else if (!intentRequest.DialogState.Equals("COMPLETED"))
            {
                return ResponseBuilder.DialogDelegate(request.Session);
            }
            else
            {
                var numberKW = DateTime.Now.GetNumberOfWeeks();
                var kw = DateTime.Now.GetWeekOfYear();
                var year = DateTime.Now.Year;
                var kategorie = request.GetSlotValueInt(SlotValues.Kategorie.ToString(), defaultValue);
                var tag = request.GetDateTime(SlotValues.Tag.ToString());

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
                    return EssenDetailsResponseHelper.GetEssenDetailsResponse(request, speisePlan, kategorie, tag, -1);
                else
                    return AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, tag.Value.Date, false);
            }
        }

        private SkillResponse WocheNachKategorieIntent(SkillRequest request)
        {
            var intentRequest = (IntentRequest)request.Request;

            if (intentRequest.DialogState.Equals("STARTED"))
            {
                return ResponseBuilder.DialogDelegate(request.Session, intentRequest.Intent);
            }
            else if (!intentRequest.DialogState.Equals("COMPLETED"))
            {
                return ResponseBuilder.DialogDelegate(request.Session);
            }
            else
            {
                var numberKW = DateTime.Now.GetNumberOfWeeks();
                var kw = DateTime.Now.GetWeekOfYear();
                var year = DateTime.Now.Year;
                var kategorie = request.GetSlotValueInt(SlotValues.Kategorie.ToString(), defaultValue);
                var nextWeek = request.GetSlotValueInt(SlotValues.NextWeek.ToString(), defaultValue) == defaultValue;

                if (!nextWeek)
                    kw++;
                if (kw > numberKW)
                {
                    kw = 1;
                    year++;
                }

                var speisePlan = GetSpeisePlaene(kw, year).Result;
                if (kategorie != defaultValue)
                    return WocheNachKategorieResponseHelper.GetWocheNachKategorieResponse(request,speisePlan, kategorie, kw);
                else
                    return AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, false);
            }
        }

        //##############################################################################################################
        private void CreateErrorLog(Exception e)
        {
            var path = @"C:\Users\sch\Desktop\Alexa\ErrorLog.txt";

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
