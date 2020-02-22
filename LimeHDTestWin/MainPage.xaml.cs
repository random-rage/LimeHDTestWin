using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace LimeHDTestWin
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void openBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var picker = new FileOpenPicker() { FileTypeFilter = { ".json" } };
            var file = await picker.PickSingleFileAsync();

            if (file == null)
                return;

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
        }

        private void nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            player.Source = new Uri((args.SelectedItem as Channel).Url);
            player.Play();
        }
    }
}
