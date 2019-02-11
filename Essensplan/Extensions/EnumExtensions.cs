using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace AssistServer.Extension
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gibt eine String-Repräsentation eines Enum-Werts zurück.
        /// Wenn er mit einem OrderAttribute dekoriert ist,
        /// entspricht der String dem hinterlegten Wert, sonst wird die default ToString()-Methode verwendet.
        /// </summary>
        /// <param name="enumInstance"></param>
        /// <returns>String-Repräsentation eines Enum-Werts</returns>
        public static string ToDescription(this Enum enumInstance)
        {
            return enumInstance.ToDescription<DescriptionAttribute>();
        }

        /// <summary>
        /// Gibt eine String-Repräsentation eines Enum-Werts zurück.
        /// Wenn er mit einem OrderAttribute dekoriert ist,
        /// entspricht der String dem hinterlegten Wert, sonst wird die default ToString()-Methode verwendet.
        /// </summary>
        /// <param name="enumInstance"></param>
        /// <returns>String-Repräsentation eines Enum-Werts</returns>
        public static string ToDescription<T>(this Enum enumInstance)
           where T : DescriptionAttribute
        {
            var typeInfo = enumInstance
               .GetType()
               .GetTypeInfo();

            var field = typeInfo.GetDeclaredField(enumInstance.ToString());

            var attribute = field != null
               ? field.GetCustomAttribute<T>()
               : null;

            return attribute != null
               ? attribute.Description
               : enumInstance.ToString();
        }

        /// <summary>
        /// Abkürzung um einen Enum-Wert in die korrespondierende Zahl zu casten
        /// </summary>
        /// <param name="enumInstance"></param>
        /// <returns></returns>
        public static int AsInt(this Enum enumInstance)
        {
            return Convert.ToInt32(enumInstance);
        }

        public static string GetEnumDisplayName(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>()
                           .Name;
        }
    }
}