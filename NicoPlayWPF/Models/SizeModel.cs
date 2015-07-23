using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace NicoPlayWPF.Models
{
    public class SizeModel : NotificationObject
    {
        public static double normalSize = 24.0;
        public static double smallSize = 15.0;
        public static double bigSize = 39.0;
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        public static double GetSizeTypeFromString(string str)
        {
            if (StringUtils.SameString(str, "big"))
            {
                return bigSize;
            }
            if (StringUtils.SameString(str, "small"))
            {
                return smallSize;
            }
            if (StringUtils.SameString(str, "medium"))
            {
                return normalSize;
            }
            return 0.0;
        }
    }
}
