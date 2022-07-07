using Flurl.Http;
using Predictorian.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace Predictorian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                string address = "http://office.bithand.de.com:8485/predict";
                string file = "image_1.jpg";
                Task<IFlurlResponse> t = DataHandler.GetMultipartData(address, file);
                t.Wait();
                Trace.WriteLine("call done");
                Trace.WriteLine(t.Result.ResponseMessage.ToString());
                if (t.Result.StatusCode == 200)
                {
                    Task<byte[]> b = t.Result.GetBytesAsync();
                    b.Wait();
                    Trace.WriteLine(b.Result.Length);
                    File.WriteAllBytes("out.jpg", b.Result);
                }
            });
        }
    }
}
