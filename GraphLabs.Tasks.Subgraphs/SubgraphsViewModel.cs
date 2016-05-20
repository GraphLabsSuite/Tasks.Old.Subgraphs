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

namespace GraphLabs.Tasks.Subgraphs
{
    /// <summary> ViewModel для TaskTemplate </summary>
    public partial class SubgraphsViewModel : TaskViewModelBase<TaskTemplate>
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
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.IsLoadingData), 
            typeof(bool), 
            typeof(SubgraphsViewModel),
            new PropertyMetadata(false));

        /// <summary> Разрешено перемещение вершин? </summary>
        public static readonly DependencyProperty IsMouseVerticesMovingEnabledProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.IsMouseVerticesMovingEnabled),
            typeof(bool),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(true));

        /// <summary> Разрешено добавление рёбер? </summary>
        public static readonly DependencyProperty IsEdgesAddingEnabledProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.IsEgesAddingEnabled),
            typeof(bool),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(false));

        /// <summary> Команды панели инструментов</summary>
        public static readonly DependencyProperty Phase1ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.Phase1ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));
        public static readonly DependencyProperty Phase2ToolBarCommandsProperty = DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.Phase2ToolBarCommands),
            typeof(ObservableCollection<ToolBarCommandBase>),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(default(ObservableCollection<ToolBarCommandBase>)));

        /// <summary> Видимость тулбаров </summary>
        public static readonly DependencyProperty Phase1ToolBarVisibilityProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.Phase1ToolBarVisibility),
            typeof(Visibility),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty Phase2ToolBarVisibilityProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.Phase2ToolBarVisibility),
            typeof(Visibility),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(Visibility.Collapsed));

        ///<summary> Видимость заданий </summary>
        public static readonly DependencyProperty Phase12VisibilityProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.Phase12Visibility),
            typeof(Visibility),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(Visibility.Visible));

        /// <summary> Выданный в задании граф </summary>
        public static readonly DependencyProperty GivenGraphProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.GivenGraph),
            typeof(IGraph),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(default(IGraph)));

        /// <summary> Рабочий граф </summary>
        public static readonly DependencyProperty CurrentGraphProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.CurrentGraph),
            typeof(IGraph),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(default(IGraph)));

        /// <summary> Радиус </summary>
        public static readonly DependencyProperty DefaultVertexRadiusProperty =
            DependencyProperty.Register(
            ExpressionUtility.NameForMember((GraphVisualizer v) => v.DefaultVertexRadius),
            typeof(double),
            typeof(SubgraphsViewModel),
            new PropertyMetadata(default(double)));

        /// <summary> Лейблы </summary>
        public static readonly DependencyProperty L1Property =
            DependencyProperty.Register(
                ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.L1),
                typeof (String),
                typeof (SubgraphsViewModel),
                new PropertyMetadata("Дополнение"));
        public static readonly DependencyProperty L2Property =
            DependencyProperty.Register(
                ExpressionUtility.NameForMember((SubgraphsViewModel m) => m.L2),
                typeof(String),
                typeof(SubgraphsViewModel),
                new PropertyMetadata("Исходный граф"));

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

        ///<summary> Видимость заданий </summary>
        public Visibility Phase12Visibility
        {
            get { return (Visibility)GetValue(Phase12VisibilityProperty); }
            set { SetValue(Phase12VisibilityProperty, value); }
        }

        /// <summary> Выданные в задании графы </summary>
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

        public String L1
        {
            get { return (String)GetValue(L1Property); }
            set { SetValue(L1Property, value); }
        }
        public String L2
        {
            get { return (String)GetValue(L2Property); }
            set { SetValue(L2Property, value); }
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
                var data = VariantSerializer.Deserialize(e.Data);
                GivenGraph = data[0];
                CurrentGraph = UndirectedGraph.CreateEmpty(GivenGraph.VerticesCount);
            });

            //var number = e.Number; -- м.б. тоже где-то показать надо
            //var version = e.Version;
        }
    }
}
