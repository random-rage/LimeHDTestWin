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
        public IEnumerable<string> Resolutions
        {
            get => from item in (GetTemplateChild("ResolutionCombo") as ComboBox).Items select item as string;
            set
            {
                var comboBox = GetTemplateChild("ResolutionCombo") as ComboBox;
                comboBox.Visibility = Visibility.Visible;
                comboBox.Items.Clear();

                foreach (var text in value)
                    comboBox.Items.Add(text);
            }
        }

        public string SelectedResolution
        {
            get => (GetTemplateChild("ResolutionCombo") as ComboBox).SelectedItem as string;
            set => (GetTemplateChild("ResolutionCombo") as ComboBox).SelectedItem = value;
        }

        public event TappedEventHandler FullscreenTapped;
        public event TappedEventHandler PlaylistTapped;
        public event TappedEventHandler NextTapped;
        public event TappedEventHandler PreviousTapped;
        public event SelectionChangedEventHandler ResolutionChanged;

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
            var resolutionCombo = GetTemplateChild("ResolutionCombo") as ComboBox;

            fullWindowButton.Tapped += FullWindowButton_Tapped;
            playlistButton.Tapped += PlaylistButton_Tapped;
            nextTrackButton.Tapped += NextTrackButton_Tapped;
            previousTrackButton.Tapped += PreviousTrackButton_Tapped;
            resolutionCombo.SelectionChanged += ResolutionCombo_SelectionChanged;

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

        private void ResolutionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResolutionChanged?.Invoke(sender, e);
        }
    }
}
