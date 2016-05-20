using GraphLabs.Common.VariantProviderService;
using GraphLabs.CommonUI.Configuration;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;

namespace GraphLabs.Tasks.Subgraphs.Configuration
{
    /// <summary> Конфигуратор заглушек wcf-сервисов </summary>
    public class MockedWcfServicesConfigurator : MockedWcfServicesConfiguratorBase
    {
        /// <summary> Сгенерировать отладочный вариант </summary>
        protected override TaskVariantDto GetDebugVariant()
        {
            var debugGraph = UndirectedGraph.CreateEmpty(7);
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[1]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[6]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[2]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[0], debugGraph.Vertices[3]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[2]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[4]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[1], debugGraph.Vertices[6]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[3]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[2], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[3], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[3], debugGraph.Vertices[6]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[4], debugGraph.Vertices[5]));
            debugGraph.AddEdge(new UndirectedEdge(debugGraph.Vertices[4], debugGraph.Vertices[6]));
            var serializedVariant = VariantSerializer.Serialize(new IGraph[]
            {
                debugGraph
            });

            return new TaskVariantDto
            {
                Data = serializedVariant,
                GeneratorVersion = "1.0",
                Number = "Debug",
                Version = 1
            };
        }
    }
}