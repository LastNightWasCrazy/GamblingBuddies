using GamblingBuddies.Controllers;
using GamblingBuddies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Tests
{
    public class GameTablesControllerTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Index_ReturnsViewWithGameTables()
        {
            var context = CreateContext();

            context.Halls.Add(new Hall
            {
                HallId = 1,
                Name = "Main Hall",
                Description = "Test",
                HallTypeId = 1,
                IsActive = true
            });

            context.GameTables.Add(new GameTable
            {
                GameTableId = 1,
                HallId = 1,
                TableNumber = 101,
                MinPlayers = 1,
                MaxPlayers = 6,
                IsActive = true
            });

            context.SaveChanges();

            var controller = new GameTablesController(context);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<GameTable>>(viewResult.Model);

            Assert.Single(model);
        }

        [Fact]
        public void Details_ReturnsNotFound_WhenTableDoesNotExist()
        {
            var context = CreateContext();
            var controller = new GameTablesController(context);

            var result = controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Details_ReturnsView_WhenTableExists()
        {
            var context = CreateContext();

            context.Halls.Add(new Hall
            {
                HallId = 1,
                Name = "VIP Hall",
                Description = "Test",
                HallTypeId = 1,
                IsActive = true
            });

            context.GameTables.Add(new GameTable
            {
                GameTableId = 1,
                HallId = 1,
                TableNumber = 201,
                MinPlayers = 1,
                MaxPlayers = 5,
                IsActive = true
            });

            context.SaveChanges();

            var controller = new GameTablesController(context);

            var result = controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameTable>(viewResult.Model);

            Assert.Equal(201, model.TableNumber);
        }
    }
}