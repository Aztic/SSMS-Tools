using EnvDTE;
using EnvDTE80;
using SSMSTools.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SSMSTools.Windows.MultiDbQueryRunner
{
    public partial class MultiDbQueryRunnerWindow : System.Windows.Window, INotifyPropertyChanged
    {
        public MultiDbQueryRunnerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private DTE2 _dte;
        public ObservableCollection<CheckboxItem> Items { get; set; }

        private bool _isAllSelected;
        private bool _isUpdating;

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

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public void SetItems(IEnumerable<CheckboxItem> items)
        {
            Items = new ObservableCollection<CheckboxItem>(items);
            foreach (var item in items)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }

            // Re-set DataContext to refresh bindings
            DataContext = null;
            DataContext = this;
        }

        public void SetEnvDte(DTE2 dte)
        {
            _dte = dte;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CheckboxItem.IsSelected) && !_isUpdating)
            {
                // Update IsAllSelected based on current item selections if we're not in a bulk update
                _isUpdating = true;
                IsAllSelected = Items.All(item => item.IsSelected);
                _isUpdating = false;
            }
        }

        private void UpdateAllItemsSelection(bool isSelected)
        {
            // Set the _isUpdating flag to true to prevent recursion
            _isUpdating = true;
            foreach (var item in Items)
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
            MessageBox.Show("Execute button clicked. Notes: " + Notes);
            var content = new StringBuilder();
            foreach (var database in Items)
            {
                if (database.IsSelected)
                {
                    content.Append($"USE {database.Name}\n");
                    content.Append($"Print 'Running query in {database.Name}'\n");
                    content.Append(Notes);
                    content.Append("\n\n");

                }
            }
            OpenNewQueryWindow(content.ToString());
            CloseToolWindow();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic to handle the "Cancel" action, e.g., clearing fields or closing the window
            MessageBox.Show("Cancel button clicked. Clearing fields...");
            Notes = string.Empty; // Clear the multiline text
            foreach (var item in Items)
            {
                item.IsSelected = false; // Uncheck all items
            }
            CloseToolWindow();
        }

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
                MessageBox.Show($"Error creating new query window: {ex.Message}");
            }
        }

        private void CloseToolWindow()
        {
            System.Windows.Window.GetWindow(this).Close();
        }
    }
}
