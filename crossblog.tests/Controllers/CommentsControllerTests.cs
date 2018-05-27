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
    public class CommentsControllerTests
    {
        private ArticlesController _articlesController;
        private CommentsController _commentsController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();
        private Mock<ICommentRepository> _commentRepositoryMock = new Mock<ICommentRepository>();

        public CommentsControllerTests()
        {
            _commentsController = new CommentsController(_articleRepositoryMock.Object, _commentRepositoryMock.Object);
        }

        [Fact]
        public async Task GetCommentsInArticle()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            var commentDbSetMock = Builder<Comment>.CreateListOfSize(3).All().With(c => c.ArticleId = 1).Build().ToAsyncDbSetMock();
            _commentRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);

            // Act
            var result = await _commentsController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentListModel;
            Assert.NotNull(content);

            Assert.Equal(3, content.Comments.Count());
        }

        [Fact]
        public async Task Get_ArticleNotFound()
        {
            // Act
            var result = await _commentsController.Get(2);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task GetCommentById()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            _commentRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Comment>(Builder<Comment>.CreateNew().With(c => c.ArticleId = 1).Build()));

            // Act
            var result = await _commentsController.Get(1, 1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var comment = objectResult.Value as CommentModel;
            Assert.NotNull(comment);

            Assert.Equal("Title1", comment.Title);
        }

        [Fact]
        public async Task GetCommentWithWrongArticleId()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            _commentRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Comment>(Builder<Comment>.CreateNew().With(c => c.ArticleId = 1).Build()));

            // Act
            var result = await _commentsController.Get(2, 1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task CreateWithValidData()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));
            var commentModel = new CommentModel
            {
                Email = "new email",
                Title = "new title",
                Content = "new content",
                Date = DateTime.UtcNow,
                Published = true
            };
            // Act
            var result = await _commentsController.Post(1, commentModel);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);
            var content = objectResult.Value as CommentModel;
            Assert.NotNull(content);

            Assert.Equal("new title", content.Title);
        }

    }
}
