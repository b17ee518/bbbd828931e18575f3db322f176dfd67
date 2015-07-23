using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace NicoPlayWPF.Models
{
    public enum PosGroupType
    {
        Normal,
        Top,
        Bottom,
        None,
    }
    public class PosGroupModel : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        public static PosGroupType GetPosGroupFromString(string str)
        {
            if (StringUtils.SameString(str, "ue"))
            {
                return PosGroupType.Top;
            }
            if (StringUtils.SameString(str, "shita") || StringUtils.SameString(str, "sita"))
            {
                return PosGroupType.Bottom;
            }
            if (StringUtils.SameString(str, "naka"))
            {
                return PosGroupType.Normal;
            }
            return PosGroupType.None;
        }

    }
}
