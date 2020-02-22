using OneWay.M3U;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LimeHDTestWin
{
    class Channel : NavigationViewItem
    {
        M3UFileInfo m3uInfo;

        /// <summary>
        /// Название канала
        /// </summary>
        public new string Name
        {
            get => Content.ToString();
            set => Content = value;
        }

        /// <summary>
        /// Адрес канала
        /// </summary>
        public string Url { get; set; }

        public Channel()
        {
            Icon = new SymbolIcon((Symbol)0xE93E);
            /*try
            {
                using (var m3uReader = new M3UFileReader(new Uri(url)))
                    m3uInfo = m3uReader.Read();
            }
            catch (Exception ex)
            {
                (new Windows.UI.Popups.MessageDialog(ex.Message, "Error")).ShowAsync();
            }*/
        }
    }
}
