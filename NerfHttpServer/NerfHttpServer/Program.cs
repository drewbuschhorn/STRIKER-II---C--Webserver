using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Data;

using HttpServer;
using HttpServer.Helpers;

namespace NerfHttpServer
{
    class Program
    {

        private static void Main(string[] args)
        {
            Program p = new Program();
            p.run();
        }

        public Striker2API.Usb usbToys;		// Striker II object

        private void run()
        {
            // create a new instance
            usbToys = new Striker2API.Usb();

            // init the Striker II, pass it the windows handle 
            usbToys.Init(Process.GetCurrentProcess().MainWindowHandle);

            bool connected = usbToys.isUSBConnected();
            if (!connected)
            {
                Console.WriteLine("No device connected.");
            }


            HttpListener listener = HttpListener.Create(System.Net.IPAddress.Any, 8089);
            listener.RequestReceived += OnRequest;
            listener.Start(5);

            Thread.Sleep(9000000);
        }

        private void OnRequest(object sender, HttpServer.RequestEventArgs e)
        {
            string[] paramss = e.Request.UriParts;
            HttpServer.IHttpResponse resp = e.Request.CreateResponse((IHttpClientContext)sender);

            if (paramss[0] == "favicon.ico")
            {
                reactFavIco(resp);
            }

            if (paramss[0] == "fire")
            {
                reactFire(resp);
            }

            if (paramss[0] == "move")
            {
                reactMove(resp,paramss[1]);
            }

            if (paramss[0] == "laser")
            {
                reactLaser(resp);
            }

            if (paramss[0] == "reset")
            {
                reactReset(resp);
            }
        }

        private static void reactFavIco(IHttpResponse resp)
        {
            Console.WriteLine("FavIco: Don't care");
            resp.Status = System.Net.HttpStatusCode.NotFound;
            close(resp, "");
        }

        private void reactFire(IHttpResponse resp)
        {
            Console.WriteLine("Fire!");
            usbToys.FireMissile();
            resp.Status = System.Net.HttpStatusCode.OK;
            close(resp, "Fire!");
        }

        private void reactMove(IHttpResponse resp, string MoveParam)
        {
            Console.WriteLine("Moving!");
            switch(MoveParam){
                case "left":
                    usbToys.MoveLeftKeyDown();
                    Thread.Sleep(1000);
                    usbToys.MoveLeftKeyUp();
                    break;
                case "right":
                    usbToys.MoveRightKeyDown();
                    Thread.Sleep(1000);
                    usbToys.MoveRightKeyUp();
                    break;
                case "up":
                    usbToys.MoveUpKeyDown();
                    Thread.Sleep(1000);
                    usbToys.MoveUpKeyUp();
                    break;
                case "down":
                    usbToys.MoveDownKeyDown();
                    Thread.Sleep(1000);
                    usbToys.MoveDownKeyUp();
                    break;
            }

            resp.Status = System.Net.HttpStatusCode.OK;
            close(resp, "Moving!"+MoveParam);
        }

        private void reactLaser(IHttpResponse resp)
        {
            Console.WriteLine("Laser!");
            usbToys.ToggleLaser();
            resp.Status = System.Net.HttpStatusCode.OK;
            close(resp, "Laser!");
        }

        private void reactReset(IHttpResponse resp)
        {
            usbToys.MoveLeftKeyDown();
            Thread.Sleep(10000);
            usbToys.MoveLeftKeyUp();

            usbToys.MoveUpKeyDown();
            Thread.Sleep(10000);
            usbToys.MoveUpKeyUp();

            resp.Status = System.Net.HttpStatusCode.OK;
            close(resp, "Reset!");
        }

        private static void close(IHttpResponse resp,string value)
        {
            resp.Connection = ConnectionType.Close;
            byte[] buffer = Encoding.UTF8.GetBytes("<html><body>"+value+"</body></html>");
            resp.Body.Write(buffer, 0, buffer.Length);
            resp.Send();
            System.Console.WriteLine("Complete");
        }
    }
}
