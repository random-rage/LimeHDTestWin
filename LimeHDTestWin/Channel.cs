using Newtonsoft.Json;
using OneWay.M3U;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

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

    class Channel : NavigationViewItem
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
        public new string Name
        {
            get => Content?.ToString();
            set => Content = value;
        }

        /// <summary>
        /// Состояние получение информации о канале
        /// </summary>
        public ChannelStatus Status { get; set; } = ChannelStatus.Loading;

        /// <summary>
        /// Качества видео
        /// </summary>
        public Dictionary<uint, string> Streams
        {
            get => (_streams == null) ? GetStreams() : _streams;
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
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.GetAsync(Uri).Completed = (requestInfo, requestStatus) =>
                        {
                            var content = requestInfo.GetResults().Content;
                            MediaType = content.Headers.ContentType.MediaType;

                            content.WriteToStreamAsync(M3UStream).Completed = (writingInfo, writingStatus) =>
                            {
                                Status = ChannelStatus.Success;
                                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>    // Меняем иконку в UI потоке
                                {
                                    Icon = new SymbolIcon((Symbol)0xE93E); // Streaming
                                });
                            };
                        };
                    }
                }
                catch (Exception)
                {
                    Status = ChannelStatus.Error;
                    Icon = new SymbolIcon((Symbol)0xE783); // Error
                }
            }
        }

        public Channel()
        {
            Icon = new SymbolIcon((Symbol)0xF16A); // ProgressRingDots
        }
        
        private Dictionary<uint, string> GetStreams()
        {
            switch (Status)
            {
                case ChannelStatus.Loading:
                    App.Message("Error", "Channel is not ready");
                    break;
                case ChannelStatus.Success:
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
                    }
                    break;
                case ChannelStatus.Error:
                    App.Message("Error", "Loading channel failed");
                    break;
                default:
                    break;
            }
            return new Dictionary<uint, string>();
        }
    }
}
