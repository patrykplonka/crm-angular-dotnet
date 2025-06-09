using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using crm.Server.Controllers;
using crm.Server.Data;
using crm.Server.Models;
using crm.Server.Models.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class CourseControllerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly CourseController _controller;

    public CourseControllerTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Role, "Student")
        }, "TestAuth"));
        _controller = new CourseController(_mockContext.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    private void SetupMockDbSet<T>(Mock<DbSet<T>> mockSet, List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestDbAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestDbAsyncEnumerator<T>(data.GetEnumerator()));
        mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
        mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(entity => data.Remove(entity));
    }

    [Fact]
    public async Task GetCourses_ReturnsOkWithEnrolledCourses()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course { Id = "1", Title = "Test Course", Instructor = "user123" }
        };
        var enrollments = new List<CourseEnrollment>
        {
            new CourseEnrollment { Id = "e1", UserId = "user123", CourseId = "1" }
        };

        var mockCourses = new Mock<DbSet<Course>>();
        var mockEnrollments = new Mock<DbSet<CourseEnrollment>>();
        SetupMockDbSet(mockCourses, courses);
        SetupMockDbSet(mockEnrollments, enrollments);

        _mockContext.Object.Courses = mockCourses.Object;
        _mockContext.Object.CourseEnrollments = mockEnrollments.Object;
        _mockContext.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var courseDtos = Assert.IsType<List<CourseDto>>(okResult.Value);
        Assert.Single(courseDtos);
        Assert.True(courseDtos[0].Enrolled);
    }
}