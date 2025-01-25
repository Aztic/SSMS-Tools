using EnvDTE;
using EnvDTE80;
using SSMSTools.Configurations.SavedDatabaseGroups;
using SSMSTools.Constants;
using SSMSTools.Events.Arguments;
using SSMSTools.Managers.Interfaces;
using SSMSTools.Models;
using SSMSTools.Windows.DatabaseGroupManager;
using SSMSTools.Windows.Interfaces;
using SSMSTools.Mappers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using SSMSTools.Factories.Interfaces;

namespace SSMSTools.Windows.MultiDbQueryRunner
{
    public partial class MultiDbQueryRunnerWindow : System.Windows.Window, INotifyPropertyChanged, IMultiDbQueryRunnerWindow
    {
        public ObservableCollection<CheckboxItem> Databases { get; private set; }
        public ObservableCollection<DatabaseGroup> DatabaseGroups { get; private set; }
        public string ServerName { get; private set; }


        private bool _isShowSystemDatabasesSelected;
        private bool _isAllSelected;
        private bool _isUpdating;
        private string _queryContent;
        private readonly DTE2 _dte;
        private readonly IMessageManager _messageManager;
        private readonly IConfigurationManager _configurationManager;
        private readonly IWindowFactory _windowFactory;

        private readonly ISet<string> _systemDatabases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "master",
            "tempdb",
            "model",
            "msdb"
        };
        private IDatabaseGroupManagerWindow _databaseGroupManagerWindow;
        private DatabaseGroup _selectedDatabaseGroup;

        public bool IsShowSystemDatabasesSelected
        {
            get => _isShowSystemDatabasesSelected;
            set
            {
                if (_isShowSystemDatabasesSelected != value)
                {
                    _isShowSystemDatabasesSelected = value;
                    OnPropertyChanged(nameof(IsShowSystemDatabasesSelected));

                    if (!_isUpdating)
                    {
                        UpdateShowSystemDatabases(value);
                    }
                }
            }
        }

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

        public DatabaseGroup SelectedDatabaseGroup
        {
            get => _selectedDatabaseGroup;
            set
            {
                if (_selectedDatabaseGroup != value)
                {
                    _selectedDatabaseGroup = value;
                    OnPropertyChanged(nameof(SelectedDatabaseGroup));
                    if (!_isUpdating)
                    {
                        HandleDatabaseGroupChange(value);
                    }
                }
            }
        }

        private void HandleDatabaseGroupChange(DatabaseGroup value)
        {
            _isUpdating = true;
            if (value != null)
            {
                if (value.Id.HasValue)
                {
                    var databases = new HashSet<string>(value.Databases);
                    foreach (var database in Databases)
                    {
                        database.IsSelected = databases.Contains(database.Name);
                    }
                }
                _selectedDatabaseGroup = value;
                // Set the visibility of the button, enable it and set the correct text
                if (SelectedDatabaseGroup != null)
                {
                    EditButton.Visibility = Visibility.Visible;
                    EditButton.Content = SelectedDatabaseGroup.Id.HasValue ? "Edit" : "Create";
                    EditButton.IsEnabled = true;
                }
            }
            _isUpdating = false;
        }

        public MultiDbQueryRunnerWindow(
            DTE2 dte,
            IMessageManager messageManager,
            IConfigurationManager configurationManager,
            IWindowFactory windowFactory)
        {
            _dte = dte;
            _messageManager = messageManager;
            _configurationManager = configurationManager;
            _windowFactory = windowFactory;
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Updates the list of available database items
        /// </summary>
        /// <param name="items"></param>
        public void SetServerInformation(ConnectedServerInformation serverInformation)
        {
            // Load all saved databases from disk
            var configuration = _configurationManager.GetConfiguration<SavedDatabaseGroupsConfiguration>(ConfigurationFiles.SavedDatabaseGroups);
            var addNewConfigurationOption = new DatabaseGroup
            {
                Id = null,
                Databases = new List<string>(),
                Title = "New group"
            };

            // Add the 'Add new group' as first option and the saved databases after this one.
            var savedDatabases = new List<DatabaseGroup>();
            savedDatabases.Add(addNewConfigurationOption);
            savedDatabases.AddRange(configuration.Global.Select(x => x.MapToModel()));

            ServerName = serverInformation.ServerName;
            Databases = new ObservableCollection<CheckboxItem>(serverInformation.Databases);
            DatabaseGroups = new ObservableCollection<DatabaseGroup>(savedDatabases);
            foreach (var item in serverInformation.Databases)
            {
                item.IsVisible = _systemDatabases.Contains(item.Name) ?
                    _isShowSystemDatabasesSelected : true;
                item.PropertyChanged += Item_PropertyChanged;
            }

            // Re-set DataContext to refresh bindings
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

        private void UpdateShowSystemDatabases(bool systemDatabasesVisible)
        {
            _isUpdating = true;
            foreach (var item in Databases)
            {
                if (_systemDatabases.Contains(item.Name))
                {
                    item.IsVisible = systemDatabasesVisible;
                }
            }
            _isUpdating = false;
        }

        private void UpdateAllItemsSelection(bool isSelected)
        {
            _isUpdating = true;
            foreach (var database in Databases)
            {
                database.IsSelected = isSelected;
            }
            _isUpdating = false;
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Event handlers

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to close the previous instance if any
            if (_databaseGroupManagerWindow != null)
            {
                try
                {
                    ((System.Windows.Window)_databaseGroupManagerWindow).Close();
                }
                catch(Exception) { }
            }

            // Create the new instance and assign the event handlers
            _databaseGroupManagerWindow = _windowFactory.CreateWindow<IDatabaseGroupManagerWindow>();
            ((System.Windows.Window)_databaseGroupManagerWindow).Owner = this;
            _databaseGroupManagerWindow.RefreshDatabaseList += new EventHandler(DatabaseGroupManagerWindow_RefreshDatabaseList);
            _databaseGroupManagerWindow.ContentSaved += new EventHandler(DatabaseGroupManagerWindow_ContentSaved);
            ((System.Windows.Window)_databaseGroupManagerWindow).Closed += new EventHandler(DatabaseGroupManagerWindow_Closed);

            // Set the databases and title
            _databaseGroupManagerWindow.SetDatabaseGroupId(SelectedDatabaseGroup.Id);
            _databaseGroupManagerWindow.SetDatabaseGroupName(SelectedDatabaseGroup.Title);
            _databaseGroupManagerWindow.SetDatabases(Databases, true);
            ((System.Windows.Window)_databaseGroupManagerWindow).ShowDialog();

        }

        private void DatabaseGroupManagerWindow_ContentSaved(object sender, EventArgs args)
        {
            var contentSavedArgs = (DatabaseGroupSavedEventArgs)args;
            DatabaseGroup databaseGroup = DatabaseGroups.FirstOrDefault(x => x.Id == contentSavedArgs.DatabaseGroupId);
            if (databaseGroup?.Id == null)
            {
                databaseGroup = new DatabaseGroup
                {
                    Id = Guid.NewGuid()
                };

                DatabaseGroups.Add(databaseGroup);
            }

            // Edit the existing databases
            databaseGroup.Title = contentSavedArgs.DatabaseGroupName;
            databaseGroup.Databases = contentSavedArgs.Databases.ToList();

            // Save content
            var configuration = new SavedDatabaseGroupsConfiguration();
            configuration.Global = DatabaseGroups.Where(x => x.Id.HasValue).Select(x => x.MapToSavedDatabaseGroup()).ToList();
            _configurationManager.SaveConfiguration<SavedDatabaseGroupsConfiguration>(ConfigurationFiles.SavedDatabaseGroups, configuration);
            SelectedDatabaseGroup = null;
            SelectedDatabaseGroup = databaseGroup;
            ((System.Windows.Window)_databaseGroupManagerWindow).Close();
        }

        private void DatabaseGroupManagerWindow_Closed(object sender, EventArgs args)
        {
            _databaseGroupManagerWindow = null;
        }

        private void DatabaseGroupManagerWindow_RefreshDatabaseList(object sender, EventArgs args)
        {
            var databaseList = new List<CheckboxItem>(Databases.Select(x => x.Clone()));
            databaseList.ForEach(x => {
                x.IsSelected = false;
                x.IsVisible = true;
            });
            _databaseGroupManagerWindow?.SetDatabases(databaseList, false);
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic to handle the "Execute" action
            var content = new StringBuilder();
            foreach (var database in Databases)
            {
                if (database.IsSelected && database.IsVisible)
                {
                    content.Append($"USE [{database.Name}]\n");
                    content.Append($"Print 'Running query in [{database.Name}]'\n");
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

        private void DatabaseGroupsCombobox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedDatabaseGroup != null)
            {
                EditButton.Visibility = Visibility.Visible;
                EditButton.Content = SelectedDatabaseGroup.Id.HasValue ? "Edit" : "Create";
                EditButton.IsEnabled = true;
            }
        }
        #endregion Event handlers
    }
}
