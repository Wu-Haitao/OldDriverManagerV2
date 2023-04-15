using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using WpfBlazor.Data;

namespace WpfBlazor.Util
{
    internal class FileHelper
    {
        private static List<Movie> _movies = new();
        public static List<Movie> movies = new();
        public static bool isLoaded = false;
        private static Random random = new Random();
        private static Movie? GetMovie(string nfo_path)
        {
            if (!File.Exists(nfo_path)) return null;

            XmlDocument xml = new();
            xml.Load(nfo_path);

            XmlNode? rootNode = xml.SelectSingleNode("movie");
            if (rootNode == null) return null;

            XmlNode? titleNode = rootNode.SelectSingleNode("title");
            string title = "";
            if (titleNode != null) title = titleNode.InnerText;

            XmlNode? numNode = rootNode.SelectSingleNode("num");
            string num = "";
            if (numNode != null) num = numNode.InnerText;

            XmlNode? sellerNode = rootNode.SelectSingleNode("seller");
            string seller = "";
            if (sellerNode != null) seller = sellerNode.InnerText;

            XmlNodeList? tagNodes = rootNode.SelectNodes("tag");
            List<string> tags = new();
            if (tagNodes != null)
            {
                foreach (XmlNode tagNode in tagNodes)
                {
                    tags.Add(tagNode.InnerText);
                }
            }

            XmlNode? fileNode = rootNode.SelectSingleNode("file");
            string file = "";
            if (fileNode != null) file = fileNode.InnerText;

            XmlNode? coverNode = rootNode.SelectSingleNode("cover");
            string cover = "";
            if (coverNode != null) cover = coverNode.InnerText;

            XmlNode? fanartNode = rootNode.SelectSingleNode("fanart");
            string fanart = "";
            if (fanartNode != null) fanart = fanartNode.InnerText;

            XmlNode? websiteNode = rootNode.SelectSingleNode("website");
            string website = "";
            if (websiteNode != null) website = websiteNode.InnerText;

            return new Movie(title, num, seller, tags, file, cover, fanart, website, nfo_path);
        }
        public static async Task GetAllMovies()
        {
            await Task.Run(() =>
            {
                _movies.Clear();
                isLoaded = true;
                string root_path = Settings.Default.RootPath;
                string[] nfo_paths = Directory.GetFiles(root_path, "*.nfo", SearchOption.AllDirectories);
                System.Diagnostics.Debug.WriteLine("Total Num: " + nfo_paths.Length);
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
                System.Diagnostics.Debug.WriteLine("Nfo Files Loaded: " + _movies.Count);
            });
        }
        public static async Task FilterMovies(string? filterKey)
        {
            if (filterKey == null || filterKey == "")
            {
                movies = _movies.ToList();
                return;
            }
            await Task.Run(() =>
            {
                var query = from movie in _movies
                            where movie.title.Contains(filterKey) || movie.num.Contains(filterKey) || movie.seller.Contains(filterKey) || movie.tags.Any(tag => tag.Contains(filterKey))
                            select movie;
                movies = query.ToList();
            });
        }
        public static async Task SortMovies()
        {
            await Task.Run(() =>
            {
                switch (Settings.Default.SortFlag)
                {
                    case 0:
                        //Sort on num
                        movies.Sort((a, b) => string.Compare(a.num, b.num));
                        break;
                    case 1:
                        //Sort randomly
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
        }
        public static async Task<List<string>> GetAllImgOfMovie(Movie movie)
        {
            string[] imgExtensions = { ".jpg", ".png" };
            List<string> imgPaths = new List<string>();
            await Task.Run(() =>
            {
                foreach (string imgExtension in imgExtensions)
                {
                    string[] paths = Directory.GetFiles(movie.fanart, "*" + imgExtension);
                    foreach (string path in paths) imgPaths.Add(path);
                }
            });
            return imgPaths;
        }
        public static bool PlayMovie(int index)
        {
            string path = movies[index].file;
            return PlayMovie(path);
        }
        public static bool PlayMovie(string path)
        {
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", path);
                return true;
            }
            else return false;
        }
        public static bool OpenPathOfMovie(string path)
        {
            if (File.Exists(path))
            {
                string argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);
                return true;
            }
            else return false;
        }
        public static bool OpenNFO(string path)
        {
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", path);
                return true;
            }
            else return false;
        }
    }
}
