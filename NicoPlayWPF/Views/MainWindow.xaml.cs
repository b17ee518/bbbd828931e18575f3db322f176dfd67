using NicoPlayWPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NicoPlayWPF.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        bool _useSoftwareOnly = false;

        DispatcherTimer _timer = new DispatcherTimer();
        Int64 _duration = 0;
        bool _isPlaying = false;

        bool _ticking = false;
        double _scale = 1.0;

        Size _videoSize = new Size(512, 384);
        double _adjustedHeight = 384.0;
        
        const int timeTickInterval = 1;

        string _videoName = "";

        Int64 _lastMSec = 0;

        NicoLabelListModel _labels = new NicoLabelListModel();
        NicoCommentListModel _comments = new NicoCommentListModel();

        public static RoutedCommand s_playStopCommand = new RoutedCommand();

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (_useSoftwareOnly)
            {
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                HwndTarget hwndTarget = hwndSource.CompositionTarget;
                hwndTarget.RenderMode = RenderMode.SoftwareOnly;
            }
            base.OnSourceInitialized(e);
        }

        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 8;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth - 8;
            this.StateChanged += MainWindow_StateChanged;

            s_playStopCommand.InputGestures.Add(new KeyGesture(Key.Space));
            this.CommandBindings.Add(new CommandBinding(s_playStopCommand, PlayStopCommandHandler));

            this.Closing += MainWindow_Closing;
            LoadWindowPos();

            me.MediaOpened += Me_MediaOpened;
            me.MediaEnded += Me_MediaEnded;
            me.SizeChanged += Me_SizeChanged;

            this.SizeChanged += MainWindow_SizeChanged;

            me.MouseLeftButtonDown += Container_MouseLeftButtonDown;

            ResetPlayButton();

            _timer.Interval = TimeSpan.FromMilliseconds(timeTickInterval);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            OpenVideo("e:/NicoPlay/testVideo/元プロゲーマーが塗りつくスプラトゥーン！Sp：11【実況】(sm26704977).mp4");

            volSlider.ValueChanged += VolSlider_ValueChanged;
            positionSlider.ValueChanged += PositionSlider_ValueChanged;

            LoadSettings();

        }

        void PlayStopCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            TogglePlayPause();
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
            }
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            me.MaxHeight = this.ActualHeight - TitleFrame.ActualHeight - BottomBar.ActualHeight;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            SaveWindowPos();
        }

        void LoadSettings()
        {
            double vol = Properties.Settings.Default.Volume;
            volSlider.Value = vol;
        }

        void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        void LoadWindowPos()
        {
            this.Left = Properties.Settings.Default.WindowLeft;
            this.Top = Properties.Settings.Default.WindowTop;
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;
            if (Properties.Settings.Default.WindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        void SaveWindowPos()
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.WindowTop = RestoreBounds.Top;
                Properties.Settings.Default.WindowLeft = RestoreBounds.Left;
                Properties.Settings.Default.WindowHeight = RestoreBounds.Height;
                Properties.Settings.Default.WindowWidth = RestoreBounds.Width;
                Properties.Settings.Default.WindowMaximized = true;
            }
            else
            {
                Properties.Settings.Default.WindowTop = this.Top;
                Properties.Settings.Default.WindowLeft = this.Left;
                Properties.Settings.Default.WindowHeight = this.Height;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowMaximized = false;
            }

            Properties.Settings.Default.Save();
        }

        void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TogglePlayPause();
            if (e.ClickCount == 2)
            {
                ToggleMaximumState();
            }
        }

        void ToggleMaximumState()
        {
            if (WindowState == System.Windows.WindowState.Normal)
            {
                WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                WindowState = System.Windows.WindowState.Normal;
            }
        }

        void TogglePlayPause()
        {
            if (!IsMediaLoaded())
            {
                return;
            }
            if (IsPlaying())
            {
                Pause();
                playPauseButton.IsChecked = false;
            }
            else
            {
                Play();
                playPauseButton.IsChecked = true;
            }
        }

        void Me_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            Size newSize = new Size(me.ActualWidth, me.ActualHeight);

            container.Width = newSize.Width;
            container.Height = newSize.Height;

	        double wScale = newSize.Width /_videoSize.Width;
	        double hScale = newSize.Height / _videoSize.Height;
	
	        double oldScale = _scale;
	        _scale = wScale < hScale ? wScale : hScale;
	        double relScale = _scale / oldScale;

            setVideoItemSize(newSize);

            _labels.applyNewScale(_scale, relScale, container.Width);
        }

        void setVideoItemSize(Size size)
        {
        }


        void Me_MediaEnded(object sender, RoutedEventArgs e)
        {
            Stop();
            DeleteVideoAndXMLs();
        }

        void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_ticking)
            {
                if (!IsPlaying())
                {
                    me.Play();
                    me.Pause();
                }
                me.Position = TimeSpan.FromMilliseconds(positionSlider.Value);
                _labels.RemoveAll();
                if (!IsPlaying())
                {
                    me.Play();
                    me.Pause();
                }
            }
            SetTimeText();
        }

        void ResetTimeText()
        {
            timeText.Text = "--:--/--:--";
        }

        void SetTimeText()
        {
            if (!IsMediaLoaded())
            {
                ResetTimeText();
                return;
            }

            int curMin = (int)me.Position.TotalMinutes;
            int curSec= me.Position.Seconds;
            int totalMin = (int)me.NaturalDuration.TimeSpan.TotalMinutes;
            int totalSec = me.NaturalDuration.TimeSpan.Seconds;
            string timeStr = string.Format("{0:D2}:{1:D2}/{2:D2}:{3:D2}", curMin, curSec, totalMin, totalSec);

            timeText.Text = timeStr;
        }

        void VolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            me.Volume = volSlider.Value / 100.0f;
            Properties.Settings.Default.Volume = volSlider.Value;
        }

        void Me_MediaOpened(object sender, RoutedEventArgs e)
        {
            _duration = (Int64)me.NaturalDuration.TimeSpan.TotalMilliseconds;

            _videoSize.Width = me.NaturalVideoWidth;
            _videoSize.Height = me.NaturalVideoHeight;

            if (_videoSize.Height > _adjustedHeight)
            {
                _videoSize.Width *= _adjustedHeight / _videoSize.Height;
                _videoSize.Height = _adjustedHeight;
            }

            positionSlider.Maximum = _duration;
            SyncPositionSlider();
            SetTimeText();
            _lastMSec = 0;

            this.Title = _videoName;
        }

        void SyncPositionSlider()
        {
            positionSlider.Value = me.Position.TotalMilliseconds;
        }

        private string _videoPath = null;
        private string _xmlPath = null;
        private string _pmxmlPath = null;

        void OpenVideo(String path)
        {
            Stop();
            _videoPath = path;
            me.Source = new Uri(path);

            _videoName = System.IO.Path.GetFileNameWithoutExtension(path);
            _xmlPath = System.IO.Path.GetDirectoryName(path);
            _xmlPath += "/";
            _xmlPath += _videoName;
            _pmxmlPath = _xmlPath;
            _pmxmlPath += ".postmaster.xml";
            _xmlPath += ".xml";
            _comments.clearComments();
            _comments.ReadFromXML(_xmlPath);
            _comments.ReadFromXML(_pmxmlPath);
            me.Play();
            me.Pause();

        }


        bool IsMediaLoaded()
        {
            return _duration > 0;
        }

        bool IsPlaying()
        {
            return _isPlaying;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (IsPlaying())
            {
                _ticking = true;
                SyncPositionSlider();

                Int64 curMSec = (Int64)me.Position.TotalMilliseconds;
                if (_lastMSec < curMSec - 1000 || _lastMSec > curMSec)
                {
                    _lastMSec = curMSec - 1000;
                }
                if (_lastMSec < 0)
                {
                    _lastMSec = 0;
                }

                //
                int msDiff = (int)(curMSec - _lastMSec);
                _comments.OnUpdate(_lastMSec, curMSec, _scale, container, _labels);
                _labels.OnUpdate(msDiff);


                _lastMSec = curMSec;

                _ticking = false;
            }
        }


        void Play()
        {
            if (!IsMediaLoaded())
            {
                ResetPlayButton();
                return;
            }
            me.Play();
            playPauseButton.IsChecked = true;
            playPauseButton.Content = "||";
            _isPlaying = true;
        }

        void Pause()
        {
            if (!IsMediaLoaded())
            {
                return;
            }
            me.Pause();
            ResetPlayButton();
            _isPlaying = false;
        }

        void ResetPlayButton()
        {
            playPauseButton.IsChecked = false;
            playPauseButton.Content = "▶";
        }

        void Stop()
        {
            if (!IsMediaLoaded())
            {
                return;
            }
            me.Stop();
            ResetPlayButton();
            me.Play();
            me.Pause();
            _isPlaying = false;
            positionSlider.Value = 0;
            _lastMSec = 0;
            _labels.RemoveAll();
        }







        private void PlayPauseButton_Checked(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void PlayPauseButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                OpenVideo(files[0]);
            }
        }

        private void CommentOffButton_Checked(object sender, RoutedEventArgs e)
        {
            commentOffButton.Content = "Off";
            _labels.SetCommentOff(true);
        }
        private void CommentOffButton_Unchecked(object sender, RoutedEventArgs e)
        {
            commentOffButton.Content = "On";
            _labels.SetCommentOff(false);
        }

        private void ShadowOffButton_Checked(object sender, RoutedEventArgs e)
        {
            shadowOffButton.Content = "S";
            _comments.SetNoShadowMode(true);
        }
        private void ShadowOffButton_Unchecked(object sender, RoutedEventArgs e)
        {
            shadowOffButton.Content = "SS";
            _comments.SetNoShadowMode(false);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        public void DeleteVideoAndXMLs()
        {
            if (_videoPath == null)
            {
                return;
            }

            string messageBoxText = _videoPath;
            string caption = "Delete File";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(this, messageBoxText, caption, button, icon, MessageBoxResult.No);

            // Process message box results
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        if (_videoPath != null)
                        {
                            File.Delete(_videoPath);
                        }
                        if (_xmlPath != null)
                        {
                            File.Delete(_xmlPath);
                        }
                        if (_pmxmlPath != null)
                        {
                            File.Delete(_pmxmlPath);
                        }
                    }
                    break;
                case MessageBoxResult.No:
                    break;
            }


        }
        
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteVideoAndXMLs();
        }


    }
}
