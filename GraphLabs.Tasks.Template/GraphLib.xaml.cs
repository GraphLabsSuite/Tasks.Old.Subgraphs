using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphLabs.Graphs;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    public partial class GraphLib : UserControl
    {
        public static ObservableCollection<IGraph> Lib;

        public GraphLib()
        {
            InitializeComponent();
            Lib = new ObservableCollection<IGraph>();
            Lib.CollectionChanged += UpdateLibrary;
        }

        public void UpdateLibrary(object o, NotifyCollectionChangedEventArgs args)
        {
            foreach (var g in args.NewItems)
            {
                if (Lib.Count > 1)
                {
                    StackPanel.Children.Add(new Rectangle
                    {
                        Stroke = new SolidColorBrush(Colors.Transparent),
                        Fill = new SolidColorBrush(Color.FromArgb(255, 112, 128, 144)),
                        Margin = new Thickness(5, 0, 5, 0),
                        Width = 1
                    });
                }
                var label = new Label
                {
                    Content = Lib.Count + ")",
                    VerticalAlignment = VerticalAlignment.Top
                };
                StackPanel.Children.Add(label);
                label.UpdateLayout();
                label.Margin = new Thickness(0, 0, -label.ActualWidth, 0);

                var gv = new GraphVisualizer
                {
                    Width = StackPanel.ActualHeight,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VisualizationAlgorithm = VisualizationAlgorithm.Circle,
                    Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    DefaultVertexRadius = 10
                };
                StackPanel.Children.Add(gv);
                gv.UpdateLayout();
                gv.Graph = (UndirectedGraph)g;
            }
        }
    }
}
