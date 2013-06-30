using System.Windows;

namespace SetGuidConverter
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;

    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string directory;

        private List<string> logItems;

        private bool enableUi;

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

        public List<string> LogItems
        {
            get
            {
                return this.logItems;
            }
            set
            {
                if (this.LogItems == value) return;
                this.logItems = value;
                OnPropertyChanged("LogItems");
            }
        }

        public bool EnableUi
        {
            get
            {
                return this.enableUi;
            }
            set
            {
                if (this.enableUi == value) return;
                this.enableUi = value;
                OnPropertyChanged("EnableUi");
            }
        }

        public MainWindow()
        {
            enableUi = true;
            InitializeComponent();
            LogItems = new List<string>();
            Directory = "";
        }

        public void PickDirectoryClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var of = new System.Windows.Forms.FolderBrowserDialog();
            var res = of.ShowDialog();
            if (res != System.Windows.Forms.DialogResult.OK) return;
            Directory = of.SelectedPath;
        }

        public void ConvertClick(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                EnableUi = false;
                var conv = new Converter(Directory);
                conv.OnEvent += s => LogItems.Insert(0, s);
                conv.Verify().ContinueWith(x => this.VerifyComplete(conv, x));
            }
            catch (Exception e)
            {
                this.Log("Error: {0}", e.Message);
                EnableUi = true;
            }
        }

        public void VerifyComplete(Converter converter, Task<bool> res)
        {
            if (this.HandleFaultedTask(res))
            {
                EnableUi = true;
                converter.Dispose();
                return;
            }
            if (res.Result)
            {
                converter.Convert().ContinueWith(x => this.ConvertComplete(converter, x));
            }
        }

        public void ConvertComplete(Converter converter, Task res)
        {
            try
            {
                if (this.HandleFaultedTask(res))
                {
                    MessageBox.Show(
                        "Conversion Completed With Some Errors", "Finsihed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                    MessageBox.Show(
                        "Conversion Completed With No Errors", "Finsihed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                this.Log("Error: {0}", e.Message);
            }
            finally
            {
                converter.Dispose();
                EnableUi = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns>True if faulted, otherwise false</returns>
        internal bool HandleFaultedTask(Task task)
        {
            if (!task.IsFaulted)
            {
                return false;
            }
            try
            {
                if (task.Exception == null)
                {
                    this.Log("Something unexpected happened.");
                }
                else
                {
                    foreach (var e in task.Exception.InnerExceptions)
                    {
                        this.Log("Error: {0}", e.Message);
                    }
                }

            }
            catch (Exception e)
            {
                this.Log("Error: {0}", e.Message);
            }
            return true;
        }

        internal void Log(string mess, params object[] args)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => this.Log(mess, args)));
                return;
            }
            LogItems.Add(String.Format(mess, args));
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
