using OldDriverManagerV2.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml;

namespace OldDriverManagerV2.Util
{
    internal class MovieListHandler
    {
        private static List<Movie> _movies = new();
        public static List<Movie> movies = new();
        private static Random random = new();
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
            string file = GetInnerText(rootNode, "file");
            string cover = GetInnerText(rootNode, "cover");
            string fanart = GetInnerText(rootNode, "fanart");
            string website = GetInnerText(rootNode, "website");

            return new Movie(title, num, seller, tags, file, cover, fanart, website, nfo_path);
        }
        public static async Task GetAllMovies()
        {
            await Task.Run(() =>
            {
                _movies.Clear();
                movies.Clear();
                string root_path = Settings.Default.RootPath;
                if (!Directory.Exists(root_path)) return;
                string[] nfo_paths = Directory.GetFiles(root_path, "*.nfo", SearchOption.AllDirectories);
                //System.Diagnostics.Debug.WriteLine("Total Num: " + nfo_paths.Length);
                foreach (string nfo_path in nfo_paths)
                {
                    try
                    {
                        Movie? movie = GetMovie(nfo_path);
                        if (movie != null) _movies.Add(movie);
                    }
                    catch
                    {
                        //System.Diagnostics.Debug.WriteLine("Error: " + nfo_path);
                    }
                }
                //System.Diagnostics.Debug.WriteLine("Nfo Files Loaded: " + _movies.Count);
            });
        }
        public static async Task FilterMovies(string filterText)
        {
            List<string> keywords = filterText.Split(" ").ToList();
            await FilterMovies(keywords);
        }
        public static async Task FilterMovies(List<string> filterKeywords)
        {
            if (filterKeywords.Count == 0)
            {
                movies = _movies.ToList();
                return;
            }
            await Task.Run(() =>
            {
                IEnumerable<Movie> query = from movie in _movies 
                                           where movie.ContainsKeywords(filterKeywords) 
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
        public static bool OpenPathOfMovie(int index)
        {
            string path = movies[index].file;
            return OpenPathOfMovie(path);

        }
        public static bool OpenPathOfMovie(string path)
        {
            return OpenPathOfFile(path);
        }
        public static bool OpenPathOfFile(string path)
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
