using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GraphLabs.Graphs.UIComponents.Visualization;
using GraphLabs.Utils;

/// <summary> Визуализатор одного графа для изоморфизма </summary>

namespace GraphLabs.Tasks.Template
{
    public class IsomorphismGraphsVisualizer : GraphVisualizer
    {
        private IsomorphismVisualizer _parent;

        /// <summary> Залипающий граф </summary>
        public ObservableCollection<Point> Glue { get; set; }

        /// <summary> Оверрайд для перемещения вершин мышкой. Нужен для залипания </summary>
        protected override void MouseMoveVertex(object sender, MouseEventArgs mouseEventArgs)
        {
            base.MouseMoveVertex(sender, mouseEventArgs);
            if (Glue == null) return;
            var trap = default(Point);
            foreach (var t in Glue.Where(t =>
                (CapturedVertex.X - t.X) * (CapturedVertex.X - t.X) +
                (CapturedVertex.Y - t.Y) * (CapturedVertex.Y - t.Y) <
                3.9 * DefaultVertexRadius * DefaultVertexRadius))
            {
                trap = t;
            }
            if (trap != default(Point))
            {
                CapturedVertex.X = trap.X;
                CapturedVertex.Y = trap.Y;
                CapturedVertex.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                CapturedVertex.Background = DefaultVertexBackground;
            }
        }

        /// <summary> "Отпустить" перемещаемую вершину </summary>
        protected override void ReleaseVertex(object sender, MouseButtonEventArgs e)
        {
            base.ReleaseVertex(sender, e);
            Graph.Vertices[0].Rename(_parent.Check().ToString());
        }

        /// <summary> Установить родительский контейнер </summary>
        public void SetParent(IsomorphismVisualizer p)
        {
            _parent = p;
        }

        /// <summary> Коллекция координат вершин графа </summary>
        public ObservableCollection<Point> GetVertexesCoordinates()
        {
            var collection = new ObservableCollection<Point>();
            Vertices.ForEach(v => collection.Add(new Point(v.X, v.Y)));
            return collection;
        }
    }
}