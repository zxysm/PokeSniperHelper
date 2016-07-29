using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace pokesniperhelper
{
    class Program
    {

        static List<Pokemon> ToRemoveList = new List<Pokemon>();


        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [STAThread]
        static void Main()
        {
            List<Pokemon> pokelist;

            Console.WriteLine("AutoTyper for PokeSniper2");
            pokelist = LoadPokemons();
            while (true)
            {
                try
                {
                    Console.WriteLine("Loading pokemons");
                    pokelist = LoadPokemons();
                }
                catch
                {
                    Console.WriteLine("Cant load pokemons (Retrying)");
                }
                if(pokelist.Count > 0)
                {

                    foreach (Pokemon pk2 in ToRemoveList)
                    {
                        Pokemon pk = pokelist.Find(x => x.ToString() == pk2.ToString());
                        if (pk != null)
                        {
                            pokelist.Remove(pk);
                        }
                    }

                    Console.WriteLine("New pokemons: " + pokelist.Count);


                    Process[] processes = Process.GetProcessesByName("PokeSniper2");

                    foreach (Process proc in processes)
                    {
                        foreach (Pokemon poks in pokelist)
                        {
                            Console.WriteLine(poks.ToString());
                            SetForegroundWindow(proc.MainWindowHandle);
                            SendKeys.SendWait(poks.name);
                            Thread.Sleep(500);
                            SendKeys.SendWait("{ENTER}");
                            Thread.Sleep(1500);
                            
                            SendKeys.SendWait(poks.coords);
                            Thread.Sleep(500);
                            SendKeys.SendWait("{ENTER}");
                            Thread.Sleep(5000);
                            SendKeys.SendWait("{ENTER}");


                            ToRemoveList.Add(poks);
                        }
                    }

                    
                }
                pokelist.Clear();
                Thread.Sleep(5000);
            }
        }


        public static List<Pokemon> LoadPokemons()
        {
            List<Pokemon> pokelist = new List<Pokemon>();

            dynamic response = JsonConvert.DeserializeObject(Fetch("get", "http://pokesnipers.com/api/v1/pokemon.json"));
            if (response != null)
            {
                foreach(var pokemon in response.results)
                {
                    Pokemon poke = new Pokemon((string)pokemon.id, (string)pokemon.name, (string)pokemon.coords, (string)pokemon.until, (string)pokemon.icon);
                    pokelist.Add(poke);
                }
            }

            return pokelist;
        }


        private static string Fetch(string method, string url, NameValueCollection data = null)
        {

            HttpWebResponse response = Request(method.ToUpper(), url, data);
            Stream responseStream = response.GetResponseStream();
            if (responseStream == null)
                return "";
            else
            {
                StreamReader reader = new StreamReader(responseStream);
                return reader.ReadToEnd();
            }
        }

        private static HttpWebResponse Request(string method, string url, NameValueCollection data = null)
        {
            string postData = "";
            if (method == "POST")
            {
                foreach (string key in data.Keys)
                {
                    postData += String.Format("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]));
                }
                if (postData.Length > 0)
                    postData = postData.Remove(postData.Length - 1);

            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "application/json, text/javascript;q=0.9, */*;q=0.5";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Method = method;
            request.UseDefaultCredentials = true;
            //request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.6) Gecko/20060728 Firefox/1.5";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            request.Timeout = 30000;

            if (method == "POST")
            {
                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bytes.Length;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return request.GetResponse() as HttpWebResponse;

        }
    }
}
