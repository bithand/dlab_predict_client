using Flurl.Http;
using Predictorian.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
                            (dict_files[file].Tag as ProgressBar).Visibility = Visibility.Collapsed;
                            dict_files[file].Visibility = Visibility.Visible;
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
            border.BorderBrush = new SolidColorBrush(Colors.LightGreen);
        }

        private void list_box_DragLeave(object sender, DragEventArgs e)
        {
            border.BorderBrush = new SolidColorBrush(Colors.White);
            e.Effects = DragDropEffects.None;
        }


        protected bool first_clear = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (!first_clear)
            {
                list_box.Items.Clear();
                first_clear = true;
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                Trace.WriteLine($"file dropped {file}");
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".bmp") || file.EndsWith(".tiff") || file.EndsWith(".png"))
                {
                    //check transparency
                    if (file.EndsWith(".tiff") || file.EndsWith(".png") || file.EndsWith(".bmp"))
                    {
                        using(var bitmap = new Bitmap(file))
                        {
                            if (HasTransparency(bitmap))
                            {
                                continue;
                            }
                        }
                    }

                    if (!dict_files.ContainsKey(file))
                    {
                        AddMeasurementItem(file);
                        StartTask(file);

                    }
                }
            }
            border.BorderBrush = new SolidColorBrush(Colors.White);
           
            /*Thread.Sleep(500);
            foreach (string file in files)
            {
                Trace.WriteLine($"file dropped {file}");
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".bmp") || file.EndsWith(".tiff") || file.EndsWith(".bmp"))
                {
                    //check transparency
                    if (file.EndsWith(".tiff") || file.EndsWith(".png"))
                    {
                        using (var bitmap = new Bitmap(file))
                        {
                            if (HasTransparency(bitmap))
                            {
                                continue;
                            }
                        }
                    }

                    if (dict_files.ContainsKey(file))
                    {
                       
                    }
                }
            }*/
        }

        Dictionary<string, System.Windows.Controls.Image> dict_files = new Dictionary<string, System.Windows.Controls.Image>();
        public void AddMeasurementItem(string filename)
        {
            list_box.Dispatcher.Invoke(() =>
            {
                Border bor = new Border() { BorderBrush = new SolidColorBrush(Colors.AliceBlue), BorderThickness = new Thickness(4), Height = 400 };
                StackPanel sp = new StackPanel();
                sp.Children.Add(new Label() { Content = "[" + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss") + "] " + Path.GetFileName(filename) });
                DockPanel pan = new DockPanel() { LastChildFill = false };
                pan.Children.Add(new System.Windows.Controls.Image() { Source = new BitmapImage(new Uri(filename)),Width=400,Margin=new Thickness(20,0,0,0)});
                ProgressBar pb = new ProgressBar() { IsIndeterminate = true, Height = 15,Width=400,Margin=new Thickness(20,0,0,0),Foreground=new SolidColorBrush(Colors.LightBlue),Background=new SolidColorBrush(Colors.LightGray),BorderThickness=new Thickness(0) };


                System.Windows.Controls.Image img = new System.Windows.Controls.Image() { Source = null, Width = 400, Margin = new Thickness(10, 0, 0, 0),Tag=pb,Visibility=Visibility.Collapsed };
                
                pan.Children.Add(img);
                pan.Children.Add(pb);
                dict_files.Add(filename, img);
                sp.Children.Add(pan);
                bor.Child = sp;
                list_box.Items.Add(bor);
            });
        }

        public  bool HasTransparency(Bitmap bitmap)
        {
            // Not an alpha-capable color format. Note that GDI+ indexed images are alpha-capable on the palette.
            if (((ImageFlags)bitmap.Flags & ImageFlags.HasAlpha) == 0)
                return false;
            // Indexed format, and no alpha colours in the image's palette: immediate pass.
            if ((bitmap.PixelFormat & System.Drawing.Imaging.PixelFormat.Indexed) != 0 && bitmap.Palette.Entries.All(c => c.A == 255))
                return false;
            // Get the byte data 'as 32-bit ARGB'. This offers a converted version of the image data without modifying the original image.
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Int32 len = bitmap.Height * data.Stride;
            Byte[] bytes = new Byte[len];
            Marshal.Copy(data.Scan0, bytes, 0, len);
            bitmap.UnlockBits(data);
            // Check the alpha bytes in the data. Since the data is little-endian, the actual byte order is [BB GG RR AA]
            for (Int32 i = 3; i < len; i += 4)
                if (bytes[i] != 255)
                    return true;
            return false;
        }
    }
}
