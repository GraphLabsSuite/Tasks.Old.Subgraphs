using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphLabs.Graphs;

/// <summary> Комбинация визуализаторов для изоморфизма </summary>

namespace GraphLabs.Tasks.Template
{
    public partial class IsomorphismVisualizer : UserControl
    {
        /// <summary> Визуализатор для изоморфизма </summary>
        public IsomorphismVisualizer()
        {
            InitializeComponent();
            WorkspaceVisualizer.SetParent(this);
            BackgroundVisualizer.SetParent(this);
        }

        /// <summary> Рабочий граф </summary>
        public static DependencyProperty WorkspaceGraphProperty =
            DependencyProperty.Register(
                "WorkspaceGraph",
                typeof(IObservableGraph),
                typeof(IsomorphismVisualizer),
                new PropertyMetadata(WorkspaceGraphChanged));

        /// <summary> Видимость рабочего графа </summary>
        public static DependencyProperty WorkspaceGraphVisibilityProperty = 
            DependencyProperty.Register(
                "WorkspaceGraphVisibility",
                typeof(Visibility),
                typeof(IsomorphismVisualizer),
                new PropertyMetadata(default(Visibility)));

        public Visibility WorkspaceGraphVisibility
        {
            get
            {
                return (Visibility)GetValue(VisibilityProperty);
            }
            set
            {
                SetValue(VisibilityProperty, value);
                WorkspaceVisualizer.Visibility = value;
            }
        }

        /// <summary> Рабочий граф </summary>
        public IObservableGraph WorkspaceGraph
        {
            get { return (IObservableGraph)GetValue(WorkspaceGraphProperty); }
            set
            {
                UpdateLayout();
                SetValue(WorkspaceGraphProperty, value);
            }
        }

        private static void WorkspaceGraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                ((IsomorphismVisualizer)d).WorkspaceVisualizer.Graph = (IObservableGraph)e.NewValue;
            }
        }

        /// <summary> Граф-прототип </summary>
        public static DependencyProperty BackgroundGraphProperty =
            DependencyProperty.Register(
                "BackgroundGraph",
                typeof(IObservableGraph),
                typeof(IsomorphismVisualizer),
                new PropertyMetadata(BackgroundGraphChanged));

        /// <summary> Граф на бэкграунде </summary>
        public IObservableGraph BackgroundGraph
        {
            get { return (IObservableGraph)GetValue(BackgroundGraphProperty); }
            set
            {
                UpdateLayout();
                SetValue(BackgroundGraphProperty, value);
            }
        }

        private static void BackgroundGraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            var i = d as IsomorphismVisualizer;
            i.BackgroundVisualizer.Graph = (IObservableGraph)e.NewValue;
            i.WorkspaceVisualizer.Glue = i.BackgroundVisualizer.GetVertexesCoordinates();
            i.UpdateLayout();
        }

        /// <summary> Проверка решения </summary>
        public bool Check()
        {
            if (BackgroundVisualizer.Graph == null ||
                    WorkspaceVisualizer.Graph == null ||
                    BackgroundVisualizer.Graph.VerticesCount != WorkspaceVisualizer.Graph.VerticesCount ||
                    BackgroundVisualizer.Graph.EdgesCount != WorkspaceVisualizer.Graph.EdgesCount)
                return false;
            var result = true;
            var vertexesOrder = new ObservableCollection<Vertex>();
            var bgPoints = BackgroundVisualizer.GetVertexesCoordinates();
            var wsPoints = WorkspaceVisualizer.GetVertexesCoordinates();
            var bgGraph = BackgroundVisualizer.Graph;
            var wsGraph = WorkspaceVisualizer.Graph;
            for (var i = 0; i < bgGraph.VerticesCount; i++)
            {
                var point = wsPoints.SingleOrDefault(p => p.X == bgPoints[i].X && p.Y == bgPoints[i].Y);
                if (point == default(Point))
                    return false;
                var index = wsPoints.IndexOf(point);
                vertexesOrder.Add((Vertex)wsGraph.Vertices[index]);
            }
            for (var i = 0; i < bgGraph.VerticesCount; i++)
                for (var j = 0; j < bgGraph.VerticesCount; j++)
                    if (i != j)
                        result &= bgGraph[bgGraph.Vertices[i], bgGraph.Vertices[j]] != null ||
                                  wsGraph[vertexesOrder[i], vertexesOrder[j]] == null;
            return result;
        }
    }
}