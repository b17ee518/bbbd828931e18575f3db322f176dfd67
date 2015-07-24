using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using NicoPlayWPF.Models;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NicoPlayWPF.ViewModels
{
    public class NicoLabel : KOutlinedTextBlock
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */
        
        Size _origSize = new Size(0, 0);
        double _origFontSize = 24.0;        
        PosGroupType _posGroup = PosGroupType.Normal;
        double _xSpeed = 1.0;
        double _scale = 1.0;
        bool _bOverflow = false;
        double _x = 0.0;
        double _y = 0.0;

        double _actualWidth = 0.0;
        double _actualHeight = 0.0;
        
        int _livedTime = 0;

        double _shadowDepth = 2.0;

        double _space = 4.0;
        double _strokeThickness = 0.75;

        public void Initialize()
        {
            this.IsHitTestVisible = false;
        }

        public void SetShadowDepth(double depth)
        {
            _shadowDepth = depth;
        }
        
        public void SetByNicoComment(NicoComment config)
        {
            this.Text = config.Text;

            this.Fill = new SolidColorBrush(config.TextColor);

            _posGroup = config.PosGroup;
            this.FontFamily = new FontFamily("MS PGothic");
            this.FontSize = config.FontSize;
            this.FontWeight = FontWeights.UltraBold;
            this.TextTrimming = System.Windows.TextTrimming.None;
            this.TextWrapping = System.Windows.TextWrapping.NoWrap;

//            this->adjustSize();
//            this->setAttribute(Qt::WA_TranslucentBackground);

            this.initReady();
        }
        
        private void initReady()
        {
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, ActualWidth, ActualHeight));

            _origSize = new Size(ActualWidth, ActualHeight);
            _origFontSize = this.FontSize;

            _actualWidth = ActualWidth;
            _actualHeight = ActualHeight;
        }

        public double getWidth()
        {
            return _actualWidth;
        }

        public double getHeight()
        {
            return _actualHeight;
        }
        
        public void setScale(double scale)
        {
            _scale = scale;

            double fontSize = _origFontSize*scale;
//            Size size = new Size(_origSize.Width*scale, _origSize.Height*scale);

//            this->resize(size);
            this.FontSize = fontSize;

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, ActualWidth, ActualHeight));

            _actualWidth = ActualWidth;
            _actualHeight = ActualHeight;

            if (_shadowDepth > 0)
            {
                DropShadowEffect effect = (DropShadowEffect)this.Effect;
                effect.ShadowDepth = _shadowDepth * scale;
                this.Effect = effect;
            }
        }

        public void applyNewScale(double scale, double relScale)
        {
	        setScale(scale);

	        setPosX(x()*relScale);
	        setPosY(y()*relScale);
        }
        public void Birth(double w, double h, double scale)
        {
            if (_shadowDepth > 0)
            {
                DropShadowEffect effect = new DropShadowEffect();
                effect.ShadowDepth = _shadowDepth * scale;
                effect.Direction = 330;
                effect.Color = Color.FromRgb(0, 0, 0);
                effect.Opacity = 1.0;
                effect.BlurRadius = 0;
                this.Effect = effect;

                this.StrokeThickness = 0.0;
            }
            else
            {
                this.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                this.StrokeThickness = _strokeThickness;
            }

            _xSpeed = (w + _origSize.Width*scale) / 4000.0f / scale;
            _livedTime = 0;

            setScale(scale);

            if (_posGroup == PosGroupType.Normal)
            {
                setPosX(w);
                setPosY(_space); 
            }
            else
            {
                setPosX(w / 2 - this.getWidth() / 2);
                if (_posGroup == PosGroupType.Top)
                {
                    setPosY(_space);
                }
                else
                {
                    setPosY(h - this.getHeight() - _space);
                }

            }
        }

        public bool IsDead()
        {
            return false;
        }

        public double x()
        {
            return this.Margin.Left;
        }
        public double y()
        {
            return this.Margin.Top;
        }
        public void setPosX(double x)
        {
            _x = x;
            this.Margin = new Thickness(_x, Margin.Top, Margin.Right, Margin.Bottom);
        }
        public void setPosY(double y)
        {
            _y = y;
            this.Margin = new Thickness(Margin.Left, _y, Margin.Right, Margin.Bottom);
        }

        public double getXSpeed()
        {
            if (_posGroup == PosGroupType.Normal)
            {
                return _xSpeed * _scale;
            }
            return 0;
        }

        public void DoMove(int msDiff)
        {
            if (_posGroup == PosGroupType.Normal)
            {
		        double newx = x() - getXSpeed()*msDiff;
                setPosX(newx);
            }
	        _livedTime += msDiff;
        }

        public bool isOverflow()
        {
            return _bOverflow;
        }
        
        public bool isDead()
        {
	        return _livedTime > 4000;
        }
        
        public bool moveDown(double bottom)
        {
            double height = this.getHeight() + _space;
            Random rand = new Random();
            if (_posGroup == PosGroupType.Bottom)
            {
                // move up
                if (y() - height < _space)
                {
                    _bOverflow = true;
                    setPosY(rand.Next((int)(bottom-height-_space*2))+_space);
                    return false;
                }
                setPosY(y()-height);
                return true;
            }
            if (y() + 2*height > bottom-_space)
            {
                _bOverflow = true;
                setPosY(rand.Next((int)(bottom-height-_space*2))+_space);
                return false;
            }
            setPosY(y()+height);
            return true;
        }

        public bool collideWith(NicoLabel other)
        {
            double thisHeight = this.getHeight() + _space;
            double otherHeight = other.getHeight() + _space;

            if (_bOverflow || other.isOverflow())
            {
                return false;
            }

            if (_posGroup != other._posGroup)
            {
                return false;
            }

            if (this.y()+thisHeight <= other.y())
            {
                return false;
            }
            if (this.y() >= other.y() + otherHeight)
            {
                return false;
            }

            if (_posGroup != PosGroupType.Normal)
            {
                return true;
            }

            if (other.x()+other.getWidth() > this.x())
            {
                return true;
            }
            if (-this.x()/(_xSpeed*_scale)*other.getXSpeed()+other.x()+other.getWidth() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
