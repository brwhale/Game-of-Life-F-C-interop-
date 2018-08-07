using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using FunctionLib;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

namespace FunctionsinWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int stride = 45;
        bool running = false;
        int prevIndex = -1;
        private double frametime = 1000 * 1.0 / 60.0;

        int getIndex(Point pos, Grid grid)
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
                if (index < grid.Children.Count && prevIndex != index) {
                    var box = ((CheckBox)grid.Children[index]);
                    box.IsChecked = !box.IsChecked;
                    prevIndex = index;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            var gs = (GridLength)new GridLengthConverter().ConvertFromString("*");            
            for (int i = stride; --i>=0;)
            {
                ChexGrid.RowDefinitions.Add(new RowDefinition() { Height = gs });
                ChexGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = gs });
            }

            for (int i = 0; i < stride; i++)
            {
                for (int j = 0; j < stride; j++)
                {
                    var box = new CheckBox() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };

                    ChexGrid.Children.Add(box);
                    Grid.SetRow(box, i);
                    Grid.SetColumn(box, j);
                }
            }
            ChexGrid.MouseMove += mouseHandler;            
            ChexGrid.MouseDown += (o, e) =>
            {
                prevIndex = -1;
            };
            ChexGrid.MouseDown += mouseHandler;
        }

        private void functionStart(object sender, RoutedEventArgs e)
        {
            Action<string> fnc = (text) =>
            {
                Application.Current.Dispatcher.Invoke((() => { OutputBox.Text = text; }));
                
            };
            App.functionalmain(fnc);
        }

        void setBoxes(FSharpList<bool> list)
        {
            int index = 0;
            foreach (CheckBox p in ChexGrid.Children)
            {
                p.IsChecked = list[index++];
            }
        }

        void setBoxes(bool value)
        {
            foreach (CheckBox p in ChexGrid.Children)
            {
                p.IsChecked = value;
            }
        }

        bool[] getBoxes()
        {
            var list = new List<bool>();
            foreach (CheckBox p in ChexGrid.Children)
            {
                list.Add(p.IsChecked ?? false);
            }
            return list.ToArray();
        }

        void tickFrame()
        {
            var t = new Stopwatch();
            t.Start();
            setBoxes(Life.lifeGameTick(getBoxes(), stride));
            t.Stop();
            var el = t.Elapsed.TotalMilliseconds;
            OutputBox.Text = $"generated in {el} ms";
        }

        async Task tickFrameAsync()
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
        }
    }
}