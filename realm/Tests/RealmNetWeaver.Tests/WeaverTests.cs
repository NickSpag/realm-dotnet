﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Moq;
using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;

namespace Tests
{
    [TestFixture]
    public class WeaverTests
    {
        Assembly _assembly;
        string _newAssemblyPath;
        string _assemblyPath;

        private Mock<ICoreProvider> _coreProviderMock;
        private Mock<ISharedGroupHandle> _sharedGroupHandleMock;
        private Mock<IGroupHandle> _groupHandleMock;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
            _assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

            _newAssemblyPath = _assemblyPath.Replace(".dll", ".processed.dll");
            File.Copy(_assemblyPath, _newAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(_newAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition
            };

            weavingTask.Execute();
            moduleDefinition.Write(_newAssemblyPath);

            _assembly = Assembly.LoadFile(_newAssemblyPath);

            // Try accessing assembly to ensure that the assembly is still valid.
            try
            {
                _assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var item in e.LoaderExceptions)
                    Debug.WriteLine("Loader exception: " + item.Message.ToString());

                Assert.Fail("Load failure");
            }
        }

        [SetUp]
        public void Setup()
        {
            _coreProviderMock = new Mock<ICoreProvider>();
            _sharedGroupHandleMock = new Mock<ISharedGroupHandle>();
            _groupHandleMock = new Mock<IGroupHandle>();

            _coreProviderMock.Setup(cp => cp.CreateSharedGroup(It.IsAny<string>()))
                .Returns(_sharedGroupHandleMock.Object);

            _sharedGroupHandleMock.Setup(sgh => sgh.StartTransaction(It.IsAny<TransactionState>()))
                .Returns(_groupHandleMock.Object);

            Realm.ActiveCoreProvider = _coreProviderMock.Object;
        }

        [Test]
        public void ShouldCreateTable()
        {
            // Arrange
            var realm = Realm.GetInstance();

            // Act
            using(realm.BeginWrite())
                realm.CreateObject(_assembly.GetType("AssemblyToProcess.Person"));

            // Assert
            //_coreProviderMock.Verify(cp => cp.AddTable(It.IsAny<IGroupHandle>(), "Person"));
            //Assert.That(coreProviderStub_.HasTable("Person"));
            //var table = coreProviderStub_.Tables["Person"];
            //Assert.That(table.Columns.Count, Is.EqualTo(5));
            //Assert.That(table.Columns["FirstName"], Is.EqualTo(typeof(string)));
        }

        [Test]
        public void ShouldSetPropertyInDatabase()
        {
            // Arrange
            var realm = Realm.GetInstance();
            var person = (dynamic)realm.CreateObject(_assembly.GetType("AssemblyToProcess.Person"));

            // Act
            person.FirstName = "John";

            // Assert
            //var table = coreProviderStub_.Tables["Person"];
            //Assert.That(table.Rows[0]["FirstName"], Is.EqualTo("John"));
        }

        [Test]
        public void ShouldKeepMultipleRowsSeparate()
        {
            // Arrange
            var realm = Realm.GetInstance();
            var person1 = (dynamic)realm.CreateObject(_assembly.GetType("AssemblyToProcess.Person"));
            var person2 = (dynamic)realm.CreateObject(_assembly.GetType("AssemblyToProcess.Person"));
            person1.FirstName = "John";

            // Act
            person2.FirstName = "Peter";
            person1.FirstName = "Joe";

            // Assert
            //var table = coreProviderStub_.Tables["Person"];
            //Assert.That(table.Rows[0]["FirstName"], Is.EqualTo("Joe"));
            //Assert.That(table.Rows[1]["FirstName"], Is.EqualTo("Peter"));
        }

        [Test]
        public void ShouldFollowMapToAttributeOnProperties()
        {
            // Arrange
            var realm = Realm.GetInstance();
            var person = (dynamic)realm.CreateObject(_assembly.GetType("AssemblyToProcess.Person"));

            // Act
            person.Email = "john@johnson.com";

            // Assert
            //var table = coreProviderStub_.Tables["Person"];
            //Assert.That(table.Rows[0]["Email"], Is.EqualTo("john@johnson.com"));
        }

        [Test, NUnit.Framework.Ignore("Not implemented yet..")]
        public void ShouldFollowMapToAttributeOnClasses()
        {
            // Arrange
            var realm = Realm.GetInstance();

            // Act
            realm.CreateObject(_assembly.GetType("AssemblyToProcess.RemappedClass"));

            // Assert
            //Assert.That(coreProviderStub_.HasTable("RemappedTable"), "The table RemappedTable was not found");
            //Assert.That(!coreProviderStub_.HasTable("RemappedClass"), "The table RemappedClass was found though it should not exist");
        }

#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(_assemblyPath,_newAssemblyPath);
        }
#endif
    }
}