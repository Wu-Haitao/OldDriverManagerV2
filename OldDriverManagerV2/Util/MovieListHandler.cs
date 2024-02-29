using OldDriverManagerV2.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml;

namespace OldDriverManagerV2.Util
{
    internal class MovieListHandler
    {
        private static List<Movie> _movies = new(); // 原始影片列表
        public static List<Movie> movies = new(); // 经排序的用于显示的影片列表

        private static Random random = new();
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private static string GetInnerText(XmlNode rootNode, string nodeName)
        {
            XmlNode? node = rootNode.SelectSingleNode(nodeName);
            if (node != null) return node.InnerText;
            else return "";
        }

        private static List<string> GetInnerTextList(XmlNode rootNode, string nodeName)
        {
            List<string> list = new List<string>();
            XmlNodeList? nodeList = rootNode.SelectNodes(nodeName);
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    list.Add(node.InnerText);
                }
            }
            return list;
        }

        private static Movie? GetMovie(string nfo_path)
        {
            if (!File.Exists(nfo_path)) return null;

            XmlDocument xml = new();
            xml.Load(nfo_path);

            XmlNode? rootNode = xml.SelectSingleNode("movie");
            if (rootNode == null) return null;

            string title = GetInnerText(rootNode, "title");
            string num = GetInnerText(rootNode, "num");
            string seller = GetInnerText(rootNode, "seller");
            List<string> casts = GetInnerTextList(rootNode, "cast");
            List<string> tags = GetInnerTextList(rootNode, "tag");
            List<string> file = GetInnerTextList(rootNode, "file");
            string cover = GetInnerText(rootNode, "cover");
            string fanart = GetInnerText(rootNode, "fanart");
            string website = GetInnerText(rootNode, "website");

            return new Movie(title, num, seller, casts, tags, file, cover, fanart, website, nfo_path);
        }

        // 读取所有影片数据
        public static async Task GetAllMovies()
        {
            await semaphore.WaitAsync();
            await Task.Run(() =>
            {
                _movies.Clear();
                movies.Clear();
                string root_path = Settings.Default.RootPath;
                if (!Directory.Exists(root_path)) return;
                string[] nfo_paths = Directory.GetFiles(root_path, "*.nfo", SearchOption.AllDirectories);
                foreach (string nfo_path in nfo_paths)
                {
                    try
                    {
                        Movie? movie = GetMovie(nfo_path);
                        if (movie != null) _movies.Add(movie);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Error: " + nfo_path);
                    }
                }
                //System.Diagnostics.Debug.WriteLine("Nfo Files Loaded: " + _movies.Count);
            });
            semaphore.Release();
        }

        // 对影片列表进行筛选
        public static async Task FilterMovies(string filterText)
        {
            List<string> keywords = filterText.Split(" ").ToList();
            await FilterMovies(keywords);
        }
        public static async Task FilterMovies(List<string> filterKeywords)
        {
            await semaphore.WaitAsync();
            await Task.Run(() =>
            {
                if (filterKeywords.Count == 0)
                {
                    movies = _movies.ToList();
                }
                else
                {
                    IEnumerable<Movie> query = from movie in _movies
                                               where movie.ContainsKeywords(filterKeywords)
                                               select movie;
                    movies = query.ToList();
                }
            });
            semaphore.Release();
        }

        // 对影片列表进行排序
        public static async Task SortMovies()
        {
            await semaphore.WaitAsync();
            await Task.Run(() =>
            {
                switch (Settings.Default.SortFlag)
                {
                    case 0:
                        // Sort on num
                        movies.Sort((a, b) => string.Compare(a.num, b.num));
                        break;
                    case 1:
                        // Sort on num (reversed)
                        movies.Sort((a, b) => string.Compare(b.num, a.num));
                        break;
                    case 2:
                        // Sort randomly
                        int n = movies.Count;
                        while (n > 1)
                        {
                            n--;
                            int k = random.Next(n + 1);
                            (movies[n], movies[k]) = (movies[k], movies[n]);
                        }
                        break;
                    default:
                        break;
                }
            });
            semaphore.Release();
        }

        // 获取影片图片路径
        public static List<string> GetImgForMovie(Movie movie)
        {
            string[] imgExtensions = { ".jpg", ".png" };
            List<string> imgPaths = new();
            if (Directory.Exists(movie.fanart))
            {
                foreach (string imgExtension in imgExtensions)
                {
                    string[] paths = Directory.GetFiles(movie.fanart, "*" + imgExtension);
                    foreach (string path in paths) imgPaths.Add(path);
                }
            }
            return imgPaths;
        }

        // 播放影片
        public static bool PlayMovie(string path)
        {
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", path);
                return true;
            }
            else return false;
        }

        // 打开影片路径
        public static bool OpenPathOfMovie(string path)
        {
            return OpenPath(path);
        }

        // 打开文件路径
        public static bool OpenPath(string path)
        {
            if (File.Exists(path))
            {
                string argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
