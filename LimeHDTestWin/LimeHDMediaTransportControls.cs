using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace LimeHDTestWin
{
    public sealed class LimeHDMediaTransportControls : MediaTransportControls
    {
        public event TappedEventHandler FullscreenTapped;
        public event TappedEventHandler PlaylistTapped;
        public event TappedEventHandler NextTapped;
        public event TappedEventHandler PreviousTapped;

        public LimeHDMediaTransportControls()
        {
            DefaultStyleKey = typeof(LimeHDMediaTransportControls);
        }

        protected override void OnApplyTemplate()
        {
            var fullWindowButton = GetTemplateChild("FullWindowButton") as AppBarButton;
            var playlistButton = GetTemplateChild("PlaylistButton") as AppBarButton;
            var nextTrackButton = GetTemplateChild("NextTrackButton") as AppBarButton;
            var previousTrackButton = GetTemplateChild("PreviousTrackButton") as AppBarButton;

            fullWindowButton.Tapped += FullWindowButton_Tapped;
            playlistButton.Tapped += PlaylistButton_Tapped;
            nextTrackButton.Tapped += NextTrackButton_Tapped;
            previousTrackButton.Tapped += PreviousTrackButton_Tapped;

            base.OnApplyTemplate();
        }
        
        private void FullWindowButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FullscreenTapped?.Invoke(sender, e);
        }

        private void PlaylistButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PlaylistTapped?.Invoke(sender, e);
        }

        private void NextTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NextTapped?.Invoke(sender, e);
        }
        private void PreviousTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PreviousTapped?.Invoke(sender, e);
        }
    }
}
