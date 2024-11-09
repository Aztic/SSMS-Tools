using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Moq;
using SSMSTools.Commands.MultiDbQueryRunner;
using SSMSTools.Factories.Interfaces;
using SSMSTools.Managers.Interfaces;
using SSMSTools.Services.Interfaces;
using SSMSTools.UnitTests.Utilities;
using SSMSTools.Windows.Interfaces;
using System;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace SSMSTools.UnitTests.Commands.MultiDbQueryRunner
{
    public class MultiDbQueryRunnerTests
    {
        public class MultiDbQueryRunnerCommandTests
        {
            private readonly Mock<IObjectExplorerService> _objectExplorerServiceMock;
            private readonly Mock<IMessageManager> _messageManagerMock;
            private readonly Mock<IWindowFactory> _windowFactoryMock;
            private readonly Mock<IMultiDbQueryRunnerWindow> _windowMock;
            private readonly Mock<MockableAsyncPackage> _packageMock;
            private readonly Mock<IServiceProvider> _serviceProviderMock;
            private readonly MultiDbQueryRunnerCommand _commandInstance;
            private readonly Mock<IUIService> _uiServiceMock;

            delegate void GetSelectedNodesCallback(out int size, out INodeInformation[] nodes);

            public MultiDbQueryRunnerCommandTests()
            {
                _uiServiceMock = new Mock<IUIService>();
                _objectExplorerServiceMock = new Mock<IObjectExplorerService>();
                _messageManagerMock = new Mock<IMessageManager>();
                _windowFactoryMock = new Mock<IWindowFactory>();
                _windowMock = new Mock<IMultiDbQueryRunnerWindow>();
                _serviceProviderMock = new Mock<IServiceProvider>();
                _serviceProviderMock.Setup(x => x.GetService(typeof(IObjectExplorerService))).Returns(_objectExplorerServiceMock.Object);
                _serviceProviderMock.Setup(x => x.GetService(typeof(IMessageManager))).Returns(_messageManagerMock.Object);
                _serviceProviderMock.Setup(x => x.GetService(typeof(IUIService))).Returns(_uiServiceMock.Object);
                _packageMock = new Mock<MockableAsyncPackage>();
                _packageMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

                _windowFactoryMock.Setup(factory => factory.CreateWindow<IMultiDbQueryRunnerWindow>()).Returns(_windowMock.Object);
                var menuCommand = new OleMenuCommandService(_serviceProviderMock.Object);
                _commandInstance = new MultiDbQueryRunnerCommand(_packageMock.Object, menuCommand);
            }

            [Fact]
            public async Task Execute_ShouldShowMessage_WhenNoDatabasesAvailable()
            {
                // Arrange
                var nodeInformationMock = new Mock<INodeInformation>();
                var mockConnectionInfo = new Mock<SqlOlapConnectionInfoBase>(string.Empty, string.Empty, string.Empty, ConnectionType.SqlConnection);
                mockConnectionInfo.Setup(x => x.ConnectionString).Returns("foo");
                nodeInformationMock.Setup(x => x.Connection).Returns(mockConnectionInfo.Object);
                var returnedArray = new INodeInformation[1]
                {
                    nodeInformationMock.Object
                };
                _objectExplorerServiceMock.Setup(service => service.GetSelectedNodes(out It.Ref<int>.IsAny, out It.Ref<INodeInformation[]>.IsAny))
                    .Callback(new GetSelectedNodesCallback((out int size, out INodeInformation[] nodes) =>
                    {
                        size = 1;
                        nodes = returnedArray;
                    }));

                // Act
                _commandInstance.Execute(null, EventArgs.Empty);


                // Assert
                _messageManagerMock.Verify(mgr => mgr.ShowMessageBox(_packageMock.Object, "MultiDbQueryRunner", "The connection has no available databases"), Times.Once);
            }

            [Fact]
            public async Task Execute_ShouldHandleOnlyOneObjectExplorerNodeAllowedException()
            {
                // Arrange
                _objectExplorerServiceMock.Setup(service => service.GetSelectedNodes(out It.Ref<int>.IsAny, out It.Ref<INodeInformation[]>.IsAny))
                    .Callback(new GetSelectedNodesCallback((out int size, out INodeInformation[] nodes) =>
                    {
                        size = 5;
                        nodes = new INodeInformation[0];
                    }));

                // Act
                _commandInstance.Execute(null, EventArgs.Empty);

                // Assert
                _messageManagerMock.Verify(mgr => mgr.ShowMessageBox(_packageMock.Object, "MultiDbQueryRunner", "Only one node needs to be selected"), Times.Once);
            }

            [Fact]
            public async Task Execute_ShouldHandleUnknownExceptions()
            {
                // Arrange
                _objectExplorerServiceMock.Setup(service => service.GetSelectedNodes(out It.Ref<int>.IsAny, out It.Ref<INodeInformation[]>.IsAny))
                                          .Throws(new Exception("Test exception"));

                // Act
                _commandInstance.Execute(null, EventArgs.Empty);

                // Assert
                _messageManagerMock.Verify(mgr => mgr.ShowMessageBox(_packageMock.Object, "MultiDbQueryRunner", "Unknown exception"), Times.Once);
            }
        }
    }
}
