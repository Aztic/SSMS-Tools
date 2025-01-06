using EnvDTE;
using EnvDTE80;
using SSMSTools.Configurations.SavedDatabaseGroups;
using SSMSTools.Constants;
using SSMSTools.Events.Arguments;
using SSMSTools.Managers.Interfaces;
using SSMSTools.Models;
using SSMSTools.Windows.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SSMSTools.Windows.DatabaseGroupManager
{
    public partial class DatabaseGroupManagerWindow : System.Windows.Window, INotifyPropertyChanged, IDatabaseGroupManagerWindow
    {
        private ICollection<string> _defaultSelectedDatabases = new Collection<string>();
        public ObservableCollection<CheckboxItem> Databases { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ContentSaved;
        public event EventHandler RefreshDatabaseList;

        private bool _isAllSelected;
        private bool _isUpdating;
        private string _databaseGroupName;
        private Guid? _databaseGroupId;

        public DatabaseGroupManagerWindow()
        {
            InitializeComponent();
            DataContext = this;
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

                    if (!_isUpdating)
                    {
                        UpdateAllItemsSelection(value);
                    }
                }
            }
        }

        public string DatabaseGroupName
        {
            get => _databaseGroupName;
            set
            {
                _databaseGroupName = value;
                OnPropertyChanged(nameof(DatabaseGroupName));
            }
        }

        private void UpdateAllItemsSelection(bool isSelected)
        {
            _isUpdating = true;
            foreach (var item in Databases)
            {
                item.IsSelected = isSelected;
            }
            _isUpdating = false;
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Public methods

        public void SetDatabaseGroupName(string databaseGroupName)
        {
            DatabaseGroupName = databaseGroupName;
            DataContext = this;
        }

        public void SetDatabaseGroupId(Guid? id)
        {
            _databaseGroupId = id;
        }

        public void SetDatabases(IEnumerable<CheckboxItem> databases, bool resetSelectedDatabases)
        {
            if (resetSelectedDatabases)
            {
                _defaultSelectedDatabases = new Collection<string>();
            }

            Databases = new ObservableCollection<CheckboxItem>(databases.Select(x => x.Clone()));
            foreach(var database in Databases)
            {
                database.PropertyChanged += Item_PropertyChanged;
                if (database.IsSelected && resetSelectedDatabases)
                {
                    _defaultSelectedDatabases.Add(database.Name);
                }
            }

            DataContext = this;
        }

        #endregion Public methods


        #region Event handlers

        private void RefreshDatabaseListButton_Click()
        {
            RefreshDatabaseList?.Invoke(this, EventArgs.Empty);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var eventArgs = new DatabaseGroupSavedEventArgs
            {
                DatabaseGroupId = _databaseGroupId,
                DatabaseGroupName = _databaseGroupName,
                Databases = new List<string>(Databases.Where(x => x.IsSelected).Select(x => x.Name))
            };
            ContentSaved?.Invoke(this, eventArgs);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

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
        #endregion Event handlers

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InvalidateVisual();
        }
    }
}
