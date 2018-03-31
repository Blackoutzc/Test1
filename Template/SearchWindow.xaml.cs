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
using System.Windows.Shapes;
using System.ComponentModel;

namespace Template
{
    /// <summary>
    /// SearchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchWindow : Window, INotifyPropertyChanged
    {
        private static SearchWindow _instance = null;
        private static object _lock = new object();
        private MainWindow _mainWindow;
        private RichTextBox _richTextBox;
        private RichTextBoxSearch _richTextBoxSearch;
        public event PropertyChangedEventHandler PropertyChanged;
        private string _info;
        public string DisplayText
        {
            get => _info;
            set
            {
                _info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayText"));
            }
        }

        public SearchWindow(MainWindow parent, RichTextBox richTextBox)
        {
            InitializeComponent();
            _mainWindow = parent;
            _richTextBox = richTextBox;
            Closing += SearchWindow_Closing;
            _richTextBoxSearch = new RichTextBoxSearch(_richTextBox);
            _info = String.Empty;
           /*
            Binding binding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("DisplayText"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            SearchBlock.SetBinding(TextBlock.TextProperty, binding);
           */
        }

        public void SearchWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            DisplayText = String.Empty;
            _richTextBoxSearch.Reset();
        }

        
        public static SearchWindow GetInstance(MainWindow mainWindow, RichTextBox richTextBox)
        {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SearchWindow(mainWindow, richTextBox);
                    }
                    return _instance;
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
            DragMove();
        }

        
        private void SearchNext_Click(object sender, RoutedEventArgs e)
        {
            _richTextBoxSearch.Next();
            DisplayText = $"共搜索到 {_richTextBoxSearch.AllFoundNums} 处实例，此处为第{_richTextBoxSearch.CurrentIndex + 1}处";
            
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _richTextBoxSearch.Search(SearchTextBox.Text.Trim());
            DisplayText = $"共搜索到 {_richTextBoxSearch.AllFoundNums} 处实例";
           
        }

        private void SearchPre_Click(object sender, RoutedEventArgs e)
        {
            _richTextBoxSearch.Pre();
            DisplayText = $"共搜索到 {_richTextBoxSearch.AllFoundNums} 处实例，此处为第{_richTextBoxSearch.CurrentIndex + 1}处";
        }
    }
}
