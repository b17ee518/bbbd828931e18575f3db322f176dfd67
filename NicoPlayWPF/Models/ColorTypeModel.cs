using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using System.Windows.Media;

namespace NicoPlayWPF.Models
{
    public class ColorTypeModel : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        public static Dictionary<string, string> s_colorMap;

        private static void InitColorMap()
        {
            if (s_colorMap == null)
            {
                s_colorMap  = new Dictionary<string, string>()
                {
                    {"white", "#FFFFFF"},
                    {"red", "#FF0000"},
                    {"pink"	 , "#FF8080"},
                    {"orange" , "#FFC000"},
                    {"yellow" , "#FFFF00"},
                    {"green"  , "#00FF00"},
                    {"cyan"	 , "#00FFFF"},
                    {"blue"	 , "#0000FF"},
                    {"purple" , "#C000FF"},
                    {"black"  , "#000000"},
            
                    {"white2"				 , "#CCCC99"},
                    {"niconicowhite"  , "#CCCC99"},
                    {"red2"					 , "#CC0033"},
                    {"truered" 			 , "#CC0033"},
                    {"pink2" 				 , "#FF33CC"},
                    {"orange2" 			 , "#FF6600"},
                    {"passionorange"  , "#FF6600"},
                    {"yellow2" 			 , "#999900"},
                    {"madyellow" 		 , "#999900"},
                    {"green2"				 , "#00CC66"},
                    {"elementalgreen" , "#00CC66"},
                    {"cyan2" 				 , "#00CCCC"},
                    {"blue2" 				 , "#3399FF"},
                    {"marineblue"		 , "#3399FF"},
                    {"purple2" 			 , "#6633CC"},
                    {"nobleviolet" 	 , "#6633CC"},
                    {"black2"				 , "#666666"},
                };        
            }
        }
        public static Color GetColorFromString(string colName)
        {
            InitColorMap();
            string colStr;
            if (s_colorMap.TryGetValue(colName, out colStr))
            {
                return (Color)ColorConverter.ConvertFromString(colStr);
            }
            return Color.FromArgb(0, 0, 0, 0);
        }
    }
}
