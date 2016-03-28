﻿using System;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    public partial class TaskTemplate
    {
        /// <summary> Ctor. </summary>
        public TaskTemplate()
        {
            InitializeComponent();
        }

        /// <summary> Клик по вершине </summary>
        public event EventHandler<VertexClickEventArgs> VertexClicked;

        private void OnVertexClicked(VertexClickEventArgs e)
        {
            var handler = VertexClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnVertexClick(object sender, VertexClickEventArgs e)
        {
            OnVertexClicked(e);
        }

        private void Visualizer_Left_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new HelpDialog(Strings.Strings_RU.stage1Help).Show();
        }
    }
}
