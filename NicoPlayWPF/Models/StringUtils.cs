using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace NicoPlayWPF.Models
{
    public class StringUtils : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        public static bool SameString(string a, string b)
        {
            return String.Compare(a, b, true) == 0;
        }
    }
}
