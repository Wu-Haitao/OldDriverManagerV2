using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace OldDriverManagerV2.Util
{
    internal static class LocalImageLoader
    {
        private static IJSRuntime? JS;
        private static ConcurrentDictionary<string, string> urls = new();
        private static ConcurrentDictionary<string, string> urls_0 = new();

        public static void Init(IJSRuntime js)
        {
            JS = js;
        }
        public static async Task<string> GenerateImgUrl(string? path, int index)
        {
            if (JS == null || path == null) return "";
            Byte[] file = await File.ReadAllBytesAsync(path);
            string url = await JS.InvokeAsync<string>("byteToUrl", file);
            switch (index)
            {
                case 0:
                    urls.TryAdd(path, url);
                    break;
                case 1:
                    urls_0.TryAdd(path, url);
                    break;
            }
            return url;
        }

        public static async Task RevokeAll(int index)
        {
            if (JS == null) return;
            switch (index)
            {
                case 0:
                    foreach (string url in urls.Values)
                    {
                        await JS.InvokeVoidAsync("revokeUrl", url);
                    }
                    urls.Clear();
                    break;
                case 1:
                    foreach (string url in urls_0.Values)
                    {
                        await JS.InvokeVoidAsync("revokeUrl", url);
                    }
                    urls_0.Clear();
                    break;
            }
        }
    }
}
