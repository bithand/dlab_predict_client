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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            
        }

        public void StartTask(string file)
        {
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                string address = "http://office.bithand.de.com:8485/predict";
                Task<IFlurlResponse> t = DataHandler.GetMultipartData(address, file);
                try
                {
                    t.Wait();
                    Trace.WriteLine("call done");
                    Trace.WriteLine(t.Result.ResponseMessage.ToString());
                    if (t.Result.StatusCode == 200)
                    {
                        Task<byte[]> b = t.Result.GetBytesAsync();
                        b.Wait();
                        Trace.WriteLine(b.Result.Length);
                        //
                        if (!Directory.Exists("result")){
                            Directory.CreateDirectory("result");
                        }
                        string fileout = ("result/out_"  + Path.GetFileName(file));
                        string absolute_fileout = Path.GetFullPath(fileout);
                        File.WriteAllBytes(fileout, b.Result);
                        list_box.Dispatcher.Invoke(() =>
                        {
                            dict_files[file].Source = new BitmapImage(new Uri(absolute_fileout));
                            list_box.InvalidateVisual();
                            Trace.WriteLine($"reading ->{absolute_fileout}");
                        });
                        
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
            border.BorderBrush = new SolidColorBrush(Colors.Green);
        }

        private void list_box_DragLeave(object sender, DragEventArgs e)
        {
            border.BorderBrush = new SolidColorBrush(Colors.White);
            e.Effects = DragDropEffects.None;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                Trace.WriteLine($"file dropped {file}");
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                {
                    if (!dict_files.ContainsKey(file))
                    {
                        StartTask(file);
                        AddMeasurementItem(file);
                    }
                }
            }
            border.BorderBrush = new SolidColorBrush(Colors.White);
        }

        Dictionary<string, Image> dict_files = new Dictionary<string, Image>();
        public void AddMeasurementItem(string filename)
        {
            list_box.Dispatcher.Invoke(() =>
            {
                Border bor = new Border() { BorderBrush = new SolidColorBrush(Colors.AliceBlue), BorderThickness = new Thickness(4), Height = 400 };
                DockPanel pan = new DockPanel() { LastChildFill = false };
                pan.Children.Add(new Image() { Source = new BitmapImage(new Uri(filename)),Width=400,Margin=new Thickness(10,0,0,0)});
                Image img = new Image() { Source = null, Width = 400, Margin = new Thickness(10, 0, 0, 0) };
                pan.Children.Add(img);
                dict_files.Add(filename, img);
                bor.Child = pan;
                list_box.Items.Add(bor);
            });
        }

       
    }
}
