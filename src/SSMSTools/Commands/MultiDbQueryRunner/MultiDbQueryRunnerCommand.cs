using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using SSMSTools.Managers.Interfaces;
using SSMSTools.Models;
using SSMSTools.Factories.Interfaces;
using SSMSTools.Windows.Interfaces;
using SSMSTools.Exceptions;
using SSMSTools.Services.Interfaces;

namespace SSMSTools.Commands.MultiDbQueryRunner
{
    internal sealed class MultiDbQueryRunnerCommand
    {
        private readonly IObjectExplorerService _objectExplorerService;
        private readonly IMessageManager _messageManager;
        private readonly IWindowFactory _windowFactory;
        private readonly IUIService _uiService;

        private struct ConnectedServer
        {
            public string ServerName { get; set; }
            public string ConnectionString { get; set; }
        }

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6539408f-9446-4c2f-9916-90c49bc6b246");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiDbQueryRunnerCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        internal MultiDbQueryRunnerCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);

            _objectExplorerService = ((SSMSToolsPackage)package).ServiceProvider.GetService(typeof(IObjectExplorerService)) as IObjectExplorerService;
            _messageManager = ((SSMSToolsPackage)package).ServiceProvider.GetService(typeof(IMessageManager)) as IMessageManager;
            _windowFactory = ((SSMSToolsPackage)package).ServiceProvider.GetService(typeof(IWindowFactory)) as IWindowFactory;
            _uiService = ((SSMSToolsPackage)package).ServiceProvider.GetService(typeof(IUIService)) as IUIService;

            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MultiDbQueryRunnerCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MultiDbQueryRunner's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = (OleMenuCommandService)await package.GetServiceAsync((typeof(IMenuCommandService)));
            Instance = new MultiDbQueryRunnerCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        internal void Execute(object sender, EventArgs e)
        {
            _uiService.ValidateUIThread();
            string title = "MultiDbQueryRunner";
            string serverName = string.Empty;
            var serverInformation = new ConnectedServerInformation();

            try
            {
                var connection = GetConnectedServers().Single();
                var connectionDatabases = GetDatabasesFromConnection(connection.ConnectionString);
                if (!connectionDatabases.Any())
                {
                    // Show a message box to prove we were here
                    _messageManager.ShowMessageBox(this.package, title, "The connection has no available databases");
                    return;
                }

                serverInformation.ServerName = connection.ServerName;
                serverInformation.Databases = connectionDatabases.Select(x => new CheckboxItem { Name = x });
            }
            catch(OnlyOneObjectExplorerNodeAllowedException)
            {
                _messageManager.ShowMessageBox(this.package, title, "Only one node needs to be selected");
                return;
            }
            catch(Exception ex)
            {
                _messageManager.ShowMessageBox(this.package, title, "Unknown exception");
                return;
            }

            var window = _windowFactory.CreateWindow<IMultiDbQueryRunnerWindow>();
            window.SetServerInformation(serverInformation);
            window.Show();
        }

        /// <summary>
        /// Gets the connection string from the selected nodes
        /// </summary>
        /// <returns></returns>
        private List<ConnectedServer> GetConnectedServers()
        {
            var usedServers = new HashSet<string>();
            var connections = new List<ConnectedServer>();

            int arraySize;
            INodeInformation[] nodes = new INodeInformation[10];
            _objectExplorerService.GetSelectedNodes(out arraySize, out nodes);
            if (arraySize != 1)
            {
                throw new OnlyOneObjectExplorerNodeAllowedException("Only one node needs to be selected");
            }
            foreach (var node in nodes)
            {
                var connectionString = node.Connection.ConnectionString;
                var serverName = node.Connection.ServerName;
                if (usedServers.Contains(serverName))
                {
                    continue;
                }

                usedServers.Add(serverName);
                connections.Add(new ConnectedServer
                {
                    ServerName = serverName,
                    ConnectionString = connectionString
                });
            }

            return connections;
        }

        /// <summary>
        /// Given a connection string, gets the list of available databases.
        /// It creates a new connection with the server and retrieves them
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private List<string> GetDatabasesFromConnection(string connectionString)
        {
            var databases = new List<string>();

            try
            {
                string query = $"SELECT name FROM sys.databases";
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                databases.Add(reader["name"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _messageManager.ShowMessageBox(this.package, nameof(MultiDbQueryRunner), $"Error fetching databases for linked server: {ex.Message}");
            }

            return databases;
        }
    }
}
