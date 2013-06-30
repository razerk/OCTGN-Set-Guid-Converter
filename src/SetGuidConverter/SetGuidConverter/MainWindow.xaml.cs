using System.Windows;

namespace SetGuidConverter
{
    using System.ComponentModel;

    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string directory;

        public string Directory
        {
            get
            {
                return this.directory;
            }
            set
            {
                if (this.directory == value) return;
                this.directory = value;
                OnPropertyChanged("Directory");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void PickDirectoryClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var of = new System.Windows.Forms.FolderBrowserDialog();
            var res = of.ShowDialog();
            if (res != System.Windows.Forms.DialogResult.OK) return;
            Directory = of.SelectedPath;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
