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
        //Thread mainThread;
        int maxVal = 30;
        double currentSliderPosition = 0;
        int frames;
        bool recording = false;
        PngBitmapEncoder[] pngImages;
        public MainWindow()
        {
            InitializeComponent();
        }

        #region GUI Events
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            SetInitialPositionToImage();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                imgFirst.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                imgFirst.Height = mainCanvas.ActualHeight;
                imgFirst.Width = mainCanvas.ActualWidth;
            }
        }

        private void btnOpenFile2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                imgSecond.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                imgSecond.Height = mainCanvas.ActualHeight;
                imgSecond.Width = mainCanvas.ActualWidth;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentSliderPosition = mainSlider.Value;

            if (radioButtonFromLeft.IsChecked == true)
                AnimationFromLeft(mainSlider.Value, mainSlider.Maximum);
            else if (radioButtonFromRight.IsChecked == true)
                AnimationFromRight(mainSlider.Value, mainSlider.Maximum);
            else if (radioButtonFromTop.IsChecked == true)
                AnimationFromTop(mainSlider.Value, mainSlider.Maximum);
            else if (radioButtonInBox.IsChecked == true)
                AnimationInBox(mainSlider.Value, mainSlider.Maximum);
            else if (radioButtonOutBox.IsChecked == true)
                AnimationOutBox(mainSlider.Value, mainSlider.Maximum);

            if (recording && mainSlider.Value > 0)
                SaveFramesToArray(mainSlider.Value);

            if (!(mainSlider.Value < mainSlider.Maximum))
            {
                //if (mainThread != null)
                //    mainThread.Abort();

                mainSlider.IsEnabled = true;

                if (recording)
                {
                    recording = false;
                    mainSlider.Maximum = maxVal;
                    mainSlider.Value = mainSlider.Maximum;
                    SaveFramesToFiles();
                }
            }
        }

        private async void btnStartAnimation_Click(object sender, RoutedEventArgs e)
        {
            SetInitialPositionToImage();

            imgFirst.Visibility = Visibility.Visible;

            mainSlider.Value = 0;
            mainSlider.IsEnabled = false;

            PerformAnimation();

            //mainThread = new Thread(new ThreadStart(PerformAnimation));
            //mainThread.Priority = ThreadPriority.Highest;
            //mainThread.Start();
        }

        private void btnStopAnimation_Click(object sender, RoutedEventArgs e)
        {
            //mainThread.Abort();
            mainSlider.IsEnabled = true;
        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mainSlider != null)
            {
                if (radioButtonFromLeft.IsChecked == true)
                    AnimationFromLeft(mainSlider.Value, maxVal);
                else if (radioButtonFromRight.IsChecked == true)
                    AnimationFromRight(mainSlider.Value, maxVal);
                else if (radioButtonFromTop.IsChecked == true)
                    AnimationFromTop(mainSlider.Value, mainSlider.Maximum);
                else if (radioButtonInBox.IsChecked == true)
                    AnimationInBox(mainSlider.Value, mainSlider.Maximum);
                else if (radioButtonOutBox.IsChecked == true)
                    AnimationOutBox(mainSlider.Value, mainSlider.Maximum);
            }
        }

        private async void btnSaveToFiles_Click(object sender, RoutedEventArgs e)
        {
            frames = (int)intUpDownFrames.Value;
            pngImages = new PngBitmapEncoder[frames];
            for (int i = 0; i < frames; ++i)
                pngImages[i] = new PngBitmapEncoder();

            recording = true;
            mainSlider.Value = 0;
            mainSlider.Maximum = frames;
            mainSlider.IsEnabled = false;

            PerformAnimation();
            //mainThread = new Thread(new ThreadStart(PerformAnimation));
            //mainThread.Start();
        }

        #endregion

        #region Animations

        private void AnimationFromLeft(double sliderPosition, double maxValue)
        {
            sliderPosition = maxValue - sliderPosition;
            Canvas.SetLeft(imgFirst, -mainCanvas.ActualWidth * sliderPosition / maxValue);
        }

        private void AnimationFromRight(double sliderPosition, double maxValue)
        {
            sliderPosition = maxVal - sliderPosition;
            Canvas.SetLeft(imgFirst, mainCanvas.ActualWidth * sliderPosition / maxValue);
        }

        private void AnimationFromTop(double sliderPosition, double maxValue)
        {
            Canvas.SetTop(imgFirst, -mainCanvas.ActualHeight + mainCanvas.ActualHeight * sliderPosition / maxValue);
        }

        private void AnimationInBox(double sliderPosition, double maxValue)
        {
            Canvas.SetTop(imgFirst, mainCanvas.ActualHeight / 2.0 - (mainCanvas.ActualHeight / 2.0) * sliderPosition / maxValue);
            Canvas.SetLeft(imgFirst, mainCanvas.ActualWidth / 2.0 - (mainCanvas.ActualWidth / 2.0) * sliderPosition / maxValue);
            imgFirst.Height = mainCanvas.ActualHeight * sliderPosition / maxValue;
            imgFirst.Width = mainCanvas.ActualWidth * sliderPosition / maxValue;
        }

        private void AnimationOutBox(double sliderPosition, double maxValue)
        {
            Canvas.SetTop(imgFirst, (mainCanvas.ActualHeight / 2.0) * sliderPosition / maxValue);
            Canvas.SetLeft(imgFirst, (mainCanvas.ActualWidth / 2.0) * sliderPosition / maxValue);
            imgFirst.Height = mainCanvas.ActualHeight * (1 - sliderPosition / maxValue);
            imgFirst.Width = mainCanvas.ActualWidth * (1 - sliderPosition / maxValue);
        }

        private void SetInitialPositionToImage()
        {
            Canvas.SetLeft(imgFirst, 0);
            Canvas.SetTop(imgFirst, 0);
            if (radioButtonFromLeft.IsChecked == true)
                Canvas.SetLeft(imgFirst, -mainCanvas.ActualWidth);
            else if (radioButtonFromRight.IsChecked == true)
                Canvas.SetLeft(imgFirst, mainCanvas.ActualWidth);
            else if (radioButtonFromTop.IsChecked == true)
                Canvas.SetTop(imgFirst, -mainCanvas.ActualHeight);
            else if (radioButtonInBox.IsChecked == true)
            {
                Canvas.SetTop(imgFirst, -mainCanvas.ActualHeight);
                Canvas.SetLeft(imgFirst, -mainCanvas.ActualWidth);
            }
            else if (radioButtonOutBox.IsChecked == true)
            {
                Canvas.SetTop(imgFirst, -mainCanvas.ActualHeight);
                Canvas.SetLeft(imgFirst, -mainCanvas.ActualWidth);
            }
        }

        private async void PerformAnimation()
        {
            while (currentSliderPosition < maxVal)
            {
                this.Dispatcher.Invoke(new Action(() => mainSlider.Value += 1));
                Thread.Sleep(50);
            }
        }

        #endregion

        #region Saving Frames

        private void SaveFramesToArray(double frame)
        {
            int pos = (int)frame - 1;
            int width = (int)mainCanvas.ActualWidth;
            int height = (int)mainCanvas.ActualHeight;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(mainCanvas);
            pngImages[pos].Frames.Add(BitmapFrame.Create(renderTargetBitmap));

        }

        private void SaveFramesToFiles()
        {
            for (int i = 1; i <= frames; ++i)
                using (Stream fileStream = File.Create("Frame " + i + ".png"))
                {
                    pngImages[i - 1].Save(fileStream);
                }
        }

        #endregion
    }
}
