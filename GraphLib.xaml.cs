using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphLabs.Graphs;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    public partial class GraphLib : UserControl
    {
        public static ObservableCollection<IGraph> Lib;
        public static ObservableCollection<IGraph> SubgraphLib;

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
                var label = new Label();
                label.Content = Lib.Count + ")";
                label.VerticalAlignment = VerticalAlignment.Top;
                StackPanel.Children.Add(label);
                label.UpdateLayout();
                label.Margin = new Thickness(0, 0, -label.ActualWidth, 0);

                var gv = new GraphVisualizer();
                gv.Height = 100;
                gv.Width = 100;
                gv.HorizontalAlignment = HorizontalAlignment.Left;
                gv.VisualizationAlgorithm = VisualizationAlgorithm.Circle;
                gv.Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                gv.DefaultVertexRadius = 10;
                StackPanel.Children.Add(gv);
                gv.UpdateLayout();
                gv.Graph = (UndirectedGraph)g;
            }
        }

        public void InitSubgraphLib()
        {

        }
    }
}
