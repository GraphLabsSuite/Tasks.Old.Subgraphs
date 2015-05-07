using GraphLabs.Common;
using GraphLabs.Common.TasksDataService;
using GraphLabs.CommonUI.Configuration;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;

namespace GraphLabs.Tasks.Template.Configuration
{
    /// <summary> Конфигуратор заглушек wcf-сервисов </summary>
    public class MockedWcfServicesConfigurator : MockedWcfServicesConfiguratorBase
    {
        /// <summary> Сгенерировать отладочный вариант </summary>
        protected override TaskVariantInfo GetDebugVariant()
        {
            var debugGraph = UndirectedGraph.CreateEmpty(5);
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[2]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[3]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[3]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[4]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[4], debugGraph.Vertices[1]));
            var serializedGraph = GraphSerializer.Serialize(debugGraph);

            return new TaskVariantInfo
            {
                Data = serializedGraph,
                GeneratorVersion = "1.0",
                Number = "Debug",
                Version = 1
            };
        }
    }
}