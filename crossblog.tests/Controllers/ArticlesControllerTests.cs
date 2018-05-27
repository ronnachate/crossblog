using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossblog.Controllers;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Newtonsoft.Json;

namespace crossblog.tests.Controllers
{
    public class ArticlesControllerTests
    {
        private ArticlesController _articlesController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();

        public ArticlesControllerTests()
        {
            _articlesController = new ArticlesController(_articleRepositoryMock.Object);
        }

        [Fact]
        public async Task Search_ReturnsEmptyList()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _articleRepositoryMock.Setup(m => m.Query()).Returns(articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search("Invalid");

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull(content);

            Assert.Equal(0, content.Articles.Count());
        }

        [Fact]
        public async Task Search_ReturnsList()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _articleRepositoryMock.Setup(m => m.Query()).Returns(articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search("Title");

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull(content);

            Assert.Equal(3, content.Articles.Count());
        }

        [Fact]
        public async Task Get_NotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task Get_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleModel;
            Assert.NotNull(content);

            Assert.Equal("Title1", content.Title);
        }

        [Fact]
        public async Task CreateWithValidData()
        {
            var artitleModel = new ArticleModel
            {
                Title = "new title",
                Content = "new content",
                Date = DateTime.Now,
                Published = false
            };
            // Act
            var result = await _articlesController.Post(artitleModel);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);
            var content = objectResult.Value as Article;
            Assert.NotNull(content);

            Assert.Equal("new title", content.Title);
        }

        [Fact]
        public async Task CreateWithInvalidData()
        {
            var artitleModel = new ArticleModel
            {
                Content = "new content",
                Date = DateTime.Now,
                Published = false
            };
            _articlesController.ModelState.AddModelError("validArticle", "validArticle");
            // Act
            var result = await _articlesController.Post(artitleModel);
            // Assert
            Assert.NotNull(result);
            var objectResult = result as BadRequestObjectResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task UpdateItemData()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var beforeUpdate = objectResult.Value as ArticleModel;
            Assert.NotNull(beforeUpdate);

            var artitleModel = new ArticleModel
            {
                Title = "new title",
                Content = "new content"
            };
            result = await _articlesController.Put(1, artitleModel);
            Assert.NotNull(result);
            //update success
            objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);
             // Act
            result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var afterUpdate = objectResult.Value as ArticleModel;
            Assert.NotNull(afterUpdate);
            Assert.Equal( artitleModel.Title, afterUpdate.Title);
            Assert.Equal( artitleModel.Content, afterUpdate.Content);
        }

        [Fact]
        public async Task Delete()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var obj = objectResult.Value as ArticleModel;
            Assert.NotNull(obj);
            result = await _articlesController.Delete(1);
            // Assert
            Assert.NotNull(result);
            var deleteResult = (OkResult)result;
             Assert.NotNull(deleteResult);

        }
    }
}
