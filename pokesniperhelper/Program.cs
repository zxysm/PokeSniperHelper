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

using WebSocketSharp;


namespace pokesniperhelper
{
    class Program
    {
        static WebSocket ws;
        static bool only_new = false;

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [STAThread]
        static void Main()
        {
            Console.Title = "AutoTyper for PokeSniper2 by Shadow";
            Console.WriteLine("AutoTyper for PokeSniper2");
            Console.WriteLine("Only new pokemons? Y/N");
            while(true)
            {
                string key = Console.ReadLine();
                if (key.ToLower() == "y")
                {
                    only_new = true;
                    break;
                }
                else if (key.ToLower() == "n")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Wrong key");
                }

            }

            ws = new WebSocket("ws://spawns.sebastienvercammen.be:49002/socket.io/?EIO=3&transport=websocket");
            ws.OnMessage += Ws_OnMessage;
            ws.OnOpen += Ws_OnOpen;
            ws.OnError += Ws_OnError;
            ws.OnClose += Ws_OnClose;

            ws.Connect();

            Console.ReadKey();
           
        }

        private static void TypePokemon(string name, string lat, string lon)
        {
            Process[] processes = Process.GetProcessesByName("PokeSniper2");

            foreach (Process proc in processes)
            {
                SetForegroundWindow(proc.MainWindowHandle);
                SendKeys.SendWait(name.Replace("'",""));
                //Thread.Sleep(500);
                SendKeys.SendWait("{ENTER}");
                //Thread.Sleep(1500);

                SendKeys.SendWait(String.Format("{0},{1}", lat, lon.Replace("\nExpires", "").Replace("Expires","")));
               // Thread.Sleep(500);
                SendKeys.SendWait("{ENTER}");
              //  Thread.Sleep(5000);
                SendKeys.SendWait("{ENTER}");
            }
        }

        

        private static void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            string msg = e.Data;
            //Console.WriteLine("Message: " + e.Data);
            if(msg== "3")
                Console.WriteLine("Pong");
            else if(msg.Contains("42"))
            {
                if (msg.Contains("helo"))
                {
                    dynamic response = JsonConvert.DeserializeObject(msg.Remove(0, 2));
                    if (response != null)
                    {
                        if(!only_new)
                        {
                            foreach (var pokemon in response[1])
                            {
                                //pokemon.name
                                //pokemon.lat
                                //pokemon.lon
                                //pokemon.IV
                                //pokemon.channel
                                //pokemon.userId
                                //pokemon.server

                                Console.WriteLine(String.Format("Pokemon: {0} ({1},{2}) {3}", pokemon.name, pokemon.lat, pokemon.lon, pokemon.IV));
                                TypePokemon((string)pokemon.name, (string)pokemon.lat, (string)pokemon.lon);

                            }
                        }
                        
                    }
                }
                else if(msg.Contains("poke"))
                {
                    dynamic response = JsonConvert.DeserializeObject(msg.Remove(0, 2));
                    if (response != null)
                    {

                        //response[1].name
                        //response[1].lat
                        //response[1].lon
                        //response[1].IV

                        Console.WriteLine(String.Format("New pokemon: {0} ({1},{2}) {3}", response[1].name, response[1].lat, response[1].lon, response[1].IV));
                        TypePokemon((string)response[1].name, (string)response[1].lat, (string)response[1].lon);

                    }
                }
            }
        }

        private static void Ws_OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to server");
            ws.Send("2probe");
            ws.Send("5");
            new Thread(() => PingTimer()).Start();
        }

        private static void Ws_OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine("Disconnected from server " + e.Code);
        }

        private static void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.Message);
        }

       

        private static void PingTimer()
        {
            while(true)
            {
                Thread.Sleep(25000);
                Console.WriteLine("Ping");
                if (ws.IsAlive)
                    ws.Send("2");
                else
                    break;
                
            }
        }
    }
}
