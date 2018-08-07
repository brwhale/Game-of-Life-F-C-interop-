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

namespace FunctionsinWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int stride = 45;
        bool running = false;
        private void mouseHandler(object o, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var grid = (Grid)o;
                var pos = e.GetPosition(grid);
                var x = (int)(pos.X * stride / grid.ActualWidth);
                var y = (int)(pos.Y * stride / grid.ActualHeight);
                var index = y * stride + x;
                if (index < grid.Children.Count)
                    ((CheckBox)grid.Children[index]).IsChecked = true;
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
                    var box = new CheckBox() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, ClickMode = ClickMode.Press, IsHitTestVisible = false };

                    ChexGrid.Children.Add(box);
                    Grid.SetRow(box, i);
                    Grid.SetColumn(box, j);
                }
            }
            ChexGrid.MouseMove += mouseHandler;
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

        void tickFrame()
        {
            var list = new List<bool>();
            foreach (CheckBox p in ChexGrid.Children)
            {
                list.Add(p.IsChecked ?? false);
            }
            var ne = Life.lifeGameTick(ListModule.OfSeq(list), stride);
            int index = 0;
            foreach (CheckBox p in ChexGrid.Children)
            {
                p.IsChecked = ne[index++];
            }
        }

        async Task tickFrameAsync()
        {
            if (Application.Current == null) return;
            var list = new List<bool>();
            Application.Current.Dispatcher.Invoke((() => {
                foreach (CheckBox p in ChexGrid.Children)
                {
                    list.Add(p.IsChecked ?? false);
                }
            }));
            var ne = Life.lifeGameTick(ListModule.OfSeq(list), stride);
            Application.Current.Dispatcher.Invoke((() => {
                int index = 0;
                foreach (CheckBox p in ChexGrid.Children)
                {
                    p.IsChecked = ne[index++];
                }
            }));
            await Task.Delay(16);
            if (running)
            {
                await tickFrameAsync();
            }
        }

        private void toggleLife(object sender, RoutedEventArgs e)
        {
            if (running) { running = false; }
            else
            {
                running = true;
                Task.Run(()=>tickFrameAsync());
            }
        }

        private void frameButton(object sender, RoutedEventArgs e)
        {
            tickFrame();
        }
    }
}
