using System;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Essensplan.Models;
using Essensplan.Models.Responses;

namespace AssistServer.Extension.NewFolder
{
    public static class SkillRequestExtensions
    {
        /// <summary>
        /// Ermittelt aus dem Slot die angegebene Nummer
        /// </summary>
        /// <param name="request, slotName, defaultValue"></param>
        public static int GetSlotValueInt(this SkillRequest request, string slotName, int defaultValue)
        {
            IntentRequest intentRequest = request.Request as IntentRequest;
            Slot attributes = null;
            String id = null;
            if (intentRequest != null && intentRequest.Intent.Slots != null && intentRequest.Intent.Slots.TryGetValue(slotName, out attributes))
            {
                if (attributes != null)
                {
                    if (attributes.Resolution != null)
                        id = attributes.Resolution.Authorities[0].Values[0].Value.Id;
                    else
                        id = attributes.Value;
                }
            }

            try
            {
                if (!String.IsNullOrEmpty(id))
                    return Convert.ToInt32(id);
            }
            catch (Exception)
            {
                return defaultValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Ermittelt aus dem Slot die angegebene Zeit
        /// </summary>
        /// <param name="request, slotName"></param>
        public static DateTime? GetDateTime(this SkillRequest request, string slotName)
        {
            IntentRequest intentRequest = request.Request as IntentRequest;
            Slot attributes = null;
            string date = null;
            if (intentRequest != null && intentRequest.Intent.Slots != null && intentRequest.Intent.Slots.TryGetValue(slotName, out attributes))
            {
                if (attributes != null)
                {
                    if (attributes.Resolution != null)
                        date = attributes.Resolution.Authorities[0].Values[0].Value.Id;
                    else
                        date = attributes.Value;
                }
            }

            if (!String.IsNullOrEmpty(date))
            {
                if (date.Contains("-"))
                {
                    var dateArray = date.Split('-');

                    var year = Convert.ToInt16(dateArray[0]);
                    var month = Convert.ToInt16(dateArray[1]);
                    var day = Convert.ToInt16(dateArray[2]);
                    return new DateTime(year, month, day);
                }
                else
                {
                    var zeitArray = date.Split(':');
                    var hour = Convert.ToInt32(zeitArray[0]);
                    var minute = Convert.ToInt32(zeitArray[1]);
                    var now = DateTime.Now;
                    return new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Entnimmt aus dem angetippten Element die Id 
        /// </summary>
        /// <param name="request"></param>
        public static int GetItemById(this SkillRequest request)
        {
            var id = 0;
            var displayRequest = (DisplayElementSelectedRequest)request.Request;
            string[] list = displayRequest.Token.Split('_');
            if (list.Length == 2)
                id = Convert.ToInt32(list[1]);
            return id;
        }

        /// <summary>
        /// Prüft ob das verwendete Gerät ein Display enthält
        /// </summary>
        /// <param name="request"></param>
        public static bool HasDisplay(this SkillRequest request)
        {
            return request.Context.System.Device.SupportedInterfaces.Count > 0;
        }

        /// <summary>
        /// Ermittelt aus den SessionAttributes den aktuellen Skilltypen        
        /// </summary>
        /// <param name="request"></param>
        public static SkillTypen GetSkillTyp(this SkillRequest request)
        {
            return (SkillTypen)Convert.ToInt32(request.Session.Attributes[SessionAttributes.CurrentSkillTyp.ToString()]);
        }

        /// <summary>
        /// Ermittelt aus den SessionAttributes das Datum der aktuellen Ansicht        
        /// </summary>
        /// <param name="request"></param>
        public static DateTime GetSessionDate(this SkillRequest request)
        {
            return (DateTime)request.Session.Attributes[SessionAttributes.CurrentTagesPlanDate.ToString()];
        }
    }
}