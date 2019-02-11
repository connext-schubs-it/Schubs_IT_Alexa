using Essensplan.Models.Responses;

namespace AssistServer.Models.Api.Alexa.Response
{
    public class SkillTypContent
    {
        public SkillTypen Typ { get; set; }
        public string UrlCard { get; set; }
        public string UrlTemplate { get; set; }
        public string CardTitle { get; set; }
    }
}
