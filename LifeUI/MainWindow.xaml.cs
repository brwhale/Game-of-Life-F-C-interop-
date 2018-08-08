using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using FunctionLib;
using Microsoft.FSharp.Collections;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FunctionsinWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int stride = 48;
        private bool running;
        private int prevIndex = -1;
        private double frametime;
        static Brush checkColor = Brushes.DodgerBlue;

        List<Brush> neighborColors = new List <Brush>(){            
            Brushes.White,
            Brushes.NavajoWhite,
            Brushes.Yellow,
            Brushes.CadetBlue,
            Brushes.Gray,
            Brushes.Yellow,
            Brushes.Orange,
            Brushes.OrangeRed,
            Brushes.Red,
        };

        public MainWindow()
        {
            InitializeComponent();

            var gs = (GridLength)new GridLengthConverter().ConvertFromString("*");            
            for (int i = stride; --i>=0;)
            {
                ChexGrid.RowDefinitions.Add(new RowDefinition { Height = gs });
                ChexGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = gs });
            }
            var h = 790.0;
            var w = h;
            for (int i = 0; i < stride; i++)
            {
                for (int j = 0; j < stride; j++)
                {
                    var box = new Rectangle() {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsHitTestVisible = false,
                        Height = h / stride,
                        Width = w / stride,
                        ClipToBounds = false,
                        Fill = neighborColors[0],
                        Tag = new Tuple<int, bool>(0,false)
                    };
                                        
                    Grid.SetRow(box, i);
                    Grid.SetColumn(box, j);
                    ChexGrid.Children.Add(box);
                }
            }
            ChexGrid.MouseMove += mouseHandler;            
            ChexGrid.MouseDown += (o, e) =>
            {
                prevIndex = -1;
            };
            ChexGrid.MouseDown += mouseHandler;
            setFPS(30);
            tickFrame();
        }

        private void setFPS(double newfps)
        {
            frametime = 1000 * 1.0 / newfps;
        }

        private int getIndex(Point pos, Grid grid)
        {
            var x = (int)(pos.X * stride / grid.ActualWidth);
            var y = (int)(pos.Y * stride / grid.ActualHeight);
            return y * stride + x;
        }
        private void mouseHandler(object o, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var grid = (Grid)o;
                var index = getIndex(e.GetPosition(grid), grid);
                if (index < grid.Children.Count && prevIndex != index)
                {
                    var box = ((Rectangle)grid.Children[index]);
                    var bb = (Tuple<int, bool>)box.Tag;
                    box.Tag = new Tuple<int,bool>(bb.Item1, !bb.Item2);
                    box.Fill = !bb.Item2 ? checkColor : neighborColors[bb.Item1]; ;
                    prevIndex = index;
                }
            }
        }

        private void functionStart(object sender, RoutedEventArgs e)
        {
            Action<string> fnc = (text) =>
            {
                Application.Current.Dispatcher.Invoke((() => { OutputBox.Text = text; }));
                
            };
            App.functionalmain(fnc);
        }

        private void setBoxes(FSharpList<Tuple<int,bool>> list)
        {
            int index = 0;
            foreach (Rectangle p in ChexGrid.Children)
            {
                var tp = list[index++];
                if (tp.Item2)
                {
                    p.Fill = checkColor;
                } else {
                    p.Fill = neighborColors[tp.Item1];
                }
                p.Tag = tp;
            }
        }

        private void setBoxes(bool value)
        {
            foreach (Rectangle p in ChexGrid.Children)
            {
                p.Tag = new Tuple<int, bool>(0,value);
                p.Fill = value ? checkColor : neighborColors[0];
            }
        }

        private bool[] getBoxes()
        {
            var list = new List<bool>();
            foreach (Rectangle p in ChexGrid.Children)
            {
                list.Add(((Tuple<int, bool>)p.Tag).Item2);
            }
            return list.ToArray();
        }

        private void tickFrame()
        {
            var t = new Stopwatch();
            t.Start();
            setBoxes(Life.lifeGameTick(getBoxes(), stride));
            t.Stop();
            var el = t.Elapsed.TotalMilliseconds;
            OutputBox.Text = $"generated in {el} ms";
        }

        private async Task tickFrameAsync()
        {
            var t = new Stopwatch();

            while (running)
            {
                t.Restart();
                // exit if the app is trying to quit
                if (!running) return;
                // get button states from ui thread
                bool[] list = new bool[0];
                Application.Current?.Dispatcher.Invoke((() =>
                {
                    list = getBoxes();
                }));
                // run function logic in background thread
                var ne = Life.lifeGameTick(list, stride);
                
                if (!running) return;
                // set button states from ui thread
                try
                {
                    Application.Current?.Dispatcher.Invoke(() => { setBoxes(ne); });
                } catch{} // it's fine if this fails

                // animate
                t.Stop();
                var el = t.Elapsed.TotalMilliseconds.Clamp(0, frametime);
                await Task.Delay(TimeSpan.FromMilliseconds(frametime - el));
            }
        }

        private void toggleLife(object sender, RoutedEventArgs e)
        {
            running = !running;
            if (running) 
            {
                Task.Run(()=>tickFrameAsync());
            }
        }

        private void frameButton(object sender, RoutedEventArgs e)
        {
            tickFrame();
        }

        private void fillbutton(object sender, RoutedEventArgs e)
        {
            running = false;
            setBoxes(true);
        }

        private void clearbutton(object sender, RoutedEventArgs e)
        {
            running = false;
            setBoxes(false);
            tickFrame();
        }
        
        private void fpsFromUI()
        {
            if (Double.TryParse(fpsBox.Text, out double fps))
                setFPS(fps);
        }

        private void clickSetFps(object sender, RoutedEventArgs e)
        {
            fpsFromUI();
        }

        private void fpsTyped(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                fpsFromUI();
            }
        }
    }
}