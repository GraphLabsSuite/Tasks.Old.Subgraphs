using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Castle.Core.Internal;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    public partial class TaskTemplateViewModel
    {
        private const string ImageResourcesPath = @"/GraphLabs.Tasks.Template;component/Images/";

        private Uri GetImageUri(string imageFileName)
        {
            return new Uri(ImageResourcesPath + imageFileName, UriKind.Relative);
        }

        private int F (int n)
        {
            return (n > 1) ? F(n - 1) * n : 1;
        }

        private void InitToolBarCommands()
        {
            #region Первый этап
            var phase1AddEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    IsEgesAddingEnabled = true;
                    _state = State.EdgesAdding;
                    UserActionsManager.RegisterInfo("Включено добавление рёбер.");
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    IsEgesAddingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo("Отключено добавление рёбер.");
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Arrow.png")),
                Description = "Добавление рёбер"
            };

            var phase1Command = new ToolBarInstantCommand(
                () =>
                {
                    var solve = true;
                    CurrentGraph.Vertices.ForEach(v1 =>
                        CurrentGraph.Vertices.ForEach(v2 =>
                            solve = solve && (v1.Equals(v2) || (CurrentGraph[v1, v2] == null ^
                                              GivenGraph[
                                                  GivenGraph.Vertices.Single(v1.Equals),
                                                  GivenGraph.Vertices.Single(v2.Equals)] == null
                            ))
                    ));
                    if (solve)
                    {
                        UserActionsManager.RegisterInfo("Верный антиграф, продолжение...");
                        GivenGraph = CurrentGraph;
                        CurrentGraph = new UndirectedGraph();
                        UserActionsManager.RegisterInfo("Теперь необходимо построить все возможные подграфы полученного дополнения");
                        while (CurrentGraph.EdgesCount > 0) CurrentGraph.RemoveEdge(CurrentGraph.Edges[0]); //del

                        Phase1ToolBarVisibility = Visibility.Collapsed;
                        Phase2ToolBarVisibility = Visibility.Visible;
                    }
                    else UserActionsManager.RegisterMistake("Неверный антиграф", 10);
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Check.png")),
                Description = "Проверить решение"
            };
            #endregion

            #region Второй этап

            var vertexDialogCommand = new ToolBarInstantCommand(
                () =>
                {
                    var dialog = new VertexDialog((UndirectedGraph) CurrentGraph, GivenGraph.Vertices);
                    dialog.Show();
                },
                () => _state == State.Nothing)
            {
                Image = new BitmapImage(GetImageUri("Vertexes.png")),
                Description = "Изменить набор вершин"
            };

            var phase2AddEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    IsEgesAddingEnabled = true;
                    _state = State.EdgesAdding;
                    UserActionsManager.RegisterInfo("Включено добавление рёбер.");
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    IsEgesAddingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo("Отключено добавление рёбер.");
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Arrow.png")),
                Description = "Добавление рёбер"
            };

            var subgraphCommand = new ToolBarInstantCommand(
                () =>
                {
                    var subgraph = true;
                    var unique = true;

                    // проверки на подграфовость и неповторимость
                    CurrentGraph.Vertices.ForEach(v1 =>
                        CurrentGraph.Vertices.ForEach(v2 =>
                            subgraph &= v1.Equals(v2) || (CurrentGraph[v1, v2] == null ^ GivenGraph[
                                                                        GivenGraph.Vertices.Single(v1.Equals),
                                                                        GivenGraph.Vertices.Single(v2.Equals)] != null)
                    ));
                    if (!subgraph)
                    {
                        UserActionsManager.RegisterMistake("Неверный подграф", 10);
                    }

                    /*
                    // удаление одиночных вершин
                    var sub = CurrentGraph;
                    CurrentGraph = UndirectedGraph.CreateEmpty(CurrentGraph.VerticesCount);
                    var vs = sub.Vertices.ToList();
                    sub.Vertices.ForEach(v =>
                        sub.Edges.ForEach(e =>
                        {
                            if (vs.Contains(v) && (e.Vertex1.Equals(v) || e.Vertex2.Equals(v)))
                                vs.Remove(v);
                        })
                    );
                    vs.ForEach(v => sub.RemoveVertex(v));*/

                    GraphLib.Lib.ForEach(g => unique &= !g.Equals(CurrentGraph));
                    if (!unique)
                    {
                        UserActionsManager.RegisterMistake("Подграф уже имеется в коллекции", 10);
                    }

                    if (unique && subgraph && CurrentGraph.VerticesCount > 1 && !CurrentGraph.Equals(GivenGraph))
                    {
                        UserActionsManager.RegisterInfo("Новый подграф добавлен в коллекцию");
                        GraphLib.Lib.Add(CurrentGraph);
                        CurrentGraph = new UndirectedGraph();
                    }
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Check.png")),
                Description = "Проверить решение"
            };

            var cheat = new ToolBarInstantCommand(
                () =>
                {
                    for (var i1 = GivenGraph.VerticesCount - 1; i1 >= 0; i1--)
                    {
                        var c1 = GivenGraph.Vertices.ToList();
                        c1.RemoveAt(i1);
                        for (var i2 = i1 - 1; i2 >= 0; i2--)
                        {
                            var c2 = c1;
                            c2.RemoveAt(i2);
                            for (var i3 = i2 - 1; i3 >= 0; i3--)
                            {
                                var c3 = c2;
                                c3.RemoveAt(i3);
                                var g3 = new UndirectedGraph();
                                c3.ForEach(v => g3.AddVertex(new Graphs.Vertex(v.Name)));
                                GraphLib.Lib.Add(g3);
                            }
                            var g2 = new UndirectedGraph();
                            c2.ForEach(v => g2.AddVertex(new Graphs.Vertex(v.Name)));
                            GraphLib.Lib.Add(g2);
                        }
                        var g1 = new UndirectedGraph();
                        c1.ForEach(v => g1.AddVertex(new Graphs.Vertex(v.Name)));
                        GraphLib.Lib.Add(g1);
                    }
                },
                () => _state == State.Nothing
                )
            {
                Description = "Добавить подграф в коллекцию"
            };

            var phase2Command = new ToolBarInstantCommand(
                () =>
                {
                    var sum = 0;
                    var n = GivenGraph.VerticesCount;
                    for (var i = 0; i < n; i++)
                        sum += F(n) / F(i) / F(n - i);
                    if (GraphLib.Lib.Count < sum)
                        UserActionsManager.RegisterMistake("Имеются неучтённые подграфы", 10);
                },
                () => _state == State.Nothing
                );
            #endregion

            Phase1ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                phase1AddEdgeCommand,
                phase1Command
            };
            Phase2ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                vertexDialogCommand,
                phase2AddEdgeCommand,
                subgraphCommand,
                phase2Command,
                cheat
            };
            Phase3ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                
            };
        }
    }
}
