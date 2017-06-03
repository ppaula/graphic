using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;

namespace WpfApp1
{
    public enum AnimationType { SlideFromLeft, SlideFromRight, SlideFromTop, SlideFromBottom, InBox, OutBox, BrightnessOffOn };
    public class AnimationViewModel : INotifyPropertyChanged
    {
        Thread mainThread;

        private const double mainCanvasHeight = 620;
        private const double mainCanvasWidth = 1090;
        public AnimationViewModel()
        {
            //ustawienie poczatkowe wartości
            sliderMinimum = 0;
            sliderMaximum = 30;
            SliderValue = 0;
            heightImg1 = 620;
            heightImg2 = 620;
            widthImg1 = 1090;
            widthImg2 = 1090;
            Image1ZIndex = 1;
            Image2ZIndex = 0;

            //inicjalizacji komend, ktore są podpiete pod przyciski w MainWindow
            StartAnimationCommand = new RelayCommand(StartAnimation);
            StopAnimationCommand = new RelayCommand(StopAnimation);
            LoadImage1Command = new RelayCommand(LoadImage1);
            LoadImage2Command = new RelayCommand(LoadImage2);

            //lista zawierajaca zapisane ramki
            Frames = new List<PngBitmapEncoder>();
        }

        #region Img1 Properties
        private double canvasLeftImg1;
        public double CanvasLeftImg1
        {
            get
            {
                return canvasLeftImg1;
            }
        }

        private double canvasRightImg1;
        public double CanvasRightImg1
        {
            get
            {
                return canvasRightImg1;
            }
        }

        private double canvasTopImg1;
        public double CanvasTopImg1
        {
            get
            {
                return canvasTopImg1;
            }
        }

        private double canvasBottomImg1;
        public double CanvasBottomImg1
        {
            get
            {
                return canvasBottomImg1;
            }
        }

        private double heightImg1;
        public double HeightImg1
        {
            get
            {
                return heightImg1;
            }
        }

        private double widthImg1;
        public double WidthImg1
        {
            get
            {
                return widthImg1;
            }
        }
        public BitmapImage Image1Source { get; set; }
        private BitmapImage Image1SourceOriginal { get; set; }
        //ZIndex - ktory obrazek jest "na wierzchu"
        public int Image1ZIndex { get; set; }

        #endregion

        #region Img2 Properties

        private double canvasLeftImg2;
        public double CanvasLeftImg2
        {
            get
            {
                return canvasLeftImg2;
            }
        }

        private double canvasRightImg2;
        public double CanvasRightImg2
        {
            get
            {
                return canvasRightImg2;
            }
        }

        private double canvasTopImg2;
        public double CanvasTopImg2
        {
            get
            {
                return canvasTopImg2;
            }
        }

        private double canvasBottomImg2;
        public double CanvasBottomImg2
        {
            get
            {
                return canvasBottomImg2;
            }
        }

        private double heightImg2;
        public double HeightImg2
        {
            get
            {
                return heightImg2;
            }
        }

        private double widthImg2;
        public double WidthImg2
        {
            get
            {
                return widthImg2;
            }
        }
        public BitmapImage Image2Source { get; set; }
        private BitmapImage Image2SourceOriginal { get; set; }
        //ZIndes - który obrazek jest "na wierzchu"
        public int Image2ZIndex { get; set; }

        #endregion

        #region Animation/Slider Properties

        private AnimationType animationType;
        public AnimationType AnimationType
        {
            get
            {
                return animationType;
            }
            set
            {
                animationType = value;
                SetInitialPositionToImage();
                ChooseAnimation();
                ChangeProperty("AnimationType");
            }
        }

        private double sliderValue;
        public double SliderValue
        {
            get
            {
                return sliderValue;
            }
            set
            {
                sliderValue = value;

                //Wywolanie eventu, że zmieniła się wartość slidera - aktualnie niewykorzystane, chcialem tym przyspieszyc zapis ramek
                OnSliderValueChanged();

                //Wybor animacji
                ChooseAnimation();

                //Powiadomienie, ze zmienila sie wartosc, aby zaktualizowala sie pozycja slidera w MainWindow
                ChangeProperty("SliderValue");
            }
        }

        private double sliderMinimum;
        public double SliderMinimum
        {
            get
            {
                return sliderMinimum;
            }
            set
            {
                sliderMinimum = value;
                //Notyfikacja, ze zmienila sie wartosc SliderMinimum
                ChangeProperty("SliderMinimum");
            }
        }

        private double sliderMaximum;
        public double SliderMaximum
        {
            get
            {
                return sliderMaximum;
            }
            set
            {
                sliderMaximum = value;
                //Notyfikacja, ze zmienila sie wartosc SliderMaximum
                ChangeProperty("SliderMaximum");
            }
        }


        public List<PngBitmapEncoder> Frames { get; set; }

        //propercja sprawdzajaca czy ma byc zapisywanie do pliku, binding podpiety pod checkbox
        public bool SaveAnimations { get; set; }

        #endregion

        #region Commands
        //komendy, do ktorych jest binding w MainWindow
        public ICommand LoadImage1Command { get; private set; }
        public ICommand LoadImage2Command { get; private set; }
        public ICommand StartAnimationCommand { get; private set; }
        public ICommand StopAnimationCommand { get; private set; }

        //metody wywoływane przez poszczegolne komendy
        private void LoadImage1()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Image1Source = new BitmapImage(new Uri(openFileDialog.FileName));
                Image1SourceOriginal = new BitmapImage(Image1Source.UriSource);
            }
            //Notyfikacja ze zmienil sie obrazek 1
            ChangeProperty("Image1Source");
        }

        

        private void LoadImage2()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                Image2Source = new BitmapImage(new Uri(openFileDialog.FileName));
                Image2SourceOriginal = new BitmapImage(Image2Source.UriSource);
            }
            //Notyfikacja, ze zmienil sie obrazek 2
            ChangeProperty("Image2Source");
        }
        private void StartAnimation()
        {
            //wyczyszczenie aktualnych ramek
            Frames.Clear();
            //wyzerowanie slidera
            SliderValue = 0;
            //wyzerowanie pozycji obrazkow
            SetInitialPositionToImage();
            //watek aby dzialalo w tle
            mainThread = new Thread(new ThreadStart(PerformAnimation));
            mainThread.Priority = ThreadPriority.Highest;
            mainThread.Start();

            //Wywolanie eventu rozpoczecia animacji - aktualnie niewykorzystane
            if (SaveAnimations)
                OnAnimationStarted();
        }
        private void StopAnimation()
        {
            //zatrzymanie watku
            mainThread.Abort();
            SliderValue = 0;

            //Wywolanie eventu zakonczenia animacji - wykorzystany do dopisania ostatniego screena
            if (SaveAnimations)
                OnAnimationStopped();
        }
        //Metoda wykonywana w wątku
        private void PerformAnimation()
        {
            var sleep_step = (int)(300 / sliderMaximum);
            while (sliderValue < sliderMaximum)
            {
                SliderValue += 0.1;
                Thread.Sleep(sleep_step);
            }

            OnAnimationStopped();
        }

        #endregion

        #region INotifyPropertyChange implementation

        public event PropertyChangedEventHandler PropertyChanged;
        //implementacja interfejsu INotifyPropertyChanged koniecznego do tego, aby kontrolki w MainWindow sie aktualizowaly
        //Jako argument należy przekazywać nazwe PROPERCJI ktora ulegla zmienie, najczesciej uzywane w setterze, ale tez w innych miejscach
        void ChangeProperty(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region AnimationEvents

        //eventy animacji, wywolywane w metodach On(...)
        public event EventHandler SliderValueChanged;
        public event EventHandler AnimationStarted;
        public event EventHandler AnimationStopped;

        //Metody wywolujace eventy wyzej
        void OnSliderValueChanged()
        {
            if (SliderValueChanged != null)
            {
                SliderValueChanged(this, new EventArgs());
            }
        }
        void OnAnimationStarted()
        {
            if (AnimationStarted != null)
            {
                AnimationStarted(this, new EventArgs());
            }
        }
        void OnAnimationStopped()
        {
            if (AnimationStopped != null)
            {
                AnimationStopped(this, new EventArgs());
            }
        }

        #endregion

        #region Animations

        private void ChooseAnimation()
        {
            switch (animationType)
            {
                case AnimationType.SlideFromLeft:
                    AnimationFromLeft();
                    break;
                case AnimationType.SlideFromRight:
                    AnimationFromRight();
                    break;
                case AnimationType.SlideFromTop:
                    AnimationFromTop();
                    break;
                case AnimationType.SlideFromBottom:
                    AnimationFromBottom();
                    break;
                case AnimationType.InBox:
                    AnimationInBox();
                    break;
                case AnimationType.OutBox:
                    AnimationOutBox();
                    break;
                case AnimationType.BrightnessOffOn:
                    BrightnessOffOn();
                    break;
                default:
                    break;
            }
        }

        private void AnimationFromLeft()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 1;
            Image2ZIndex = 0;

            //obliczenie pozycji obrazkow
            double sliderPosition = sliderMaximum - sliderValue;
            canvasLeftImg1 = -mainCanvasWidth * sliderPosition / sliderMaximum;

            //powiadomienie o zmianie propercji
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("CanvasLeftImg1");
        }
        private void AnimationFromRight()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 1;
            Image2ZIndex = 0;

            //obliczenie polozen
            double sliderposition = sliderMaximum - sliderValue;
            canvasLeftImg1 = mainCanvasWidth * sliderposition / sliderMaximum;

            //Notyfikacja
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("CanvasLeftImg1");
        }
        private void AnimationFromTop()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 1;
            Image2ZIndex = 0;

            //Obliczenia
            canvasTopImg1 = -mainCanvasHeight + mainCanvasHeight * sliderValue / sliderMaximum;

            //Notyfikacja
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("CanvasTopImg1");
        }
        private void AnimationFromBottom()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 1;
            Image2ZIndex = 0;

            //Obliczenia
            canvasTopImg1 = mainCanvasHeight - sliderValue / sliderMaximum * mainCanvasHeight;

            //Notyfikacja
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("CanvasTopImg1");
        }
        private void AnimationInBox()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 1;
            Image2ZIndex = 0;

            //Obliczenia
            canvasTopImg1 = mainCanvasHeight / 2.0 - (mainCanvasHeight / 2.0) * sliderValue / sliderMaximum;
            canvasLeftImg1 = mainCanvasWidth / 2.0 - (mainCanvasWidth / 2.0) * sliderValue / sliderMaximum;
            heightImg1 = mainCanvasHeight * sliderValue / sliderMaximum;
            widthImg1 = mainCanvasWidth * sliderValue / sliderMaximum;

            //Notyfikacja
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("CanvasTopImg1");
            ChangeProperty("CanvasLeftImg1");
            ChangeProperty("HeightImg1");
            ChangeProperty("WidthImg1");
        }
        private void AnimationOutBox()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 0;
            Image2ZIndex = 1;

            //Obliczenia
            canvasTopImg2 = (mainCanvasHeight / 2.0) * sliderValue / sliderMaximum;
            canvasLeftImg2 = (mainCanvasWidth / 2.0) * sliderValue / sliderMaximum;
            heightImg2 = mainCanvasHeight * (1 - sliderValue / sliderMaximum);
            widthImg2 = mainCanvasWidth * (1 - sliderValue / sliderMaximum);

            //Notyfikacja
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("CanvasTopImg2");
            ChangeProperty("CanvasLeftImg2");
            ChangeProperty("HeightImg2");
            ChangeProperty("WidthImg2");
        }

        private void BrightnessOffOn()
        {
            //ustawienie kolejnosci obrazkow
            Image1ZIndex = 0;
            Image2ZIndex = 1;

            //Obliczenia
            if(sliderValue <= 15)
            {
                int brightness_value = (int)((sliderValue / 15) * 255);
                WriteableBitmap writeableBitmap = ChangeBrightness(Image1Source, -brightness_value);
                BitmapImage writeableBitmapConverted = ConvertWriteableBitmapToBitmapImage(writeableBitmap);
                Image1Source = writeableBitmapConverted;
            }
            else
            {
                Image1ZIndex = 1;
                Image2ZIndex = 0;
                int brightness_value = (int)(((sliderValue - 15) / 15) * 255 - 255);
                WriteableBitmap writeableBitmap = ChangeBrightness(Image2Source, brightness_value);
                BitmapImage writeableBitmapConverted = ConvertWriteableBitmapToBitmapImage(writeableBitmap);
                Image2Source = writeableBitmapConverted;
            }


            //Notyfikacja
            ChangeProperty("Image1ZIndex");
            ChangeProperty("Image2ZIndex");
            ChangeProperty("Image1Source");
            ChangeProperty("Image2Source");

            //Kopiujemy Image1Source i Image2Source z oryginału, aby nie rozjaśniać w nieskończoność
            Image1Source = new BitmapImage(Image1SourceOriginal.UriSource);
            Image2Source = new BitmapImage(Image2SourceOriginal.UriSource);
        }
        private void SetInitialPositionToImage()
        {
            canvasLeftImg1 = 0;
            canvasRightImg1 = 0;
            canvasTopImg1 = 0;
            canvasBottomImg1 = 0;
            canvasLeftImg2 = 0;
            canvasRightImg2 = 0;
            canvasTopImg1 = 0;
            canvasTopImg2 = 0;
            widthImg1 = 1090;
            widthImg2 = 1090;
            heightImg1 = 620;
            heightImg2 = 620;

            NotifyAnimationPositionsChanged();
        }
        //notyfikacja o zmianie wszystkich zmiennych
        private void NotifyAnimationPositionsChanged()
        {
            ChangeProperty("CanvasLeftImg1");
            ChangeProperty("CanvasRightImg1");
            ChangeProperty("CanvasTopImg1");
            ChangeProperty("CanvasBottomImg1");
            ChangeProperty("CanvasLeftImg2");
            ChangeProperty("CanvasRightImg2");
            ChangeProperty("CanvasTopImg2");
            ChangeProperty("CanvasBottomImg2");
            ChangeProperty("HeightImg1");
            ChangeProperty("HeightImg2");
            ChangeProperty("WidthImg1");
            ChangeProperty("WidthImg2");
        }


        //zmiana jasności obrazka przekazanego jako source
        private WriteableBitmap ChangeBrightness(BitmapImage source, int val)
        {
            var myWriteableBitmap = new WriteableBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, source.Format, null);


            int stride = source.PixelWidth * 4;
            int size = source.PixelHeight * stride;
            byte[] pixels = new byte[size];
            source.CopyPixels(pixels, stride, 0);

            for (int x = 0; x < source.PixelWidth; x++)
            {
                for (int y = 0; y < source.PixelHeight; y++)
                {
                    int index = y * stride + 4 * x;

                    int R = (pixels[index] + val);
                    int G = (pixels[index + 1] + val);
                    int B = (pixels[index + 2] + val);

                    if (R > 255) R = 255;
                    if (G > 255) G = 255;
                    if (B > 255) B = 255;
                    if (R < 0) R = 0;
                    if (G < 0) G = 0;
                    if (B < 0) B = 0;

                    pixels[index] = Convert.ToByte(R);      //R
                    pixels[index + 1] = Convert.ToByte(G);  //G
                    pixels[index + 2] = Convert.ToByte(B);  //B
                }
            }

            myWriteableBitmap.WritePixels (
                new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
                pixels,
                stride,
                0 );

            // return the new bitmap
            return myWriteableBitmap;
        }

        //konwerter z WriteableBitmap do BitmapImage
        //konieczny z racji tego, że zapisywać pixele można tylko do WriteableBitmap
        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
        #endregion
    }
}
