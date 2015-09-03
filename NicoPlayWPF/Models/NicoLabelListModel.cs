using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using NicoPlayWPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace NicoPlayWPF.Models
{
    public class NicoLabelListModel : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        private List<NicoLabel> labels = new List<NicoLabel>();
        bool _commentOff = false;

        public void OnUpdate(int msDiff)
        {
            foreach (NicoLabel label in labels)
            {
                if (label.isDead())
                {
                    Panel parent = (Panel)VisualTreeHelper.GetParent(label);
                    parent.Children.Remove(label);
                }
            }
            labels.RemoveAll(item => item.isDead());

            foreach (NicoLabel label in labels)
            {
                label.Visibility = _commentOff ? Visibility.Hidden : Visibility.Visible;
                label.DoMove(msDiff);
            }
        }

        public void SetCommentOff(bool off)
        {
            _commentOff = off;
        }

        public void Add(NicoLabel label)
        {
            labels.Add(label);
        }
        
        public void RemoveAll()
        {
            foreach (NicoLabel label in labels)
            {
                Panel parent = (Panel)VisualTreeHelper.GetParent(label);
                parent.Children.Remove(label);
            }
            labels.Clear();
        }
        
        public void applyNewScale(double scale, double relscale, double w)
        {
            foreach (NicoLabel label in labels)
            {
                label.applyNewScale(scale, relscale, w);
            }
        }

        public void checkCollision(NicoLabel label, double scale, Panel container)
        {
            foreach(NicoLabel item in labels)
            {
                if (label == item)
                {
                    continue;
                }
                if (label.collideWith(item))
                {
                    if (label.moveDown(container.ActualHeight))
                    {
                        // moved done
                        checkCollision(label, scale, container);
                        return;
                    }
                    else
                    {
                        // overflow
                        return;
                    }
                }
            }
        }
    }
}
