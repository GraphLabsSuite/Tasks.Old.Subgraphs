using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Castle.Core.Internal;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;

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
            #region Добавление рёбер
            var phase1AddEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    IsEgesAddingEnabled = true;
                    _state = State.EdgesAdding;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.edgesCommandButtonOn);
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    IsEgesAddingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.edgesCommandButtonOff);
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Arrow.png")),
                Description = Strings.Strings_RU.edgesCommandButtonDisc
            };
            #endregion

            #region Завершить этап
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
                        UserActionsManager.RegisterInfo(Strings.Strings_RU.stage1Done);
                        GivenGraph = CurrentGraph;
                        CurrentGraph = new UndirectedGraph();

                        Phase1ToolBarVisibility = Visibility.Collapsed;
                        Phase2ToolBarVisibility = Visibility.Visible;
                        new HelpDialog(Strings.Strings_RU.help2).Show();
                    }
                    else UserActionsManager.RegisterMistake(Strings.Strings_RU.wrongComplement, 10);
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Check.png")),
                Description = Strings.Strings_RU.stage1DoneButtonDisc
            };
            #endregion

            #region Справка
            var phase1HelpCommand = new ToolBarInstantCommand(
                () => new HelpDialog(Strings.Strings_RU.help1).Show(),
                () => _state == State.Nothing
                )
            {
                Description = Strings.Strings_RU.helpButtonDisc,
                Image = new BitmapImage(GetImageUri("Info.png"))
            };
            #endregion
            #endregion

            #region Второй этап
            #region Добавление вершин
            var vertexDialogCommand = new ToolBarInstantCommand(
                () =>
                {
                    var dialog = new VertexDialog((UndirectedGraph) CurrentGraph, GivenGraph.Vertices);
                    dialog.Show();
                    dialog.Closed += (sender, args) =>
                    {
                        var buf = CurrentGraph;
                        CurrentGraph = null;
                        CurrentGraph = buf;
                    };
                },
                () => _state == State.Nothing)
            {
                Image = new BitmapImage(GetImageUri("Vertexes.png")),
                Description = Strings.Strings_RU.vertexDialogButton
            };
            #endregion

            #region Добавление рёбер
            var phase2AddEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    IsEgesAddingEnabled = true;
                    _state = State.EdgesAdding;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.edgesCommandButtonOn);
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    IsEgesAddingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.edgesCommandButtonOff);
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Arrow.png")),
                Description = Strings.Strings_RU.edgesCommandButtonDisc
            };
            #endregion

            #region Добавление подграфов
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
                        UserActionsManager.RegisterMistake(Strings.Strings_RU.wrongSubgraph, 10);
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
                        UserActionsManager.RegisterMistake(Strings.Strings_RU.subgraphAlreadyAdded, 10);
                    }

                    if (unique && subgraph && CurrentGraph.VerticesCount > 1 && !CurrentGraph.Equals(GivenGraph))
                    {
                        UserActionsManager.RegisterInfo(Strings.Strings_RU.subgraphAdded);
                        GraphLib.Lib.Add(CurrentGraph);
                        CurrentGraph = new UndirectedGraph();
                    }
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Check.png")),
                Description = Strings.Strings_RU.checkSubgraph
            };
            #endregion

            #region Справка
            var phase2HelpCommand = new ToolBarInstantCommand(
                () => new HelpDialog(Strings.Strings_RU.help2).Show(),
                () => _state == State.Nothing
                )
            {
                Description = Strings.Strings_RU.helpButtonDisc,
                Image = new BitmapImage(GetImageUri("Info.png"))
            };
            #endregion

            #region Молния
            var cheat = new ToolBarInstantCommand(
                () =>
                {
                    for ()
                    /*for (var i1 = 0; i1 < GivenGraph.VerticesCount; i1++)
                    {
                        var c1 = new List<Graphs.Vertex>();
                        GivenGraph.Vertices.ForEach(v => c1.Add(new Graphs.Vertex(v.Name)));
                        c1.RemoveAt(i1);
                        for (var i2 = i1; i2 < c1.Count; i2++)
                        {
                            var c2 = new List<Graphs.Vertex>();
                            c1.ForEach(v => c2.Add(new Graphs.Vertex(v.Name)));
                            c2.RemoveAt(i2);
                            for (var i3 = i2; i3 < c2.Count; i3++)
                            {
                                var c3 = new List<Graphs.Vertex>();
                                c2.ForEach(v => c3.Add(new Graphs.Vertex(v.Name)));
                                c3.RemoveAt(i3);
                                var g3 = new UndirectedGraph();
                                c3.ForEach(v => g3.AddVertex(new Graphs.Vertex(v.Name)));
                                g3.Vertices.ForEach(v1 =>
                                    g3.Vertices.ForEach(v2 =>
                                    {
                                        if (GivenGraph[GivenGraph.Vertices.Single(v => v.Name == v1.Name),
                                                       GivenGraph.Vertices.Single(v => v.Name == v2.Name)] != null && g3[v1, v2] == null)
                                            g3.AddEdge(new UndirectedEdge(v1, v2));
                                    }));
                                if (GraphLib.Lib.SingleOrDefault(g => g.Equals(g3)) == null)
                                GraphLib.Lib.Add(g3);
                            }
                            var g2 = new UndirectedGraph();
                            c2.ForEach(v => g2.AddVertex(new Graphs.Vertex(v.Name)));
                            g2.Vertices.ForEach(v1 =>
                                    g2.Vertices.ForEach(v2 =>
                                    {
                                        if (GivenGraph[GivenGraph.Vertices.Single(v => v.Name == v1.Name),
                                                       GivenGraph.Vertices.Single(v => v.Name == v2.Name)] != null && g2[v1, v2] == null)
                                            g2.AddEdge(new UndirectedEdge(v1, v2));
                                    }));
                            if (GraphLib.Lib.SingleOrDefault(g => g.Equals(g2)) == null)
                                GraphLib.Lib.Add(g2);
                        }
                        var g1 = new UndirectedGraph();
                        c1.ForEach(v => g1.AddVertex(new Graphs.Vertex(v.Name)));
                        g1.Vertices.ForEach(v1 =>
                                    g1.Vertices.ForEach(v2 =>
                                    {
                                        if (GivenGraph[GivenGraph.Vertices.Single(v => v.Name == v1.Name),
                                                       GivenGraph.Vertices.Single(v => v.Name == v2.Name)] != null && g1[v1, v2] == null)
                                            g1.AddEdge(new UndirectedEdge(v1, v2));
                                    }));
                        if (GraphLib.Lib.SingleOrDefault(g => g.Equals(g1)) == null)
                            GraphLib.Lib.Add(g1);
                    }*/
                },
                () => _state == State.Nothing
                )
            {
                Description = "Молния",
                Image = new BitmapImage(GetImageUri("thunder.png"))
            };
            #endregion

            #region Завершить этап
            var phase2Command = new ToolBarInstantCommand(
                () =>
                {
                    var sum = 0;
                    var n = GivenGraph.VerticesCount;
                    for (var i = 2; i < n; i++)
                        sum += F(n) / F(i) / F(n - i);
                    if (GraphLib.Lib.Count < sum)
                        UserActionsManager.RegisterMistake(Strings.Strings_RU.info1, 10);
                },
                () => _state == State.Nothing
                )
            {
                Description = Strings.Strings_RU.stage2DoneButtonDisc
            };
            #endregion
            #endregion

            Phase1ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                phase1AddEdgeCommand,
                phase1Command,
                phase1HelpCommand
            };
            Phase2ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                vertexDialogCommand,
                phase2AddEdgeCommand,
                subgraphCommand,
                phase2Command,
                phase2HelpCommand,
                cheat
            };
            Phase3ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                
            };
        }
    }
}
