using EnvDTE;
using EnvDTE80;
using SSMSTools.Managers.Interfaces;
using SSMSTools.Models;
using SSMSTools.Windows.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SSMSTools.Windows.MultiDbQueryRunner
{
    public partial class MultiDbQueryRunnerWindow : System.Windows.Window, INotifyPropertyChanged, IMultiDbQueryRunnerWindow
    {
        public ObservableCollection<CheckboxItem> Databases { get; private set; }
        public string ServerName { get; private set; }

        private bool _isAllSelected;
        private bool _isUpdating;
        private string _queryContent;
        private readonly DTE2 _dte;
        private readonly IMessageManager _messageManager;

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (_isAllSelected != value)
                {
                    _isAllSelected = value;
                    OnPropertyChanged(nameof(IsAllSelected));

                    // Only update items if we're not already updating
                    if (!_isUpdating)
                    {
                        UpdateAllItemsSelection(value);
                    }
                }
            }
        }
        
        public string QueryContent
        {
            get => _queryContent;
            set
            {
                _queryContent = value;
                OnPropertyChanged(nameof(QueryContent));
            }
        }

        public MultiDbQueryRunnerWindow(
            DTE2 dte,
            IMessageManager messageManager)
        {
            _dte = dte;
            _messageManager = messageManager;
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Updates the list of available database items
        /// </summary>
        /// <param name="items"></param>
        public void SetServerInformation(ConnectedServerInformation serverInformation)
        {
            ServerName = serverInformation.ServerName;
            Databases = new ObservableCollection<CheckboxItem>(serverInformation.Databases);
            foreach (var item in serverInformation.Databases)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }

            // Re-set DataContext to refresh bindings
            DataContext = null;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckboxItem.IsSelected) && !_isUpdating)
            {
                // Update IsAllSelected based on current item selections if we're not in a bulk update
                _isUpdating = true;
                IsAllSelected = Databases.All(item => item.IsSelected);
                _isUpdating = false;
            }
        }

        private void UpdateAllItemsSelection(bool isSelected)
        {
            // Set the _isUpdating flag to true to prevent recursion
            _isUpdating = true;
            foreach (var item in Databases)
            {
                item.IsSelected = isSelected;
            }
            _isUpdating = false;
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic to handle the "Execute" action
            var content = new StringBuilder();
            foreach (var database in Databases)
            {
                if (database.IsSelected)
                {
                    content.Append($"USE {database.Name}\n");
                    content.Append($"Print 'Running query in {database.Name}'\n");
                    content.Append(QueryContent);
                    content.Append("\n\n");

                }
            }
            OpenNewQueryWindow(content.ToString());
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            QueryContent = string.Empty;
            foreach (var item in Databases)
            {
                item.IsSelected = false;
            }
            Close();
        }

        /// <summary>
        /// Tries to open a new query window and writes the specified content in the newly created window
        /// </summary>
        /// <param name="content"></param>
        private void OpenNewQueryWindow(string content)
        {
            try
            {
                // Use SSMS DTE command to open a new query window
                _dte.ExecuteCommand("File.NewQuery");

                // Get the active document, which should now be the new query window
                Document newQueryWindow = _dte.ActiveDocument;
                if (newQueryWindow == null || newQueryWindow.Type != "Text")
                {
                    MessageBox.Show("Unable to create a new SQL query window.");
                    return;
                }

                // Insert the content into the new query window
                var textDoc = (TextDocument)newQueryWindow.Object("TextDocument");
                var editPoint = textDoc.StartPoint.CreateEditPoint();
                editPoint.Insert(content);

                // Activate and bring the new query window to the foreground
                newQueryWindow.Activate();
            }
            catch (Exception ex)
            {
                _messageManager.ShowSimpleMessageBox($"Error creating new query window: {ex.Message}");
            }
        }
    }
}
