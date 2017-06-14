using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Aby dodać nową animacje trzeba:
        /// - utworzyć nowy wpis w enum z typami animacji w AnimationViewModel
        /// - dodać RadioButton w MainWindow i podpiąc do niego Binding, w CommandParameter trzeba podac nazwe Animacji taką samą jak w enum
        /// - w setterze SliderValue wywolac wybrana animacje, no i wcześniej ja napisac :D
        /// </summary>
        AnimationViewModel animationViewModel;
        public MainWindow()
        {
            InitializeComponent();
            animationViewModel = new AnimationViewModel();
            //Ustawienie contextu - dzieki temu dzialaja bindingi zrobione w MainWindow.xaml
            DataContext = animationViewModel;
            //Obsluzenie eventu AnimationStopped
            animationViewModel.AnimationStopped += AnimationViewModel_AnimationStopped;
        }

        //Zatrzymanie animacji - zrobienie ostatniego screena :D
        private void AnimationViewModel_AnimationStopped(object sender, EventArgs e)
        {
            //Dispatcher, poniewaz event jest wywolywany z klasy AnimationViewModel i nie ma dostepu do elementów z MainWindow
            //nie wiem jak to inaczej obejść, zawsze tak robie
            Dispatcher.Invoke(new Action(() => SaveFramesToArray()));
            //zapisanie do pliku, zapisuje na koniec zeby nie zamulać podczas animacji
            if (animationViewModel.SaveAnimations)
                SaveFramesToFiles();
        }

        //zapisywanie zawartości Canvas - naszych obrazków
        private void SaveFramesToArray()
        {
            int width = (int)mainCanvas.ActualWidth;
            int height = (int)mainCanvas.ActualHeight;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(mainCanvas);
            var obj = new PngBitmapEncoder();
            obj.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            animationViewModel.Frames.Add(obj);
        }

        //zapisywanie ramek do pliku
        private void SaveFramesToFiles()
        {
            for (int i = 1; i < animationViewModel.Frames.Count - 1; ++i)
            {
                using (Stream fileStream = File.Create("Frame " + i.ToString("00") + ".png"))
                {
                    Dispatcher.Invoke(new Action(() => animationViewModel.Frames[i].Save(fileStream)));
                }
            }
        }

        private void mainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider mySlider = sender as Slider;
            double val = mySlider.Value;
            if ((int)(val * 10.0) % 10 == 0)
                if (animationViewModel.SaveAnimations)
                    SaveFramesToArray();
        }
    }
}
