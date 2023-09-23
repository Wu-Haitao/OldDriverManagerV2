using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OldDriverManagerV2.Util
{
    internal static class LocalImageLoader
    {
        private static IJSRuntime? JS;
        private static ConcurrentDictionary<string, string> urls = new();

        public static void Init(IJSRuntime js)
        {
            JS = js;
        }

        public static async Task<string> GenerateImgUrl(string? path)
        {
            if (JS == null || path == null) return "";
            string? url;
            if (!urls.ContainsKey(path))
            {
                byte[] file = await File.ReadAllBytesAsync(path);
                url = await JS.InvokeAsync<string>("byteToUrl", file);
                urls.TryAdd(path, url);
            }
            else
            {
                url = urls[path];
            }
            Debug.WriteLine(urls.Count);

            if (url != null) return url;
            else return "";
        }


        public static async Task RevokePath(string path)
        {
            if (JS == null) return;
            if (urls.ContainsKey(path))
            {
                string? url;
                urls.TryRemove(path, out url);
                if (!string.IsNullOrEmpty(url))
                {
                    await JS.InvokeVoidAsync("revokeUrl", url);
                }
            }
            Debug.WriteLine(urls.Count);

        }

    }
}
