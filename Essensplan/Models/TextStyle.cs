namespace AssistServer.Models.Api.Alexa.Response
{
    // Der ausgegebene Text bekommt die entsprechende Textgröße/-stil
    public static class TextStyle
    {
        public static string SetBold(string text)
        {
            return $"<b>{text}</b>";
        }

        public static string SetFont5(string text)
        {
            return $"<font size='5'><b>{text}</b></font>";
        }

        public static string SetFont4(string text)
        {
            return $"<font size='4'><b>{text}</b></font>";
        }

        public static string SetFont3(string text)
        {
            return $"<font size='3'>{text}</font>";
        }

        public static string SetFont2(string text)
        {
            return $"<font size='2'>{text}</font>";
        }
    }
}
