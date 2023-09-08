using Dapper;
using Microsoft.Extensions.Configuration;
using Moq;
using SmartVault.DAL;
using SmartVault.Library;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;

namespace SmartVault.UnitTests.cs
{
    public class DatabaseCreationTests
    {

        DatabaseCreation _sut;
        SQLiteConnection _connection; //Cannot be mocket as this does not depends on an interface
        Mock<IConfigurationRoot> _configurationMock;
        string _databasePath;

        [SetUp]
        public void Setup()
        {
            _databasePath = "unitTestDB";
            _configurationMock = new Mock<IConfigurationRoot>();
            _configurationMock.Setup(x => x["DataGeneration:SeedElementsCount"]).Returns("5");
            _configurationMock.Setup(x => x["DataGeneration:SeedDocumentsCount"]).Returns("5");
            _configurationMock.Setup(x => x["DatabaseFileName"]).Returns(_databasePath);
            _configurationMock.Setup(x => x["ConnectionStrings:DefaultConnection"]).Returns("data source={0}");
            _connection = DatabaseCreation.CreateDatabaseIntance(_configurationMock.Object);
            //_connectionMock.Setup(x => x.ExecuteScalar<string>(It.IsAny<string>(),null,null,null,null)).Returns("Result");
            //_connectionMock.Setup(x => x.ExecuteAsync(It.IsAny<string>(), null, null, null, null)).Returns(Task.FromResult(1));
            //_connectionMock.Setup(x => x.BeginTransactionAsync(token)).Returns(valueTask);
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            var dbTransactionMock = new Mock<DbTransaction>();
            var valueTask = new ValueTask<DbTransaction>(dbTransactionMock.Object);
            

            
        }

        [Test]
        public async Task CreateDatabase_ValidSQLConnectionSent_ShouldReturnTrueIfAllEntitiesWereAdded()
        {
            _sut = new DatabaseCreation(_connection);
            var result = await _sut.CreateDatabase(_configurationMock.Object);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task CreateDatabase_DataAlreadyExists_ShouldThrowExceptionUserIdAlreadyExists()
        {
            _sut = new DatabaseCreation(_connection);
            await _sut.CreateDatabase(_configurationMock.Object);
            AsyncTestDelegate testDelegate = async () => { await _sut.CreateDatabase(_configurationMock.Object); };

            Assert.ThrowsAsync<SQLiteException>(testDelegate, "System.Data.SQLite.SQLiteException : constraint failed\r\nUNIQUE constraint failed: User.Id");

        }

        [TearDown]
        public void TearDown()
        {
            _sut.CloseConnection();
            File.Delete(_databasePath); //Delete the database file to avoid exceptions.
        }

        //System.Data.SQLite.SQLiteException 
    }
}