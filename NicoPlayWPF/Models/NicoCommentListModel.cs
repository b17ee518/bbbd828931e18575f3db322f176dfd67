using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using NicoPlayWPF.ViewModels;
using System.Xml;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;

namespace NicoPlayWPF.Models
{
    public class NicoCommentListModel : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        public List<NicoComment> comments = new List<NicoComment>();
        Color invalidColor = Color.FromArgb(0, 0, 0, 0);

        bool _noShadow = false;
        
        public void SetNoShadowMode(bool bNoShadow)
        {
            _noShadow = bNoShadow;
        }

        public void clearComments()
        {
            comments.Clear();
        }

        public void ParseElement(XmlNode node)
        {
            while (true)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    break;
                }
                XmlElement elem = (XmlElement)node;
                if (elem.Name == "chat")
                {
                    string text = elem.InnerText;
                    string vposstr = elem.GetAttribute("vpos");
                    if (vposstr == null)
                    {
                        break;
                    }
                    string mailstr = elem.GetAttribute("mail");

                    NicoComment comment = new NicoComment();
                    comment.Text = text;
                    comment.VPos = Int64.Parse(vposstr);

                    comment.TextColor = Color.FromRgb(0xff, 0xff, 0xff);
                    comment.PosGroup = PosGroupType.Normal;
                    comment.FontSize = SizeModel.normalSize;

                    if (mailstr != null)
                    {
                        string[] splited = mailstr.Split();
                        foreach (string mail in splited)
                        {
                            if (mail == "184")
                            {
                                continue;
                            }

                            if (mail.StartsWith("#"))
                            {
                                comment.TextColor = (Color)ColorConverter.ConvertFromString(mail);
                            }
                            else
                            {
                                PosGroupType posType = PosGroupModel.GetPosGroupFromString(mail);
                                if (posType != PosGroupType.None)
                                {
                                    comment.PosGroup = posType;
                                    continue;
                                }
                                double fontSize = SizeModel.GetSizeTypeFromString(mail);
                                if (fontSize > 0)
                                {
                                    comment.FontSize = fontSize;
                                    continue;
                                }

                                
                                Color col = ColorTypeModel.GetColorFromString(mail);
                                if (col != invalidColor)
                                {
                                    comment.TextColor = col;
                                    continue;
                                }
                                 
                            }
                        }
                    }

                    comments.Add(comment);
                    break;
                }
                break;
            }
            if (node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    ParseElement(child);
                }
            }
        }

        public bool ReadFromXML(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            XmlDocument doc = new XmlDocument();
            try
            {
            	doc.Load(path);
            }
            catch (System.Exception ex)
            {
                return false;
            }

            XmlElement elem = doc.DocumentElement;

            ParseElement(elem);
           
            return true;
        }

        public void OnUpdate(Int64 lastMSec, Int64 curMSec, double scale, Panel container, NicoLabelListModel labels)
        {
            double w = container.ActualWidth;
            double h = container.ActualHeight;
            foreach (NicoComment comm in comments)
            {
		        Int64 vposMSec = comm.VPos * 10;
                if (vposMSec >= lastMSec && vposMSec < curMSec)
                {
			        NicoLabel label = new NicoLabel();
//                    label.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    // child of
                    container.Children.Add(label);

//			        label->setAttribute(Qt::WA_TransparentForMouseEvents);
                    label.SetByNicoComment(comm);
                    if (_noShadow)
                    {
                        label.SetShadowDepth(0);
                    }
                    label.Birth(w, h, scale);


			        label.DoMove((int)(curMSec - vposMSec));

                    labels.checkCollision(label, scale, container);

                    labels.Add(label);
                }
            }
        }

    }
}
