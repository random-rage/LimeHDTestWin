using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace LimeHDTestWin
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Channel _currentChannel = null;
        int _autoHideTimeout = 0;
        DateTime _autoHideTimeOrigin;
        DispatcherTimer _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };

        /// <summary>
        /// Таймаут скрытия интерфейса в секундах
        /// </summary>
        public int AutoHideTimeout
        {
            get => _autoHideTimeout;
            set
            {
                if (value > 0)
                {
                    _autoHideTimeout = value;
                    _autoHideTimeOrigin = DateTime.Now;
                    player.PointerMoved += Player_PointerMoved;
                    player.Tapped += Player_Tapped; ;
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                }
                else
                {
                    _autoHideTimeout = 0;
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                    player.PointerMoved -= Player_PointerMoved;
                    player.Tapped -= Player_Tapped;
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Player_Tapped(object sender, TappedRoutedEventArgs e)
        {
            splitView.IsPaneOpen = false;

            if (playerControls.Visibility == Visibility.Collapsed)
            {
                playerControls.Visibility = Visibility.Visible;
                _autoHideTimeOrigin = DateTime.Now;
                _timer.Start();
            }
            else
            {
                playerControls.Visibility = Visibility.Collapsed;
                _timer.Stop();
            }
        }

        private void Player_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            _autoHideTimeOrigin = DateTime.Now;
        }

        private void Timer_Tick(object sender, object e)
        {
            if ((DateTime.Now - _autoHideTimeOrigin).TotalSeconds < _autoHideTimeout)
                return;

            playerControls.Visibility = Visibility.Collapsed;
            _timer.Stop();
        }

        private async void OpenPlaylist(StorageFile file)
        {
            List<Channel> channels;

            using (var fileReader = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                var jsonSerializer = new JsonSerializer();
                channels = jsonSerializer.Deserialize(fileReader, typeof(List<Channel>)) as List<Channel>;
            }

            if (channels == null)
            {
                App.Message("Error", "Can't load playlist");
                return;
            }

            nav.MenuItems.Clear();

            foreach (var chan in channels)
                nav.MenuItems.Add(chan);

            nav.IsPaneOpen = true;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AutoHideTimeout = 3;
            OpenPlaylist(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/playlist.json")));

            Task.Run(async () =>
            {
                try
                {
                    var response = await App.HttpClient.GetAsync(new Uri("http://info.limehd.tv/tech.php"));
                    var content = await response.Content.ReadAsStringAsync();
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => overlay.Children.Clear());

                    if (response.IsSuccessStatusCode)
                    {
                        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            foreach (var item in json)
                                overlay.Children.Add(new TextBlock() { Text = $"{{{item.Key}: {item.Value}}}" });
                        });
                    }
                    else
                        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            overlay.Children.Add(new TextBlock() { Text = content }));
                }
                catch (Exception ex)
                {
                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
                    {
                        overlay.Children.Clear();
                        overlay.Children.Add(new TextBlock() { Text = ex.Message });
                    });
                }
            });
        }

        private async void openBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var picker = new FileOpenPicker() { FileTypeFilter = { ".json" } };
            var file = await picker.PickSingleFileAsync();

            if (file == null)
                return;

            OpenPlaylist(file);
        }

        private void PlayNext()
        {
            bool found = false;
            foreach (var item in nav.MenuItems)
            {
                if (found)
                {
                    nav.SelectedItem = item;
                    break;
                } 
                else if (item == nav.SelectedItem)
                    found = true;
            }
        }

        private void PlayPrevious()
        {
            object prev = null;
            foreach (var item in nav.MenuItems)
            {
                if (item == nav.SelectedItem)
                    break;

                prev = item;
            }

            if (prev != null)
                nav.SelectedItem = prev;
        }

        private async void nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            _currentChannel = args.SelectedItem as Channel;
            player.Source = null;
            GC.Collect();

            if (_currentChannel.MediaType == null)
            {
                App.Message("Error", "Failed to get channel information");
                return;
            }

            try
            {
                player.Source = MediaSource.CreateFromAdaptiveMediaSource((await AdaptiveMediaSource.CreateFromStreamAsync(
                    _currentChannel.M3UStream.GetInputStreamAt((ulong)SeekOrigin.Begin),
                    _currentChannel.Uri,
                    _currentChannel.MediaType
                )).MediaSource);

                playerControls.Resolutions = _currentChannel.Streams.Values;
                playerControls.SelectedResolution = _currentChannel.Streams[(player.Source as MediaSource).AdaptiveMediaSource.InitialBitrate];
            }
            catch (Exception ex)
            {
                App.Message("Unexpected error", ex.Message);
            }

            if (playerControls.Visibility == Visibility.Visible && !_timer.IsEnabled)
                _timer.Start();        
        }

        private void playerControls_PlaylistTapped(object sender, TappedRoutedEventArgs e)
        {
            nav.IsPaneOpen = true;
        }

        private void playerControls_PreviousTapped(object sender, TappedRoutedEventArgs e)
        {
            PlayPrevious();
        }

        private void playerControls_NextTapped(object sender, TappedRoutedEventArgs e)
        {
            PlayNext();
        }

        private async void playerControls_ResolutionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentChannel == null || playerControls.SelectedResolution == null)
                return;

            try
            {
                var bitrate = (from s in _currentChannel.Streams where s.Value == playerControls.SelectedResolution select s.Key).FirstOrDefault();
                var mediaSource = player.Source as MediaSource;

                player.MediaPlayer.Pause();
                mediaSource.AdaptiveMediaSource.DesiredMaxBitrate = bitrate;
                mediaSource.AdaptiveMediaSource.InitialBitrate = bitrate;
                mediaSource.AdaptiveMediaSource.DesiredMinBitrate = bitrate;
                player.Source = mediaSource;
                player.MediaPlayer.Play();
            }
            catch (Exception ex)
            {
                App.Message("Unexpected error", ex.Message);
            }
        }

        private void playerControls_FullscreenTapped(object sender, TappedRoutedEventArgs e)
        {
            playerControls.IsPlaylistButtonEnabled = player.IsFullWindow;
        }
    }
}
