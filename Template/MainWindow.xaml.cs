using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Template
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private RoutedCommand OpenSearchWindowCommand = new RoutedCommand("OpenSearch", typeof(RichTextBox));
        private SearchWindow _searchWindow  =null;
        public MainWindow()
        {
            InitializeComponent();
            _searchWindow = SearchWindow.GetInstance(this, this?.RichTextBoxInstance);
            toolbar.Loaded += Toolbar_Loaded;
            HandleCommand();
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _searchWindow.Closing -= _searchWindow.SearchWindow_Closing;
            _searchWindow.Close();
        }

        private void HandleCommand()
        {
            SearchItem.Command = OpenSearchWindowCommand;
            SearchItem.CommandTarget = RichTextBoxInstance;
            OpenSearchWindowCommand.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
            CommandBinding searchBinding = new CommandBinding(OpenSearchWindowCommand, OpenSearch_Executed, OpenSearch_CanExecute);
            CommandBindings.Add(searchBinding);
        }

        private void OpenSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _searchWindow.Show();
            e.Handled = true;
        }

        private void Toolbar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;

            if (toolbar.Template.FindName("OverflowGrid", toolbar) is FrameworkElement overflowGrid)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            if (toolBar.Template.FindName("MainPanelBorder", toolBar) is FrameworkElement mainPanelBorder)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
