using Microsoft.WindowsAPICodePack.Dialogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops;
using System.Windows.Media;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool fileLoaded;
        private bool isPlaying;
        private string videoEyeHeatmap;
        private string videoEyeRaw;
        private string videoNormal;
        private string videoCaptura;
        private DispatcherTimer timerVideoTime = null;
        private bool sliderDragging = false;
        //MediaPlayer controla el audio
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Image imgPlay = new Image
        {
            Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/WpfApp1;component/Pictures/play.png"))
        };
        private Image imgStop = new Image
        {
            Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/WpfApp1;component/Pictures/stop2.png"))
        };

        public MainWindow()
        {
            InitializeComponent();
            System.Diagnostics.Debug.Write("Wow");
            this.fileLoaded = false;
            this.isPlaying = false;
            this.videoNormal = null;
            this.videoEyeRaw = null;
            this.videoCaptura = null;
            this.videoEyeHeatmap = null;

            if (IntPtr.Size == 4)
            {
                // Use 32 bits library
                this.MyControl.MediaPlayer.VlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "libvlc\\win-x86"));
                this.MyControl2.MediaPlayer.VlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "libvlc\\win-x86"));
            }
            else
            {
                // Use 64 bits library
                this.MyControl.MediaPlayer.VlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "libvlc\\win-x64"));
                this.MyControl2.MediaPlayer.VlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "libvlc\\win-x64"));
            }

            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };

            this.MyControl.MediaPlayer.VlcMediaplayerOptions = options;
            this.MyControl2.MediaPlayer.VlcMediaplayerOptions = options;

            // Load libvlc libraries and initializes stuff. It is important that the options (if you want to pass any) and lib directory are given before calling this method.
            this.MyControl.MediaPlayer.EndInit();
            this.MyControl2.MediaPlayer.EndInit();
            // This can also be called before EndInit
            this.MyControl.MediaPlayer.Log += (sender, args) =>
            {
                string message = string.Format("libVlc : {0} {1} @ {2}", args.Level, args.Message, args.Module);
                System.Diagnostics.Debug.WriteLine(message);
            };
            this.MyControl2.MediaPlayer.Log += (sender, args) =>
            {
                string message = string.Format("libVlc : {0} {1} @ {2}", args.Level, args.Message, args.Module);
                System.Diagnostics.Debug.WriteLine(message);
            };
            this.DataContext = this;

            var myController = new PlotController();

            myController.UnbindAll();
            myController.BindMouseDown(OxyMouseButton.Left, PlotCommands.Track);

            EEGPlot.Controller = myController;
            EEGPlotBetaalto.Controller = myController;
            EEGPlotBetabajo.Controller = myController;
            EEGPlotTheta.Controller = myController;
            EEGPlotGamma.Controller = myController;
            SerialPlot.Controller = myController;
            GSRPlot.Controller = myController;
           
        }
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public PlotModel EEGModel { get; set; }
        public PlotModel SerialModel { get; set; }
        public PlotModel BrainRModel { get; set; }
        /// <summary>
        /// Creates a model showing EEG data.
        /// </summary>
        /// <returns>A PlotModel.</returns>
        private static PlotModel CreateEEGModel(string EEGFile,int onda)
        {
            var plot = new PlotModel
            {
                Subtitle = "Ondas EEG",
            };
            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                TickStyle = OxyPlot.Axes.TickStyle.Inside,
                TextColor = OxyColors.Transparent,
                IsZoomEnabled = false
            });
            string[] lines = System.IO.File.ReadAllLines(EEGFile);
            float x, alpha, beta1, beta2, gamma, theta;
            var lineAlpha = new LineSeries { Title = "Alpha" };
            var lineBeta1 = new LineSeries { Title = "Beta1" };
            var lineBeta2 = new LineSeries { Title = "Beta2" };
            var lineGamma = new LineSeries { Title = "Gamma" };
            var lineTheta = new LineSeries { Title = "Theta" };
            foreach (string line in lines)
            {
                string[] data = line.Split(',');
                int dataLen = data.Length;
                int indexData = 16;
                x = float.Parse(data[0]);
                if(dataLen > 70)
                {
                    theta = 0;
                    alpha = 0;
                    beta1 = 0;
                    beta2 = 0;
                    gamma = 0;
                    int i;

                    for (i = 0; i < (dataLen-indexData-7) / 5; i++)
                    {
                        theta += float.Parse(data[indexData + (i * 5)]);
                        alpha += float.Parse(data[indexData + (i * 5) + 1]);
                        beta1 += float.Parse(data[indexData + (i * 5) + 2]);
                        beta2 += float.Parse(data[indexData + (i * 5) + 3]);
                        gamma += float.Parse(data[indexData + (i * 5) + 4]);
                    }

                    theta /= i+1;
                    alpha /= i+1;
                    beta1 /= i+1;
                    beta2 /= i+1;
                    gamma /= i+1;
                }
                else
                {
                    theta = float.Parse(data[indexData]);
                    alpha = float.Parse(data[indexData+1]);
                    beta1 = float.Parse(data[indexData+2]);
                    beta2 = float.Parse(data[indexData+3]);
                    gamma = float.Parse(data[indexData+4]);
                }
                
                lineAlpha.Points.Add(new DataPoint(x, alpha));
                lineBeta1.Points.Add(new DataPoint(x, beta1));
                lineBeta2.Points.Add(new DataPoint(x, beta2));
                lineGamma.Points.Add(new DataPoint(x, gamma));
                lineTheta.Points.Add(new DataPoint(x, theta));
            }
            switch (onda)
            {
                case 1:
                    lineAlpha.Color = OxyColors.Yellow;
                    plot.Series.Add(lineAlpha);
                    break;

                case 2:
                    lineBeta1.Color = OxyColors.SaddleBrown;
                    plot.Series.Add(lineBeta1);
                    break;

                case 3:
                    lineBeta2.Color = OxyColors.Red;
                    plot.Series.Add(lineBeta2);
                    break;

                case 4:
                    lineTheta.Color = OxyColors.HotPink;
                    plot.Series.Add(lineTheta);
                    break;

                case 5:
                    lineGamma.Color = OxyColors.MediumBlue;
                    plot.Series.Add(lineGamma);
                    
                    break;

                default:
                    return null;
            }
          
            return plot;
        }
        /// <summary>
        /// Creates a model showing EEG data.
        /// </summary>
        /// <returns>A PlotModel.</returns>
        private static PlotModel CreateBrainRModel(string EEGFile,int index)
        {
            var plot = new PlotModel
            {
                Subtitle = "Ritmos mentales",
            };
            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                TickStyle = OxyPlot.Axes.TickStyle.Inside,
                //MaximumRange = 1,
                TextColor = OxyColors.Transparent,
                IsZoomEnabled = false
            });    
            string[] lines = System.IO.File.ReadAllLines(EEGFile);
            var lineEntusiasmo = new LineSeries { Title = "Excitación" };
            var lineRelajacion = new LineSeries { Title = "Relajación" };
            var lineCompromiso = new LineSeries { Title = "Compromiso" };
            var lineEstres = new LineSeries { Title = "Frustración" };
            var lineInteres = new LineSeries { Title = "Valencia" };
            var lineConcentracion = new LineSeries { Title = "Atención" };
            float x, entusiasmo, relajacion, compromiso, estres, interes, concentracion;
            foreach (string line in lines)
            {
                string[] data = line.Split(',');
                int dataLen = data.Length;
                //System.Windows.MessageBox.Show("" +data.Length, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                x = float.Parse(data[0]);
                entusiasmo = float.Parse(data[dataLen-6]);
                relajacion = float.Parse(data[dataLen-5]);
                compromiso = float.Parse(data[dataLen-4]);
                estres = float.Parse(data[dataLen-3]);
                interes = float.Parse(data[dataLen-2]);
                concentracion = float.Parse(data[dataLen-1]);
                switch (index)
                {
                    case 1:
                        lineEntusiasmo.Points.Add(new DataPoint(x, entusiasmo));
                        lineEntusiasmo.Color = OxyColors.YellowGreen;
                        break;
                      
                    case 2:
                        lineRelajacion.Points.Add(new DataPoint(x, relajacion));
                        lineRelajacion.Color = OxyColors.DarkOrange;
                        break;

                    case 3:
                        lineCompromiso.Points.Add(new DataPoint(x, compromiso));
                        lineCompromiso.Color = OxyColors.Brown;
                        break;

                    case 4:
                        lineEstres.Points.Add(new DataPoint(x, estres));
                        lineEstres.Color = OxyColors.BlueViolet;
                        break;

                    case 5:
                        lineInteres.Points.Add(new DataPoint(x, interes));
                        lineInteres.Color = OxyColors.Fuchsia;
                        break;

                    case 6:
                        lineConcentracion.Points.Add(new DataPoint(x, concentracion));
                        lineConcentracion.Color = OxyColors.DarkOliveGreen;
                        break;
                    default:

                        break;
                }
            }
            switch(index)
            {
                case 1:
                    plot.Series.Add(lineEntusiasmo);
                    break;
                    
                case 2:
                    plot.Series.Add(lineRelajacion);
                    break;

                case 3:
                    plot.Series.Add(lineCompromiso);
                    break;

                case 4:
                    plot.Series.Add(lineEstres);
                    break;

                case 5:
                    plot.Series.Add(lineInteres);
                    break;

                case 6:
                    plot.Series.Add(lineConcentracion);
                    break;
                default:

                    break;
            }
           
            //
            //plot.Series.Add(lineCompromiso);
            //plot.Series.Add(lineEstres);
            //plot.Series.Add(lineInteres);
            //plot.Series.Add(lineConcentracion);

            return plot;
        }

        /// Creates a model showing Serial data.
        private static PlotModel CreateSerialModel(string SerialFile, int index)
        {
            //Gráfica ECG
            if (index==0){
                var plot = new PlotModel
                {
                    Subtitle = "Sensor ECG"
                };

                plot.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    TickStyle = OxyPlot.Axes.TickStyle.Inside,
                    TextColor = OxyColors.Transparent,
                    IsZoomEnabled = false
                });
                string[] lines = System.IO.File.ReadAllLines(SerialFile);
                var plotLines = new List<LineSeries>();
                foreach (string data in lines[0].Split(',').Skip(1))
                {
                    if (data.Split(':')[0].Contains("ECG"))
                    {
                        var plotLine = new LineSeries { Title = data.Split(':')[0] };
                        plotLines.Add(plotLine);
                        
                    }
                }
                float x, value;
                foreach (string line in lines)
                {
                    string[] row = line.Split(',');
                    x = float.Parse(row[0]);
                    foreach (string data in row.Skip(1))
                    {  
                        if (data.Contains("ECG"))
                        {
                            value = float.Parse(data.Split(':')[1]);
                            plotLines[0].Points.Add(new DataPoint(x, value));
                        }
                    }
                }
                foreach (LineSeries line in plotLines)
                {
                    line.Color = OxyColors.PaleVioletRed;
                    plot.Series.Add(line);
                }
                return plot;
            }
            //Gráfica GSR
            else if (index==1)
            {
                var plot = new PlotModel
                {
                    Subtitle = "Sensor GSR"
                };

                plot.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    TickStyle = OxyPlot.Axes.TickStyle.Inside,
                    TextColor = OxyColors.Transparent,
                    IsZoomEnabled = false
                });
                string[] lines = System.IO.File.ReadAllLines(SerialFile);
                var plotLines = new List<LineSeries>();
                foreach (string data in lines[0].Split(',').Skip(1))
                {
                    if (data.Split(':')[0].Contains("GSR"))
                    {
                        var plotLine = new LineSeries { Title = data.Split(':')[0] };
                        plotLines.Add(plotLine);
                    }
                }
                float x, value;

                foreach (string line in lines)
                {
                    string[] row = line.Split(',');
                    int i = 0;
                    x = float.Parse(row[0]);
                    foreach (string data in row.Skip(1))
                    {
                        if (data.Contains("GSR"))
                        {
                            value = float.Parse(data.Split(':')[1]);
                            plotLines[0].Points.Add(new DataPoint(x, value));
                        }
                        i++;
                    }
                }
                foreach (LineSeries line in plotLines)
                {
                    line.Color = OxyColors.ForestGreen;
                    plot.Series.Add(line);
                }
                return plot;
            }
            else{
                return null;
            }
        }

        //Método enlazado al evento de clic en botón de play
        /*
         Si es el primer clic se verifica que se hayan cargado los archivos a reproducir
         Luego de comenzar la reproduccion el evento funciona para pausar o continuar la reproduccion 
         de los elementos multimedia
             */
        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.fileLoaded)
            {
                System.Windows.MessageBox.Show("Cargar directorio de archivos en 'Menú>Abrir grabación'", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (this.isPlaying)
            {
                MyControl.MediaPlayer.Pause();
                if (MyControl2!=null)
                {
                    MyControl2.MediaPlayer.Pause();
                }
                mediaPlayer.Pause();
                this.isPlaying = false;
                imgBtnPlay.Source = imgPlay.Source;
            }
            else
            {
                MyControl.MediaPlayer.Play();
                if (MyControl2!=null)
                {
                    MyControl2.MediaPlayer.Play();
                }
                mediaPlayer.Play();
                this.isPlaying = true;
                imgBtnPlay.Source = imgStop.Source;
            }
            if (!timeSlider.IsEnabled)
            {
                timeSlider.IsEnabled = true;
            }
        }

        private void OnForwardButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.fileLoaded)
            {
                System.Windows.MessageBox.Show("Cargar directorio de archivos con 'Abrir directorio'", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //MyControl.MediaPlayer.Time = (long)(10+  MyControl.MediaPlayer.GetCurrentMedia().Duration.TotalSeconds * 1000);
                MyControl.MediaPlayer.Position = (float) (MyControl.MediaPlayer.Position + 0.01);
                mediaPlayer.Position = mediaPlayer.Position.Add(new TimeSpan(0, 0, 0,1,10));
                System.Windows.MessageBox.Show(""+MyControl.MediaPlayer.Position, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Windows.MessageBox.Show("" + mediaPlayer.Position, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                if (MyControl2!=null)
                {
                    MyControl2.MediaPlayer.Position = (float)(MyControl2.MediaPlayer.Position + 0.01);
                }
                timeSlider.Value = MyControl.MediaPlayer.Position;    
            }
        }

        private void OnRewindButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.fileLoaded)
            {
                System.Windows.MessageBox.Show("Cargar directorio de archivos con 'Abrir directorio'", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MyControl.MediaPlayer.Position = (float)(MyControl.MediaPlayer.Position - 0.01);
                mediaPlayer.Position = mediaPlayer.Position.Add(new TimeSpan(0, 0,0,1, -10));
                if (MyControl2!=null)
                {
                    MyControl2.MediaPlayer.Position = (float)(MyControl2.MediaPlayer.Position - 0.1);
                }
                timeSlider.Value = MyControl.MediaPlayer.Position;
            }
        }

        //Método que termina la aplicación al dar clic en la opción salir del menú
        private void exit_menuClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        //Método que abre una ventana donde se navega a la carpeta que contiene los archvivos a analizar
        private void OpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                EnsureFileExists = false,
                AllowNonFileSystemItems = false,
                DefaultFileName = "Seleccionar folder",
                Title = "Selecciona la carpeta con los archivos para analizar",
                IsFolderPicker = true
            };
            this.fileLoaded = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string[] files = Directory.GetFiles(dialog.FileName);
                this.videoNormal = null;
                this.videoEyeRaw = null;
                this.videoEyeHeatmap = null;

                int videos_counter = 0;

                foreach (string file in files)
                {
                    if (file.Contains("VideoFull") && file.Contains(".mp4"))
                    {
                        if (file.Contains("Heatmap"))
                        {
                            this.videoEyeHeatmap = file;
                        }
                        else if (file.Contains("Raw"))
                        {
                            this.videoEyeRaw = file;
                        }
                        else
                        {
                            this.videoNormal = file;
                        }
                        videos_counter++;
                    }
                    else if (file.Contains("Video_") && file.Contains(".mp4") && this.videoNormal == null)
                    {
                        this.videoNormal = file;
                    }
                    else if (file.Contains("Video_") && file.Contains(".mp4") && this.videoCaptura == null && this.videoNormal!=null) {
                        this.videoCaptura = file;
                    }
                    else if (file.Contains("EEG") && file.Contains(".csv"))
                    {
                        this.EEGModel = CreateEEGModel(file,1);
                        EEGPlot.Model = this.EEGModel;
                        EEGPlot.InvalidatePlot(true);

                        //this.EEGModel = CreateEEGModel(file);
                        EEGPlotBetaalto.Model = CreateEEGModel(file,2);
                        EEGPlotBetaalto.InvalidatePlot(true);

                        EEGPlotBetabajo.Model = CreateEEGModel(file,3);
                        EEGPlotBetabajo.InvalidatePlot(true);

                        EEGPlotTheta.Model = CreateEEGModel(file,4);
                        EEGPlotTheta.InvalidatePlot(true);

                        EEGPlotGamma.Model = CreateEEGModel(file,5);
                        EEGPlotGamma.InvalidatePlot(true);

                        this.BrainRModel = CreateBrainRModel(file,1);
                        BrainRPlot.Model = this.BrainRModel;
                        BrainRPlot.InvalidatePlot(true);

                        //this.BrainRModel = CreateBrainRModel(file);
                        BrainRelajacionPlot.Model = CreateBrainRModel(file,2);
                        BrainRelajacionPlot.InvalidatePlot(true);

                        BrainCompromisoPlot.Model = CreateBrainRModel(file, 3);
                        BrainCompromisoPlot.InvalidatePlot(true);

                        BrainEstresPlot.Model = CreateBrainRModel(file, 4);
                        BrainEstresPlot.InvalidatePlot(true);

                        BrainInteresPlot.Model = CreateBrainRModel(file, 5);
                        BrainInteresPlot.InvalidatePlot(true);

                        BrainConcentracionPlot.Model = CreateBrainRModel(file, 6);
                        BrainConcentracionPlot.InvalidatePlot(true);

                        this.fileLoaded = true;
                        lineEEG.Visibility = Visibility.Visible;
                        lineEEGBetaalto.Visibility = Visibility.Visible;
                        lineEEGBetabajo.Visibility = Visibility.Visible;
                        lineEEGTheta.Visibility = Visibility.Visible;
                        lineEEGGamma.Visibility = Visibility.Visible;
                        lineBrainR.Visibility = Visibility.Visible;
                        lineBrainRelajacion.Visibility = Visibility.Visible;
                        lineBrainCompromiso.Visibility = Visibility.Visible;
                        lineBrainEstres.Visibility = Visibility.Visible;
                        lineBrainInteres.Visibility = Visibility.Visible;
                        lineBrainConcentracion.Visibility = Visibility.Visible;
                    }
                    else if (file.Contains("Serial") && file.Contains(".csv"))
                    {
                        string line = File.ReadLines(file).First();
                        if (line.Contains("ECG"))
                        {
                            this.SerialModel = CreateSerialModel(file, 0);
                            SerialPlot.Model = this.SerialModel;
                            SerialPlot.InvalidatePlot(true);
                            lineECG.Visibility = Visibility.Visible;
                            lineGSR.Visibility = Visibility.Visible;
                        }
                        if (line.Contains("GSR"))
                        {
                            GSRPlot.Model = CreateSerialModel(file, 1); ;
                            GSRPlot.InvalidatePlot(true);
                        }
                        if (line.Contains("EMG"))
                        {
                            //Aquí iria el sensor de EMG
                         }
                        this.fileLoaded = true;
                    }
                    else if(file.Contains("Audio") && file.Contains(".wav")){
                        mediaPlayer.Open(new Uri(file));
                    }
                }
     
                if (this.videoEyeHeatmap != null)
                {
                    if (this.timerVideoTime != null)
                    {
                        this.timerVideoTime.Stop();
                        this.timerVideoTime = null;
                    }
                    MyControl.MediaPlayer.SetMedia(new FileInfo(this.videoEyeHeatmap));
                    this.fileLoaded = true;
                    // Create a timer that will update the counters and the time slider
                    this.timerVideoTime = new DispatcherTimer();
                    this.timerVideoTime.Tick += new EventHandler(timer_Tick);
                    this.timerVideoTime.Start();
                }
                else if (this.videoEyeRaw != null)
                {
                    if (this.timerVideoTime != null)
                    {
                        this.timerVideoTime.Stop();
                        this.timerVideoTime = null;
                    }
                    MyControl.MediaPlayer.SetMedia(new FileInfo(this.videoEyeRaw));
                    this.fileLoaded = true;
                    // Create a timer that will update the counters and the time slider
                    this.timerVideoTime = new DispatcherTimer();
                    this.timerVideoTime.Tick += new EventHandler(timer_Tick);
                    this.timerVideoTime.Start();
                }
                else if (this.videoNormal != null)
                {
                    if (this.timerVideoTime != null)
                    {
                        this.timerVideoTime.Stop();
                        this.timerVideoTime = null;
                    }
                    MyControl.MediaPlayer.SetMedia(new FileInfo(this.videoNormal));
                    this.fileLoaded = true;
                    // Create a timer that will update the counters and the time slider
                    this.timerVideoTime = new DispatcherTimer();
                    this.timerVideoTime.Tick += new EventHandler(timer_Tick);
                    this.timerVideoTime.Start();
                }
                if (this.videoCaptura != null)
                {
                    if (this.timerVideoTime != null)
                    {
                        this.timerVideoTime.Stop();
                        this.timerVideoTime = null;
                    }
                    MyControl2.MediaPlayer.SetMedia(new FileInfo(this.videoCaptura));
                    this.fileLoaded = true;
                    // Create a timer that will update the counters and the time slider
                    this.timerVideoTime = new DispatcherTimer();
                    this.timerVideoTime.Tick += new EventHandler(timer_Tick);
                    this.timerVideoTime.Start();
                }
                else
                {
                    MyControl2 = null;
                }
            }
        }

        //Método que se ejecuta cuando se suelta el control del slider 
        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //La barra se debe deshabilitar si no esta un video cargado
            if (MyControl.MediaPlayer.Time > (long)(((Slider)sender).Value * MyControl.MediaPlayer.GetCurrentMedia().Duration.TotalSeconds * 1000))
            {
                System.Windows.MessageBox.Show("Drag hacia atras", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Windows.MessageBox.Show("Posicion:"+mediaPlayer.Position +" Total:"+mediaPlayer.NaturalDuration, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Windows.MessageBox.Show("Posicion:"  + (mediaPlayer.NaturalDuration- mediaPlayer.Position), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Drag hacia adelante", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            MyControl.MediaPlayer.Time = (long)(((Slider)sender).Value * MyControl.MediaPlayer.GetCurrentMedia().Duration.TotalSeconds * 1000);
            //mediaPlayer.Position= mediaPlayer.Position.Add(new TimeSpan(0, 0, 0, 0, mediaPlayer.Position.Milliseconds));
           // mediaPlayer.Position.
            if (MyControl2!=null)
            {
                MyControl2.MediaPlayer.Time = (long)(((Slider)sender).Value * MyControl2.MediaPlayer.GetCurrentMedia().Duration.TotalSeconds * 1000);
            }
            
            if (this.isPlaying)
            {
                MyControl.MediaPlayer.Play();
                if (MyControl2 != null)
                {
                    MyControl2.MediaPlayer.Play();
                }
            }
            this.sliderDragging = false;
        }

        //Método que se ejecuta al pulsar el botón del slider
        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (!this.sliderDragging)
            {
                //MyControl.MediaPlayer.Pause();
                this.sliderDragging = true;
            }
        }

        //Método que se esta ejecutando cada cierto periodo de tiempo y va actualizando la posicion de las lineas del avance de la reproducción
        void timer_Tick(object sender, EventArgs e)
        {
            // Check if the movie finished calculate it's total time
            if (MyControl.MediaPlayer.Length > 0)
            {
                if (!this.sliderDragging)
                {
                    // Updating time slider
                    timeSlider.Value = MyControl.MediaPlayer.Position;
                }
                // Updating EEG lines position
                lineEEG.X1 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEG.X2 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGBetaalto.X1 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGBetaalto.X2 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGBetabajo.X1 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGBetabajo.X2 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGTheta.X1 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGTheta.X2 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGGamma.X1 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineEEGGamma.X2 = ((EEGPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                // Updating Serial sensors lines position
                lineECG.X1 = ((SerialPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineECG.X2 = ((SerialPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineGSR.X1 = ((GSRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineGSR.X2 = ((GSRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                // Updating Ritmos mentales lines position
                lineBrainR.X1 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineBrainR.X2 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                lineBrainRelajacion.X1 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineBrainRelajacion.X2 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                lineBrainCompromiso.X1 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineBrainCompromiso.X2 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                lineBrainEstres.X1 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineBrainEstres.X2 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                lineBrainInteres.X1 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineBrainInteres.X2 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);

                lineBrainConcentracion.X1 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                lineBrainConcentracion.X2 = ((BrainRPlot.ActualWidth - 70) * MyControl.MediaPlayer.Position);
                //Si se llega al final de la reproducción, los valores de apuntan al inicio de la reproducción
                if (timeSlider.Value == 1)
                {
                    MyControl.MediaPlayer.Stop();
                    MyControl.MediaPlayer.Time = 0;
                    MyControl2.MediaPlayer.Stop();
                    MyControl2  .MediaPlayer.Time = 0;
                    this.isPlaying = false;
                    imgBtnPlay.Source = imgPlay.Source;
                }
            }
        }
    }
}