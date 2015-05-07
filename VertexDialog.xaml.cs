using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GraphLabs.Graphs;
using GraphLabs.Utils;

namespace GraphLabs.Tasks.Template
{
    public partial class VertexDialog
    {
        private UndirectedGraph _graph;

        public VertexDialog(UndirectedGraph currentGraph, ReadOnlyCollection<IVertex> v2)
        {
            InitializeComponent();

            Title = Strings.Strings_RU.vertexDialogTitle;
            OkButton.Content = Strings.Strings_RU.okButton;
            CancelButton.Content = Strings.Strings_RU.cancelButton;
            Info.Text = Strings.Strings_RU.vertexDialogInfo;

            v2.ForEach(v =>
            {
                _graph = currentGraph;
                var cb = new CheckBox {Content = Strings.Strings_RU.vertex + " [" + v.ToString() + "]"};
                if (currentGraph.Vertices.SingleOrDefault(s => s.Equals(v)) != null)
                    cb.IsChecked = true;
                VertexList.Children.Add(cb);
            });
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            VertexList.Children.ForEach(ch =>
            {
                var cb = ch as CheckBox;
                var name = cb.Content.ToString().Substring(9);
                name = name.Substring(0, name.Length - 1);
                if (_graph.Vertices.SingleOrDefault(v => v.Name == name) == null && cb.IsChecked == true)
                    _graph.AddVertex(new Vertex(name));
                if (_graph.Vertices.SingleOrDefault(v => v.Name == name) != null && cb.IsChecked == false)
                    _graph.RemoveVertex(_graph.Vertices.Single(v => v.Name == name));
            });
            DialogResult = true;
        }
    }
}
