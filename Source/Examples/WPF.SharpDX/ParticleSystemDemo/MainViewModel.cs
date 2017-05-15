﻿using DemoCore;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ParticleSystemDemo
{
    public class MainViewModel : BaseViewModel
    {
        private Stream particleTexture;
        public Stream ParticleTexture
        {
            set
            {
                SetValue(ref particleTexture, value);
            }
            get
            {
                return particleTexture;
            }
        }

        private Vector3D acceleration = new Vector3D(0, 1, 0);
        public Vector3D Acceleration
        {
            set
            {
                SetValue(ref acceleration, value);
            }
            get
            {
                return acceleration;
            }
        }

        private int accelerationX = 0;
        public int AccelerationX
        {
            set
            {
                if(SetValue(ref accelerationX, value))
                {
                    UpdateAcceleration();
                }
            }
            get
            {
                return accelerationX;
            }
        }

        private Size particlesize = new Size(0.1, 0.1);
        public Size ParticleSize
        {
            set
            {
                SetValue(ref particlesize, value);
            }
            get
            {
                return particlesize;
            }
        }

        private int sizeSlider = 10;
        public int SizeSlider
        {
            set
            {
                if(SetValue(ref sizeSlider, value))
                {
                    ParticleSize = new Size(((double)value)/100, ((double)value)/100);
                }
            }
            get
            {
                return sizeSlider;
            }
        }

        private int accelerationY = 100;
        public int AccelerationY
        {
            set
            {
                if (SetValue(ref accelerationY, value))
                {
                    UpdateAcceleration();
                }
            }
            get
            {
                return accelerationY;
            }
        }

        private int accelerationZ = 0;
        public int AccelerationZ
        {
            set
            {
                if (SetValue(ref accelerationZ, value))
                {
                    UpdateAcceleration();
                }
            }
            get
            {
                return accelerationZ;
            }
        }

        const int DefaultBoundScale = 10;
        public LineGeometry3D BoundingLines { private set; get; }

        public ScaleTransform3D BoundingLineTransform { private set; get; } = new ScaleTransform3D(DefaultBoundScale, DefaultBoundScale, DefaultBoundScale);

        private Rect3D particleBounds = new Rect3D(0, 0, 0, DefaultBoundScale, DefaultBoundScale, DefaultBoundScale);
        public Rect3D ParticleBounds
        {
            set
            {
                SetValue(ref particleBounds, value);
            }
            get
            {
                return particleBounds;
            }
        }

        private int boundScale = DefaultBoundScale;
        public int BoundScale
        {
            set
            {
                if(SetValue(ref boundScale, value))
                {
                    ParticleBounds = new Rect3D(0, 0, 0, value, value, value);
                    BoundingLineTransform.ScaleX = BoundingLineTransform.ScaleY = BoundingLineTransform.ScaleZ = value;
                }
            }
            get
            {
                return boundScale;
            }
        }

        private Color blendColor = Colors.White;
        public Color BlendColor
        {
            set
            {
                if(SetValue(ref blendColor, value))
                {
                    BlendColorBrush = new SolidColorBrush(value);
                }
            }
            get
            {
                return blendColor;
            }
        }

        private int redValue = 255;
        public int RedValue
        {
            set
            {
                if(SetValue(ref redValue, value))
                {
                    BlendColor = Color.FromRgb((byte)RedValue, (byte)GreenValue, (byte)BlueValue);
                }
            }
            get
            {
                return redValue;
            }
        }

        private int greenValue = 255;
        public int GreenValue
        {
            set
            {
                if (SetValue(ref greenValue, value))
                {
                    BlendColor = Color.FromRgb((byte)RedValue, (byte)GreenValue, (byte)BlueValue);
                }
            }
            get
            {
                return greenValue;
            }
        }

        private int blueValue = 255;
        public int BlueValue
        {
            set
            {
                if (SetValue(ref blueValue, value))
                {
                    BlendColor = Color.FromRgb((byte)RedValue, (byte)GreenValue, (byte)BlueValue);
                }
            }
            get
            {
                return blueValue;
            }
        }

        private SolidColorBrush blendColorBrush = new SolidColorBrush(Colors.White);
        public SolidColorBrush BlendColorBrush
        {
            set
            {
                SetValue(ref blendColorBrush, value);
            }
            get
            {
                return blendColorBrush;
            }
        }
        private int numTextureRows;
        public int NumTextureRows
        {
            set
            {
                SetValue(ref numTextureRows, value);
            }
            get
            {
                return numTextureRows;
            }
        }

        private int numTextureColumns;
        public int NumTextureColumns
        {
            set
            {
                SetValue(ref numTextureColumns, value);
            }
            get
            {
                return numTextureColumns;
            }
        }

        private int selectedTextureIndex = 0;
        public int SelectedTextureIndex
        {
            set
            {
                if(SetValue(ref selectedTextureIndex, value))
                {
                    LoadTexture(value);
                }
            }
            get
            {
                return selectedTextureIndex;
            }
        }

        public readonly Tuple<int, int>[] TextureColumnsRows = new Tuple<int, int>[] { new Tuple<int, int>(1, 1), new Tuple<int, int>(4, 4), new Tuple<int, int>(4, 4) };
        public readonly string[] Textures = new string[] {@"Snowflake.png", @"FXT_Explosion_Fireball_Atlas_d.png", @"FXT_Sparks_01_Atlas_d.png"};
        public readonly int[] DefaultParticleSizes = new int[] { 20, 90, 10 };
        public MainViewModel()
        {
            var lineBuilder = new LineBuilder();
            lineBuilder.AddBox(new SharpDX.Vector3(), 1, 1, 1);
            BoundingLines = lineBuilder.ToLineGeometry3D();
            LoadTexture(SelectedTextureIndex);
        }

        private void LoadTexture(int index)
        {
            ParticleTexture = new FileStream(new System.Uri(Textures[index], System.UriKind.RelativeOrAbsolute).ToString(), FileMode.Open);
            NumTextureColumns = TextureColumnsRows[index].Item1;
            NumTextureRows = TextureColumnsRows[index].Item2;
            SizeSlider = DefaultParticleSizes[index];
        }

        private void UpdateAcceleration()
        {
            Acceleration = new Vector3D((double)AccelerationX/100, (double)AccelerationY /100, (double)AccelerationZ /100);
        }
    }
}
