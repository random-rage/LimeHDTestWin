using Newtonsoft.Json;
using OneWay.M3U;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace LimeHDTestWin
{
    /// <summary>
    /// Состояние получение информации о канале
    /// </summary>
    public enum ChannelStatus
    {
        /// <summary>
        /// Идёт получение информации о канале
        /// </summary>
        Loading = 0,
        /// <summary>
        /// Информация успешно получена
        /// </summary>
        Success = 1,
        /// <summary>
        /// Не удалось получить информацию о канале
        /// </summary>
        Error = 2
    }

    class Channel
    {
        Dictionary<uint, string> _streams = null;

        /// <summary>
        /// Содержимое, полученное по ссылке Url
        /// </summary>
        public InMemoryRandomAccessStream M3UStream { get; set; } = new InMemoryRandomAccessStream();

        /// <summary>
        /// MIME-тип содержимого
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Название канала
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => NavigationItem.Content?.ToString();
            set => NavigationItem.Content = value;
        }

        /// <summary>
        /// Связанный элемент управления
        /// </summary>
        public NavigationViewItem NavigationItem { get; set; } = new NavigationViewItem() { Icon = new SymbolIcon((Symbol)0xF16A) };

        /// <summary>
        /// Состояние получение информации о канале
        /// </summary>
        public ChannelStatus Status { get; set; } = ChannelStatus.Loading;

        /// <summary>
        /// Качества видео
        /// </summary>
        public Dictionary<uint, string> Streams
        {
            get => _streams ?? GetStreams();
            set => _streams = value;
        }

        /// <summary>
        /// URI канала
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Адрес канала
        /// </summary>
        [JsonProperty("url")]
        public string Url
        {
            get => Uri?.ToString();
            set
            {
                Uri = new Uri(value);
                Task.Run(async () => {
                    try
                    {
                        var response = await App.HttpClient.GetAsync(Uri);
                        await response.Content.WriteToStreamAsync(M3UStream);
                        MediaType = response.Content.Headers.ContentType.MediaType;

                        Status = ChannelStatus.Success;
                        NavigationItem.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            NavigationItem.Icon = new SymbolIcon((Symbol)0xE93E); // Streaming
                        });
                    }
                    catch (Exception)
                    {
                        Status = ChannelStatus.Error;
                        NavigationItem.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            NavigationItem.Icon = new SymbolIcon((Symbol)0xE783); // Error
                        });
                    }
                });
            }
        }

        private Dictionary<uint, string> GetStreams()
        {
            try
            {
                M3UFileInfo m3uInfo;

                using (var m3uReader = new M3UFileReader(M3UStream.GetInputStreamAt((ulong)SeekOrigin.Begin).AsStreamForRead()))
                    m3uInfo = m3uReader.Read();

                return _streams = m3uInfo.Streams
                    .Where(s => s.Resolution != null && s.Bandwidth != null)
                    .ToDictionary(s => (uint)s.Bandwidth, s => s.Resolution);
            }
            catch (Exception ex)
            {
                App.Message("Unexpected error", ex.Message);
                return new Dictionary<uint, string>();
            }
        }
    }
}
