using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace LimeHDTestWin
{
    public sealed class LimeHDMediaTransportControls : MediaTransportControls
    {
        public bool IsPlaylistButtonEnabled
        {
            get => (GetTemplateChild("PlaylistButton") as AppBarButton).IsEnabled;
            set => (GetTemplateChild("PlaylistButton") as AppBarButton).IsEnabled = value;
        }

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
            var commandBar = GetTemplateChild("MediaControlsCommandBar") as CommandBar;
            var fullWindowButton = GetTemplateChild("FullWindowButton") as AppBarButton;
            var playlistButton = GetTemplateChild("PlaylistButton") as AppBarButton;
            var nextTrackButton = GetTemplateChild("NextTrackButton") as AppBarButton;
            var previousTrackButton = GetTemplateChild("PreviousTrackButton") as AppBarButton;
            var resolutionCombo = GetTemplateChild("ResolutionCombo") as ComboBox;
            var nextButton = GetTemplateChild("NextButton") as Button;
            var previousButton = GetTemplateChild("PreviousButton") as Button;

            commandBar.Tapped += CommandBar_Tapped;

            fullWindowButton.Tapped += FullWindowButton_Tapped;
            playlistButton.Tapped += PlaylistButton_Tapped;
            nextTrackButton.Tapped += NextTrackButton_Tapped;
            previousTrackButton.Tapped += PreviousTrackButton_Tapped;

            nextButton.Tapped += NextTrackButton_Tapped;
            previousButton.Tapped += PreviousTrackButton_Tapped;
            resolutionCombo.SelectionChanged += ResolutionCombo_SelectionChanged;

            base.OnApplyTemplate();
        }

        private void CommandBar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void FullWindowButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var nextButton = GetTemplateChild("NextButton") as Button;
            var previousButton = GetTemplateChild("PreviousButton") as Button;

            nextButton.Visibility = (nextButton.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            previousButton.Visibility = (previousButton.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;

            IsNextTrackButtonVisible = !IsNextTrackButtonVisible;
            IsPreviousTrackButtonVisible = !IsPreviousTrackButtonVisible;

            FullscreenTapped?.Invoke(sender, e);
        }

        private void PlaylistButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PlaylistTapped?.Invoke(sender, e);
        }

        private void NextTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NextTapped?.Invoke(sender, e);
            e.Handled = true;
        }
        private void PreviousTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PreviousTapped?.Invoke(sender, e);
            e.Handled = true;
        }

        private void ResolutionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResolutionChanged?.Invoke(sender, e);
        }
    }
}
