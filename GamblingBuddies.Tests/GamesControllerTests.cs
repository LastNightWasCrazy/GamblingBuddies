using GamblingBuddies.Controllers;
using GamblingBuddies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Tests
{
    public class GamesControllerTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Index_ReturnsViewWithGames()
        {
            var context = CreateContext();

            context.GameCategoryDictionaries.Add(new GameCategoryDictionary
            {
                GameCategoryId = 1,
                Name = "Cards",
                Description = "Gry karciane"
            });

            context.Games.Add(new Game
            {
                GameId = 1,
                Name = "Poker",
                Description = "Poker test",
                GameCategoryId = 1,
                IsActive = true
            });

            context.SaveChanges();

            var controller = new GamesController(context);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Game>>(viewResult.Model);

            Assert.Single(model);
        }

        [Fact]
        public void Index_ReturnsEmptyCollection_WhenNoGamesExist()
        {
            var context = CreateContext();

            var controller = new GamesController(context);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Game>>(viewResult.Model);

            Assert.Empty(model);
        }

        [Fact]
        public void Index_ReturnsActiveGame()
        {
            var context = CreateContext();

            context.GameCategoryDictionaries.Add(new GameCategoryDictionary
            {
                GameCategoryId = 1,
                Name = "Roulette",
                Description = "Ruletka"
            });

            context.Games.Add(new Game
            {
                GameId = 1,
                Name = "European Roulette",
                Description = "Test",
                GameCategoryId = 1,
                IsActive = true
            });

            context.SaveChanges();

            var controller = new GamesController(context);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Game>>(viewResult.Model);

            Assert.Contains(model, g => g.Name == "European Roulette");
        }
    }
}