using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using GraphLabs.Common;
using GraphLabs.Common.Utils;
using GraphLabs.CommonUI;
using GraphLabs.CommonUI.Controls.ViewModels;
using GraphLabs.Graphs;
using GraphLabs.Graphs.DataTransferObjects.Converters;
using GraphLabs.Graphs.UIComponents.Visualization;

namespace GraphLabs.Tasks.Template
{
    /// <summary> ViewModel для TaskTemplate </summary>
    public partial class TaskTemplateViewModel : TaskViewModelBase<TaskTemplate>
    {
        /// <summary> Текущее состояние </summary>
        private enum State
        {
            /// <summary> Пусто </summary>
            Nothing,
            /// <summary> Добавление рёбер </summary>
            EdgesAdding
        }

        /// <summary> Текущее состояние </summary>
        private State _state;

        /// <summary> Допустимые версии генератора, с помощью которого сгенерирован вариант </summary>
        private readonly Version[] _allowedGeneratorVersions = {  new Version(1, 0) };
        
        /// <summary> Допустимые версии генератора </summary>
        protected override Version[] AllowedGeneratorVersions
        {
            get { return _allowedGeneratorVersions; }
        }


        #region Public свойства вьюмодели

        /// <summary> Идёт загрузка данных? </summary>
        public static readonly DependencyProperty IsLoadingDataProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsLoadingData), 
            typeof(bool), 
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(false));

        /// <summary> Разрешено перемещение вершин? </summary>
        public static readonly DependencyProperty IsMouseVerticesMovingEnabledProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsMouseVerticesMovingEnabled),
            typeof(bool),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(true));

        /// <summary> Разрешено добавление рёбер? </summary>
        public static readonly DependencyProperty IsEdgesAddingEnabledProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.IsEgesAddingEnabled),
            typeof(bool),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(false));

        /// <summary> Команды панели инструментов</summary>
        public static readonly DependencyProperty Phase1ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Phase1ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));
        public static readonly DependencyProperty Phase2ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Phase2ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));
        public static readonly DependencyProperty Phase3ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Phase3ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));

        /// <summary> Видимость тулбаров </summary>
        public static readonly DependencyProperty Phase1ToolBarVisibilityProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Phase1ToolBarVisibility),
            typeof(Visibility),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty Phase2ToolBarVisibilityProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Phase2ToolBarVisibility),
            typeof(Visibility),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty Phase3ToolBarVisibilityProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.Phase3ToolBarVisibility),
            typeof(Visibility),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary> Выданный в задании граф </summary>
        public static readonly DependencyProperty GivenGraphProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.GivenGraph),
            typeof(IGraph),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(IGraph)));

        /// <summary> Антиграф </summary>
        public static readonly DependencyProperty CurrentGraphProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((TaskTemplateViewModel m) => m.CurrentGraph),
            typeof(IGraph),
            typeof(TaskTemplateViewModel),
            new PropertyMetadata(default(IGraph)));

        /// <summary> Радиус </summary>
        public static readonly DependencyProperty DefaultVertexRadiusProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((GraphVisualizer v) => v.DefaultVertexRadius),
            typeof(double),
            typeof(GraphVisualizer),
            new PropertyMetadata(default(double)));

        /// <summary> Лейблы </summary>

        /// <summary> Идёт загрузка данных? </summary>
        public bool IsLoadingData
        {
            get { return (bool)GetValue(IsLoadingDataProperty); }
            private set { SetValue(IsLoadingDataProperty, value); }
        }

        /// <summary> Разрешено перемещение вершин? </summary>
        public bool IsMouseVerticesMovingEnabled
        {
            get { return (bool)GetValue(IsMouseVerticesMovingEnabledProperty); }
            set { SetValue(IsMouseVerticesMovingEnabledProperty, value); }
        }

        /// <summary> Разрешено добавление рёбер? </summary>
        public bool IsEgesAddingEnabled{
            get { return (bool)GetValue(IsEdgesAddingEnabledProperty); }
            set { SetValue(IsEdgesAddingEnabledProperty, value); }
        }

        /// <summary> Команды панели инструментов </summary>
        public ObservableCollection<ToolBarCommandBase> Phase1ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>)GetValue(Phase1ToolBarCommandsProperty); }
            set { SetValue(Phase1ToolBarCommandsProperty, value); }
        }
        public ObservableCollection<ToolBarCommandBase> Phase2ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>)GetValue(Phase2ToolBarCommandsProperty); }
            set { SetValue(Phase2ToolBarCommandsProperty, value); }
        }
        public ObservableCollection<ToolBarCommandBase> Phase3ToolBarCommands
        {
            get { return (ObservableCollection<ToolBarCommandBase>)GetValue(Phase3ToolBarCommandsProperty); }
            set { SetValue(Phase3ToolBarCommandsProperty, value); }
        }

        /// <summary> Видимость панелей инструментов </summary>
        public Visibility Phase1ToolBarVisibility
        {
            get { return (Visibility)GetValue(Phase1ToolBarVisibilityProperty); }
            set { SetValue(Phase1ToolBarVisibilityProperty, value); }
        }
        public Visibility Phase2ToolBarVisibility
        {
            get { return (Visibility)GetValue(Phase2ToolBarVisibilityProperty); }
            set { SetValue(Phase2ToolBarVisibilityProperty, value); }
        }
        public Visibility Phase3ToolBarVisibility
        {
            get { return (Visibility)GetValue(Phase3ToolBarVisibilityProperty); }
            set { SetValue(Phase3ToolBarVisibilityProperty, value); }
        }

        /// <summary> Выданный в задании граф </summary>
        public IGraph GivenGraph
        {
            get { return (IGraph)GetValue(GivenGraphProperty); }
            set { SetValue(GivenGraphProperty, value); }
        }

        /// <summary> Пустой подграф </summary>
        public IGraph CurrentGraph
        {
            get { return (IGraph)GetValue(CurrentGraphProperty); }
            set { SetValue(CurrentGraphProperty, value); }
        }

        #endregion


        /// <summary> Инициализация </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            UserActionsManager.PropertyChanged += (sender, args) => HandlePropertyChanged(args);
            VariantProvider.PropertyChanged += (sender, args) => HandlePropertyChanged(args);
            InitToolBarCommands();
            SubscribeToViewEvents();
        }

        private void SubscribeToViewEvents()
        {
            View.Loaded += (sender, args) => StartVariantDownload();
        }

        /// <summary> Начать загрузку варианта </summary>
        public void StartVariantDownload()
        {
            VariantProvider.DownloadVariantAsync();
        }

        private void HandlePropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ExpressionUtility.NameForMember((IUiBlockerAsyncProcessor p) => p.IsBusy))
            {
                // Нас могли дёрнуть из другого потока, поэтому доступ к UI - через Dispatcher.
                Dispatcher.BeginInvoke(RecalculateIsLoadingData);
            }
        }

        private void RecalculateIsLoadingData()
        {
            IsLoadingData = VariantProvider.IsBusy || UserActionsManager.IsBusy;
        }

        /// <summary> Задание загружено </summary>
        /// <param name="e"></param>
        protected override void OnTaskLoadingComlete(VariantDownloadedEventArgs e)
        {
            // Мы вызваны из другого потока. Поэтому работаем с UI-элементами через Dispatcher.
            Dispatcher.BeginInvoke(() =>
            {
                GivenGraph = GraphSerializer.Deserialize(e.Data);
                CurrentGraph = UndirectedGraph.CreateEmpty(GivenGraph.VerticesCount);
            });

            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
        }
    }
}
