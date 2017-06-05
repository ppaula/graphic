using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace WpfApp1
{
    public class BrightnessEffect : ShaderEffect
    {
        public BrightnessEffect()
        {
            PixelShader = m_shader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BrightnessProperty);
            UpdateShaderValue(AlphaProperty);

        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BrightnessEffect), 0);

        public float Brightness
        {
            get { return (float)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(BrightnessEffect),
                new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));

        public float Alpha
        {
            get { return (float)GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(double), typeof(BrightnessEffect),
                new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        private static PixelShader m_shader = new PixelShader
        {
            UriSource = new Uri("pack://application:,,,/WpfApp1;component/bricon.ps")
        };

    }
    
}
