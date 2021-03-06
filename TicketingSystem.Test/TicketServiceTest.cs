﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntityFrameworkCoreMock;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Helpers;
using TicketingSystem.Core.Services;
using TicketingSystem.Database.Context;
using TicketingSystem.Database.Entities;
using TicketingSystem.Database.Repositories;

namespace TicketingSystem.Test
{
    [TestFixture]
    public class TicketServiceTest
    {
        private TicketService _service;
        private DbContextMock<DatabaseContext> _dbContextMock;
        private Mock<UserManager<User>> _userManagerMock;

        private DbContextOptions<DatabaseContext> DummyOptions { get; } =
            new DbContextOptionsBuilder<DatabaseContext>().Options;

        [SetUp]
        public void Initialize()
        {

            _dbContextMock = new DbContextMock<DatabaseContext>(DummyOptions);
            InitializeMockDbSet();
            InitializeMockUserManager();

            var repository = new TicketRepository(_dbContextMock.Object);
            var convertor = new TicketConverter(new PriorityRepository(_dbContextMock.Object),
                new ServiceTypeRepository(_dbContextMock.Object),
                new StatusRepository(_dbContextMock.Object),
                new TicketTypeRepository(_dbContextMock.Object),
                new UserRepository(_dbContextMock.Object, _userManagerMock.Object));

            _service = new TicketService(repository, convertor);
        }

        private void InitializeMockUserManager()
        {
            List<User> users = new List<User>
            {
                new User {UserName = "user1", Email = "user1@mail.com", Id = "1"},
                new User {UserName = "user2", Email = "user2@mail.com", Id = "2"}
            };

            var store = new Mock<IUserStore<User>>();
            _userManagerMock =
                new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _userManagerMock.Object.UserValidators.Add(new UserValidator<User>());
            _userManagerMock.Object.PasswordValidators.Add(new PasswordValidator<User>());

            _userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<User, string>((x, y) => users.Add(x));
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        }

        private void InitializeMockDbSet()
        {
            var users = new[]
            {
                new User {Id = "1", UserName = "user"}
            };
            var priority = new[]
            {
                new Priority {Id = "1", Name = "priority"},
            };
            var serviceType = new[]
            {
                new ServiceType {Id = "1", Name = "serviceType"},
            };
            var ticketType = new[]
            {
                new TicketType {Id = "1", Name = "ticketType"},
            };
            var status = new[]
            {
                new Status {Id = "1", Name = "status"},
            };
            var tickets = GetFakeData();

            var dbSetMock = _dbContextMock.CreateDbSetMock(x => x.Tickets, tickets);
            var dbUserSetMock = _dbContextMock.CreateDbSetMock(x => x.Users, users);
            var dbPrioritySetMock = _dbContextMock.CreateDbSetMock(x => x.Priorities, priority);
            var dbServiceTypeSetMock = _dbContextMock.CreateDbSetMock(x => x.ServiceTypes, serviceType);
            var dbTicketTypeSetMock = _dbContextMock.CreateDbSetMock(x => x.TicketTypes, ticketType);
            var dbStatusSetMock = _dbContextMock.CreateDbSetMock(x => x.Statuses, status);
        }

        [Test]
        public void GetAllTickets()
        {
            var result = _service.GetAll();
            var count = result.Count();

            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetTicket()
        {
            var result = _service.GetById("1");
            var actual = GetFakeData()[0];

            Assert.AreEqual(result.Subject, actual.Subject);
        }

        [Test]
        public void AddTicket()
        {
            _service.Add(new TicketDto
            {
                Id = "3",
                Subject = "name3",
                ServiceTypeName = "serviceType",
                OpenDateTime = DateTime.Now.ToString(),
                CustomerName = "customer",
                Description = "description",
                PriorityName = "priority",
                StatusName = "status",
                TicketTypeName = "ticketType",
                UserName = "user"
            });
            _service.Save();

            var result = _service.GetAll();
            var count = result.Count();

            Assert.AreEqual(3, count);
        }

        [Test]
        public void EditTicket()
        {
            _service.Edit(new TicketDto
            {
                Id = "1",
                Subject = "newSubject",
                ServiceTypeName = "serviceType",
                OpenDateTime = DateTime.Now.ToString(),
                CustomerName = "customer",
                Description = "description",
                PriorityName = "priority",
                StatusName = "status",
                TicketTypeName = "ticketType",
                UserName = "user"
            });
            _service.Save();

            var result = _service.GetById("1");

            Assert.AreEqual("newSubject", result.Subject);
        }

        private Ticket[] GetFakeData()
        {
            return new[]
            {
                new Ticket {Id = "1", Subject = "name1",
                    ServiceType = new ServiceType{Id = "service1", Name = "servicetype1"},
                    User = new User{Id = "1"},
                    Priority = new Priority{ Id = "1", Name = "priority1"},
                    Status = new Status{Id = "1", Name = "status1"},
                    TicketType = new TicketType{Id = "1", Name = "ticket type1"},
                    CustomerName = "customer",
                    Description = "description",
                    OpenDateTime = DateTime.Now
                },
                new Ticket {Id = "2", Subject = "name2",
                    ServiceType = new ServiceType{Id = "service2", Name = "servicetype2"},
                    User = new User{Id = "2"},
                    Priority = new Priority{ Id = "2", Name = "priority2"},
                    Status = new Status{Id = "2", Name = "status1"},
                    TicketType = new TicketType{Id = "2", Name = "ticket type2"},
                    CustomerName = "customer",
                    Description = "description",
                    OpenDateTime = DateTime.Now
                }
            };
        }
    }
}
