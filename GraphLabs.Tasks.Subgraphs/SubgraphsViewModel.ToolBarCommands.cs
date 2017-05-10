using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Castle.Components.DictionaryAdapter;
using GraphLabs.Common;
using GraphLabs.CommonUI.Controls;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Tasks.Subgraphs.Strings;
using GraphLabs.Graphs;
using GraphLabs.Utils;

namespace GraphLabs.Tasks.Subgraphs
{
    public partial class SubgraphsViewModel
    {
        
        #region Полезности

        private const string ImageResourcesPath = @"/GraphLabs.Tasks.Subgraphs;component/Images/";

        private Uri GetImageUri(string imageFileName)
        {
            return new Uri(ImageResourcesPath + imageFileName, UriKind.Relative);
        }

        private static UndirectedGraph CombUndGraph(IGraph g, int n)
        {
            var graph = (UndirectedGraph)((UndirectedGraph)g).Clone();
            var list = new List<String>();
            var order = new Stack<int>();
            g.Vertices.ForEach(v => list.Add(v.Name));
            for (var i = 1; i <= graph.VerticesCount; i++)
            {
                order.Push(n % i);
                n /= i;
            }
            for (var i = 0; i < graph.VerticesCount; i++)
            {
                var j = order.Pop();
                graph.Vertices[i].Rename(list[j]);
                list.RemoveAt(j);
            }
            return graph;
        }

        private static int F(int n)
        {
            return n > 1 ? F(n - 1)*n : 1;
        }

        private static void FindAllSubgraphs(IGraph graph, int iStart, List<Graphs.IVertex> cStart, ICollection<IGraph> collection)
        {
            for (var i = iStart; i < cStart.Count; i++)
            {
                var c = new List<Graphs.IVertex>();
                cStart.ForEach(v => c.Add(new Graphs.Vertex(v.Name)));
                c.RemoveAt(i);

                FindAllSubgraphs(graph, i, c, collection);

                var g = new UndirectedGraph();
                c.ForEach(v => g.AddVertex(new Graphs.Vertex(v.Name)));
                g.Vertices.ForEach(v1 =>
                        g.Vertices.ForEach(v2 =>
                        {
                            if (graph[graph.Vertices.Single(v => v.Name == v1.Name),
                                      graph.Vertices.Single(v => v.Name == v2.Name)] != null && g[v1, v2] == null)
                                g.AddEdge(new UndirectedEdge(v1, v2));
                        }));
                if (GraphLib.Lib.SingleOrDefault(s => s.Equals(g)) == null && Unique(g, collection) &&
                    g.VerticesCount > 0)
                {
                    g.Vertices.ForEach(v => v.Rename(v.Name + '.'));
                    for (var j = 0; j < g.VerticesCount; j++)
                        g.Vertices[j].Rename(j.ToString());
                    collection.Add(g);
                }
            }
        }

        private static bool Unique(IGraph g, IEnumerable<IGraph> c)
        {
            var unique = true;

            var gCopy = UndirectedGraph.CreateEmpty(g.VerticesCount);
            for (var i = 0; i < g.VerticesCount; i++)
                for (var j = i + 1; j < g.VerticesCount; j++)
                    if (g[g.Vertices[i], g.Vertices[j]] != null)
                        gCopy.AddEdge(new UndirectedEdge(gCopy.Vertices[i], gCopy.Vertices[j]));

            var graphs = new ObservableCollection<IGraph>();
            var n = F(gCopy.VerticesCount);
            for (var i = 0; i < n; i++)
                graphs.Add(CombUndGraph(gCopy, i));
            graphs.ForEach(g1 =>
                c.ForEach(g2 =>
                    unique &= !g1.Equals(g2)
                    )
                );

            return unique;
        }

        #endregion

        private void InitToolBarCommands()
        {
            #region Первый этап
            #region Добавление рёбер
            var phase1AddEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    // добавление ребер включено
                    IsMouseVerticesMovingEnabled = false;
                    IsEgesAddingEnabled = true;
                    _state = State.EdgesAdding;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.buttonEdgesOn);
                },
                () =>
                {
                    // добавление ребер отключено
                    IsMouseVerticesMovingEnabled = true;
                    IsEgesAddingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.buttonEdgesOff);
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Arrow.png")),
                Description = Strings.Strings_RU.buttonEdges
            };
            #endregion

            #region Завершить этап
            var allSubgraphs = new ObservableCollection<IGraph>();

            var phase1Command = new ToolBarInstantCommand(
                () =>
                {
                    var solve = true;
                    //var GP = new GraphLabs.Graphs.
                    // stage1answer - содержит информацию о ребрах графа, отправляемого на проверку
                    var stage1answer = "";
                    // добавляем все ребра
                    CurrentGraph.Edges.ForEach(edge =>
                    {
                        stage1answer += "(" + edge.Vertex1.ToString() + "; " + edge.Vertex2.ToString() + "), ";
                    });

                    // информация об отправленном на проверку графе
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.stage1Check);
                    CurrentGraph.Vertices.ForEach(v1 =>
                        CurrentGraph.Vertices.ForEach(v2 =>
                            {
                                solve = solve && (v1.Equals(v2) || (CurrentGraph[v1, v2] == null ^
                                                                    GivenGraph[
                                                                        GivenGraph.Vertices.Single(v1.Equals),
                                                                        GivenGraph.Vertices.Single(v2.Equals)] == null
                            ));
                            }
                        ));
                    if (solve)
                    {
                        // информация о завершении этапа
                        UserActionsManager.RegisterInfo(Strings.Strings_RU.stage1Done);
                        GivenGraph = CurrentGraph;
                        CurrentGraph = new UndirectedGraph();

                        Phase1ToolBarVisibility = Visibility.Collapsed;
                        Phase2ToolBarVisibility = Visibility.Visible;
                        L1 = Strings.Strings_RU.subgraph;

                        FindAllSubgraphs(GivenGraph, 0, GivenGraph.Vertices.ToList(), allSubgraphs);

                        new SimpleDialog("Справка", Strings.Strings_RU.stage2Help).Show();
                    }
                    else UserActionsManager.RegisterMistake(Strings.Strings_RU.stage1Mistake1, 10);
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
                () =>
                {
                    // вызов справки
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.stage1HelpCall);
                    new SimpleDialog("Справка", Strings.Strings_RU.stage1Help).Show();
                },
                () => _state == State.Nothing
                )
            {
                Description = Strings.Strings_RU.buttonHelp,
                Image = new BitmapImage(GetImageUri("Info.png"))
            };
            #endregion

            #region Молния
            var thunderbolt1 = new ToolBarInstantCommand(
                () =>
                {
                    // вызов молнии
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.stage1ThunderCall);
                    CurrentGraph.Vertices.ForEach(v1 =>
                        CurrentGraph.Vertices.ForEach(v2 =>
                        {
                            if (!v1.Equals(v2) && CurrentGraph[v1, v2] == null && GivenGraph[
                                                  GivenGraph.Vertices.Single(v1.Equals),
                                                  GivenGraph.Vertices.Single(v2.Equals)] == null)
                                CurrentGraph.AddEdge(new UndirectedEdge((Vertex)v1, (Vertex)v2));
                            if (!v1.Equals(v2) && CurrentGraph[v1, v2] != null && GivenGraph[
                                                  GivenGraph.Vertices.Single(v1.Equals),
                                                  GivenGraph.Vertices.Single(v2.Equals)] != null)
                                CurrentGraph.RemoveEdge(CurrentGraph[v1, v2]);
                        }
                    ));
                },
                () => _state == State.Nothing
                )
            {
                Description = "Молния",
                Image = new BitmapImage(GetImageUri("thunder.png"))
            };
            #endregion
            #endregion

            #region Второй этап
            #region Добавление вершин

            var vertexDialogCommand = new ToolBarInstantCommand(
                () =>
                {
                    var dialog = new VertexDialog((UndirectedGraph)CurrentGraph, GivenGraph.Vertices);
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
                Description = Strings.Strings_RU.buttonVertexDialog
            };
            #endregion

            #region Добавление рёбер
            var phase2AddEdgeCommand = new ToolBarToggleCommand(
                () =>
                {
                    IsMouseVerticesMovingEnabled = false;
                    IsEgesAddingEnabled = true;
                    _state = State.EdgesAdding;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.buttonEdgesOn);
                },
                () =>
                {
                    IsMouseVerticesMovingEnabled = true;
                    IsEgesAddingEnabled = false;
                    _state = State.Nothing;
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.buttonEdgesOff);
                },
                () => _state == State.Nothing,
                () => true
                )
            {
                Image = new BitmapImage(GetImageUri("Arrow.png")),
                Description = Strings.Strings_RU.buttonEdges
            };
            #endregion

            #region Добавление подграфов
            var subgraphCommand = new ToolBarInstantCommand(
                () =>
                {
                    var subgraph = true;
                    var unique = Unique((UndirectedGraph)CurrentGraph, GraphLib.Lib);
                    var subVertexes = ""; //строка, содержащая вершины подграфа
                    var subEdges = ""; //строка, содержащая ребра подграфа
                    CurrentGraph.Edges.ForEach(edge =>
                    {
                        subEdges += "(" + edge.Vertex1.ToString() + "; " + edge.Vertex2.ToString() + "), ";
                    });
                    CurrentGraph.Vertices.ForEach(v1 =>
                    {
                        subVertexes += v1.ToString() + "; ";
                        CurrentGraph.Vertices.ForEach(v2 =>
                        {
                            subgraph &= v1.Equals(v2) || (CurrentGraph[v1, v2] == null ^ GivenGraph[
                                                                    GivenGraph.Vertices.Single(v1.Equals),
                                                                    GivenGraph.Vertices.Single(v2.Equals)] != null);
                        }
                        );
                    }
                    );
                    // попытка добавить пустой граф
                    if (CurrentGraph.VerticesCount == 0) return;
                    // в зависимости от наличия ребер в добавляемом подграфе
                    if (subEdges.Length > 2)
                        UserActionsManager.RegisterInfo(Strings.Strings_RU.stage2Check + "({" + subVertexes.Remove(subVertexes.Length - 2) + "}, {" + subEdges.Remove(subEdges.Length - 2) + "})");
                    else
                        UserActionsManager.RegisterInfo(Strings.Strings_RU.stage2Check + "({" + subVertexes.Remove(subVertexes.Length - 2) + "}, {пустое множество})");
                    if (!subgraph)
                    {
                        UserActionsManager.RegisterMistake(Strings.Strings_RU.stage2Mistake1, 10);
                        return;
                    }

                    if (!unique)
                    {
                        UserActionsManager.RegisterMistake(Strings.Strings_RU.stage2Mistake2, 10);
                        return;
                    }

                    var newGraph = UndirectedGraph.CreateEmpty(CurrentGraph.VerticesCount);
                    for (var i = 0; i < CurrentGraph.VerticesCount; i++)
                        for (var j = i + 1; j < CurrentGraph.VerticesCount; j++)
                            if (CurrentGraph[CurrentGraph.Vertices[i], CurrentGraph.Vertices[j]] != null)
                            {
                                newGraph.AddEdge(new UndirectedEdge(newGraph.Vertices[i], newGraph.Vertices[j]));
                            }
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.stage2Subgraph);

                    GraphLib.Lib.Add(newGraph);
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Collection.png")),
                Description = Strings.Strings_RU.buttonCheckSubgraph
            };
            #endregion

            #region Справка
            var phase2HelpCommand = new ToolBarInstantCommand(
                () =>
                {
                    // вызов справки
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.stage2HelpCall);
                    new SimpleDialog("Справка", Strings.Strings_RU.stage2Help).Show();
                },
                () => _state == State.Nothing
                )
            {
                Description = Strings.Strings_RU.buttonHelp,
                Image = new BitmapImage(GetImageUri("Info.png"))
            };
            #endregion

            #region Молния
            var thunderbolt2 = new ToolBarInstantCommand(
                () =>
                {
                    // вызов молнии
                    UserActionsManager.RegisterInfo(Strings.Strings_RU.stage2ThunderCall);
                    allSubgraphs.ForEach(s =>
                    {
                        if (Unique(s, GraphLib.Lib))
                            GraphLib.Lib.Add(s);
                    });
                    var g = UndirectedGraph.CreateEmpty(GivenGraph.VerticesCount);
                    for (var i = 0; i < g.VerticesCount; i++)
                        for (var j = i + 1; j < g.VerticesCount; j++)
                            if (GivenGraph[GivenGraph.Vertices[i], GivenGraph.Vertices[j]] != null)
                                g.AddEdge(new UndirectedEdge(g.Vertices[i], g.Vertices[j]));
                    GraphLib.Lib.Add(g);
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
                    if (GraphLib.Lib.Count > allSubgraphs.Count)
                    {
                        UserActionsManager.RegisterInfo(Strings.Strings_RU.stage2Done);
                        UserActionsManager.ReportThatTaskFinished();
                    }
                    else
                    {
                        UserActionsManager.RegisterMistake(Strings.Strings_RU.stage2Mistake3, 10);
                    }
                },
                () => _state == State.Nothing
                )
            {
                Image = new BitmapImage(GetImageUri("Check.png")),
                Description = Strings.Strings_RU.stage2DoneButtonDisc
            };
            #endregion
            #endregion

            Phase1ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                phase1AddEdgeCommand,
                phase1Command,
                phase1HelpCommand
                #if DEBUG
                ,
                thunderbolt1
                #endif
            };
            Phase2ToolBarCommands = new ObservableCollection<ToolBarCommandBase>
            {
                vertexDialogCommand,
                phase2AddEdgeCommand,
                subgraphCommand,
                phase2Command,
                phase2HelpCommand
                #if DEBUG
                ,
                thunderbolt2
                #endif
            };
        }
    }
}
