﻿using System;
using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Aws.DynamoDb;
using ServiceStack.Testing;
using ServiceStack.Text;

namespace ServiceStack.Aws.DynamoDbTests
{
    [TestFixture]
    public class DynamoDbAuthRepositoryTests : DynamoTestBase
    {
        private ServiceStackHost appHost;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DynamoMetadata.Reset();
            var db = CreatePocoDynamo();
            db.DeleteAllTables(TimeSpan.FromMinutes(1));

            appHost = new BasicAppHost()
                .Init();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            appHost.Dispose();
        }

        IUserAuthRepository CreateAuthRepo(IPocoDynamo db)
        {
            var authRepo = new DynamoDbAuthRepository(db);
            authRepo.InitSchema();
            return authRepo;
        }

        [Test]
        public void Does_create_Auth_Tables()
        {
            var db = CreatePocoDynamo();
            var authRepo = CreateAuthRepo(db);
            authRepo.InitSchema();

            db.GetTableNames().PrintDump();

            Assert.That(db.GetTableNames(), Is.EquivalentTo(new[] {
                typeof(Seq).Name,
                typeof(UserAuth).Name,
                typeof(UserAuthDetails).Name,
                typeof(UserAuthRole).Name,
            }));

            var userAuth = AssertTable(db, typeof(UserAuth), "Id");
            AssertIndex(userAuth.GlobalIndexes[0], "UsernameUserAuthIndex", "UserName", "Id");

            var userAuthDetails = AssertTable(db, typeof(UserAuthDetails), "UserAuthId", "Id");
            AssertIndex(userAuthDetails.GlobalIndexes[0], "UserIdUserAuthDetailsIndex", "UserId", "Provider");

            var userAuthRole = AssertTable(db, typeof(UserAuthRole), "UserAuthId", "Id");
            AssertIndex(userAuthRole.LocalIndexes[0], "UserAuthRoleRoleIndex", "UserAuthId", "Role");
            AssertIndex(userAuthRole.LocalIndexes[1], "UserAuthRolePermissionIndex", "UserAuthId", "Permission");
        }

        [Test]
        public void Can_Create_UserAuth()
        {
            var db = CreatePocoDynamo();
            var authRepo = CreateAuthRepo(db);

            authRepo.CreateUserAuth(new UserAuth
            {
                DisplayName = "Credentials",
                FirstName = "First",
                LastName = "Last",
                FullName = "First Last",
                Email = "demis.bellot@gmail.com",
            }, "test");

            authRepo.CreateUserAuth(new UserAuth
            {
                DisplayName = "Credentials",
                FirstName = "First",
                LastName = "Last",
                FullName = "First Last",
                UserName = "mythz",
            }, "test");
        }
    }
}