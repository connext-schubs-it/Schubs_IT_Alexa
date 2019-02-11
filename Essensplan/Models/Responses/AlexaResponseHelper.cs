using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using Alexa.NET.Response.Directive.Templates.Types;
using AssistServer.Extension.NewFolder;
using Essensplan.Models;
using Essensplan.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssistServer.Models.Api.Alexa.Response
{
    public class AlexaResponseHelper
    {
        public const string Ended = "Danke und bis zum nächsten mal";

        protected static List<SkillTypContent> skillParameter = new List<SkillTypContent>
        {
             new SkillTypContent {
                Typ = SkillTypen.Heutiges,
                UrlCard = "https://vivmobil.connext.de/alexa/1200x800_3.png",
                UrlTemplate = "https://vivmobil.connext.de/alexa/1024x600_3.png",
                CardTitle = "Heutiger Speiseplan"
             },
             new SkillTypContent
             {
                 Typ = SkillTypen.SpeisePlan,
                 UrlCard = "https://vivmobil.connext.de/alexa/1200x800_3.png",
                 UrlTemplate = "https://vivmobil.connext.de/alexa/1024x600_3.png",
                 CardTitle = "Essensplan für den "
             },
             new SkillTypContent
             {
                 Typ = SkillTypen.EssensDetail,
                 UrlCard = "https://vivmobil.connext.de/alexa/1200x800_3.png",
                 UrlTemplate = "https://vivmobil.connext.de/alexa/1024x600_3.png",
             },
             new SkillTypContent
             {
                 Typ = SkillTypen.WocheNachKategorie,
                 UrlCard = "https://vivmobil.connext.de/alexa/1200x800_3.png",
                 UrlTemplate = "https://vivmobil.connext.de/alexa/1024x600_3.png",
                 CardTitle = "für KW"
             },
          };

        public static SkillResponse CreateSimpleResponse(SkillRequest request, SkillTypen typ, string text, string title, string speech, DateTime date, bool? shouldEndSession)
        {
            if (speech == null)
                speech = text;

            var response = new SkillResponse
            {
                Version = "1.0",
                SessionAttributes = CreateSessionAttributes(request, typ, -1, null, date),
                Response = new ResponseBody
                {
                    Card = new SimpleCard
                    {
                        Title = title,
                        Content = text
                    },
                    Reprompt = CreateReprompt(),
                    OutputSpeech = new SsmlOutputSpeech { Ssml = $"<speak>{speech}</speak>" },
                    ShouldEndSession = false,
                    //ShouldEndSession = shouldEndSession,
                }
            };            

            return response;
        }

        public static SkillResponse CreateListSkillResponse(SkillRequest request, SkillTypen typ, List<ListItem> listItems, IOutputSpeech speech, ICard card, string title, DateTime date, bool? shouldEndSession)
        {
            return new SkillResponse
            {
                Version = "1.0",
                Response = CreateListResponseBody(request, typ, listItems, speech, card, title, shouldEndSession),
                SessionAttributes = CreateSessionAttributes(request, typ, -1, listItems, date)
            };
        }

        public static SkillResponse CreateBodySkillResponse(SkillRequest request, SkillTypen typ, BodyTemplate2 template  , IOutputSpeech speech, ICard card, DateTime date, bool? shouldEndSession)
        {
            return new SkillResponse
            {
                Version = "1.0",
                Response = CreateBodyResponseBody(request, template , speech, card, shouldEndSession),
                SessionAttributes = CreateSessionAttributes(request, typ, -1, null, date)
            };
        }

        private static ResponseBody CreateListResponseBody(SkillRequest request, SkillTypen typ, List<ListItem> listItems, IOutputSpeech speech, ICard card, string title, bool? shouldEndSession)
        {
            var hasDisplay = request.HasDisplay();
            var body = new ResponseBody
            {
                OutputSpeech = speech,
                Card = card,
                Reprompt = CreateReprompt(),
                ShouldEndSession = shouldEndSession
            };

            if (hasDisplay)
                body.Directives = CreateListDirectives(typ, listItems, title);

            return body;
        }

        private static ResponseBody CreateBodyResponseBody(SkillRequest request, BodyTemplate2 template, IOutputSpeech speech, ICard card, bool? shouldEndSession)
        {
            var hasDisplay = request.HasDisplay();
            var body = new ResponseBody
            {
                OutputSpeech = speech,
                Card = card,
                Reprompt = CreateReprompt(),
                ShouldEndSession = shouldEndSession
            };

            if (hasDisplay)
                body.Directives = CreateBodyDirectives(template);

            return body;
        }

        private static ICard CreateLinkAccountCard()
        {
            return new LinkAccountCard();
        }

        private static IList<IDirective> CreateListDirectives(SkillTypen typ, List<ListItem> listItems, string title)
        {
            var directive = new DisplayRenderTemplateDirective();
            directive.Template = CreateListTemplate1(typ, listItems, title);
            var items = new List<IDirective>();
            items.Add(directive);

            return items;
        }

        private static IList<IDirective> CreateBodyDirectives(BodyTemplate2 template)
        {
            var directive = new DisplayRenderTemplateDirective();
            directive.Template = template;
            var items = new List<IDirective>();
            items.Add(directive);

            return items;
        }

        private static ListTemplate1 CreateListTemplate1(SkillTypen typ, List<ListItem> listItems, string title)
        {
            var url = skillParameter.FirstOrDefault(v => v.Typ == typ)?.UrlTemplate;

            if (listItems.Count == 0 || listItems == null)
            {
                var item = CreateNoContentList();
                listItems.Add(item);
            }

            return new ListTemplate1
            {
                Token = typ.ToString(),
                Title = title,
                BackButton = BackButtonVisibility.Visible,
                BackgroundImage = new TemplateImage
                {
                    ContentDescription = "_Icon",
                    Sources = new List<ImageSource> { new ImageSource { Url = url } }
                },
                Items = listItems
            };
        }

        private static ListItem CreateNoContentList()
        {
            return new ListItem
            {
                Token = "NoItems",
                Image = new TemplateImage
                {
                    ContentDescription = "NoIcon",
                    Sources = new List<ImageSource> { new ImageSource { Url = "" } }
                },
                Content = new TemplateContent
                {
                    Primary = new TemplateText
                    {
                        Text = TextStyle.SetFont5("Keine Einträge enthalten"),
                        Type = TextType.Rich
                    }
                }
            };
        }

        protected static Reprompt CreateReprompt()
        {
            return new Reprompt
            {
                OutputSpeech = new SsmlOutputSpeech
                {
                    Ssml = ""
                }
            };
        }

        public static BodyTemplate2 AddBodyContent(SkillTypen typ, SkillBodyContent content, string title)
        {
            var url = skillParameter.FirstOrDefault(v => v.Typ == typ)?.UrlTemplate;

            return new BodyTemplate2
            {
                Token = typ.ToString(),
                Title = title,
                BackButton = BackButtonVisibility.Visible,
                Image = new TemplateImage
                {
                    ContentDescription = "_Icon",
                    Sources = new List<ImageSource> { new ImageSource { Url = content.ImageUrl } }
                },
                BackgroundImage = new TemplateImage
                {
                    ContentDescription = "_Icon",
                    Sources = new List<ImageSource> { new ImageSource { Url = url } }
                },
                Content = new TemplateContent
                {
                    Primary = new TemplateText { Text = content.Primaer, Type = TextType.Rich },
                    Secondary = new TemplateText { Text = content.Sekundaer, Type = TextType.Rich },
                    Tertiary = new TemplateText { Text = content.Tertiaer, Type = TextType.Rich }
                }
            };
        }

        public static ListItem AddListItemWithImage(string pageToken, int id, string primaer, string sekundaer, string tertiaer, string image)
        {
            var listItem = new ListItem
            {
                Token = $"{pageToken}_{id}",
                Content = new TemplateContent
                {
                    Primary = new TemplateText
                    {
                        Text = primaer,
                        Type = TextType.Rich
                    },
                    Secondary = new TemplateText
                    {
                        Text = sekundaer,
                        Type = TextType.Rich
                    },
                    Tertiary = new TemplateText
                    {
                        Text = tertiaer,
                        Type = TextType.Rich
                    }
                },
                Image = new TemplateImage
                {
                    Sources = new List<ImageSource> { new ImageSource { Url = image } },
                    ContentDescription = "_Icon"
                }
            };

            return listItem;
        }

        private static Dictionary<string, object> CreateSessionAttributes(SkillRequest request, SkillTypen typ, int id, List<ListItem> listItems, DateTime date)
        {
            var listTokens = new List<string>();
            if (listItems != null)
            {
                foreach (var eintrag in listItems)
                    listTokens.Add(eintrag.Token);
            }

            var dictionary = new Dictionary<string, object>
            {
                { SessionAttributes.Id.ToString(), id },
                { SessionAttributes.CurrentListItems.ToString(), listTokens },
                { SessionAttributes.CurrentSkillTyp.ToString(), (int)typ },
                { SessionAttributes.CurrentTagesPlanDate.ToString(), date },
            };            

            return dictionary;
        }
    }
}

