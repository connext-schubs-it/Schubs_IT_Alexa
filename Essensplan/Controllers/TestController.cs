using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AssistServer.Extension;
using AssistServer.Extension.NewFolder;
using AssistServer.Models.Api.Alexa.Response;
using Essensplan.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Essensplan.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpPost]
        public dynamic Alexa([FromBody]SkillRequest request)
        {
            if (request.Context.System.ApiAccessToken == null)
                return new BadRequestResult();
            
            var response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, null);
            var requestType = request.GetRequestType();

            if (requestType == typeof(LaunchRequest))
                response = LaunchRequestHandler(request);

            else if (requestType == typeof(IntentRequest))
                response = IntentRequestHandler(request);

            else if (requestType == typeof(SessionEndedRequest))
                response = SessionEndedRequestHandler(request);

            return response;
        }

        private SkillResponse LaunchRequestHandler(SkillRequest request)
        {
            return AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Ended, "Hi. Mit diesem Skill kannst du zwei Farben mischen.", "", null, DateTime.Now, null);
        }

        private SkillResponse IntentRequestHandler(SkillRequest request)
        {
            var response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, null);
            var intentRequest = (IntentRequest)request.Request;

            if (intentRequest.Intent.Name.Equals("FarbeMischenIntent"))
                response = FarbeMischenIntent(request);

            return response;
        }

        private SkillResponse SessionEndedRequestHandler(SkillRequest request)
        {
            return AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Ended, "Alles klar, machs gut.", "", null, DateTime.Now, true);
        }

        private SkillResponse FarbeMischenIntent(SkillRequest request)
        {
            var response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, FehlerTypen.FehlerAnfrage.ToDescription(), "", null, DateTime.Now, null);
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
                var text = "Daraus ergibt sich";
                var farbe1 = request.GetSlotValueInt("Farbe_I", -1);
                var farbe2 = request.GetSlotValueInt("Farbe_II", -1);

                if (farbe1 == 1 && farbe2 == 2 || farbe1 == 2 && farbe2 == 1)
                {
                    response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, $"{text} lila.", "", null, DateTime.Now, null);
                }
                else if (farbe1 == 1 && farbe2 == 3 || farbe1 == 3 && farbe2 == 1)
                {
                    response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, $"{text} orange.", "", null, DateTime.Now, null);
                }
                else if (farbe1 == 3 && farbe2 == 2 || farbe1 == 2 && farbe2 == 3)
                {
                    response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, $"{text} grün.", "", null, DateTime.Now, null);
                }
                else if (farbe1 == farbe2)
                {
                    response = AlexaResponseHelper.CreateSimpleResponse(request, SkillTypen.Error, "<s>Hey</s>, die gleiche Farbe zu mischen ist witzlos.", "", null, DateTime.Now, null);
                }

                return response;
            }
        }
    }
}
