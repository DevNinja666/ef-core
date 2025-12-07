// Project: UniversityApi
// Files included below. Create a new ASP.NET Core Web API project and replace / add these files.

// appsettings.json (add your connection string)
/*
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UniversityDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
*/

// Program.cs
using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core (SQL Server). Change connection string in appsettings.json.
builder.Services.AddDbContext<UniversityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// -------------------------
// Models/Student.cs
namespace UniversityApi.Models
{
    public class Student
n    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }

        public ICollection<StudentCourse> StudentCourses { get; set; }
            = new List<StudentCourse>();
    }
}

// Models/Course.cs
namespace UniversityApi.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Credit { get; set; }

        public ICollection<StudentCourse> StudentCourses { get; set; }
            = new List<StudentCourse>();
    }
}

// Models/StudentCourse.cs
namespace UniversityApi.Models
{
    public class StudentCourse
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}

// Data/UniversityContext.cs
using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;

namespace UniversityApi.Data
{
    public class UniversityContext : DbContext
    {
        public UniversityContext(DbContextOptions<UniversityContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Name)
                      .HasMaxLength(200);
            });

            // Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                      .HasMaxLength(200);

                // Credit check constraint: Credit > 0
                entity.HasCheckConstraint("CK_Courses_Credit_Positive", "[Credit] > 0");
            });

            // StudentCourse join table (many-to-many)
            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.ToTable("StudentCourses");
                entity.HasKey(sc => new { sc.StudentId, sc.CourseId });

                entity.HasOne(sc => sc.Student)
                      .WithMany(s => s.StudentCourses)
                      .HasForeignKey(sc => sc.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sc => sc.Course)
                      .WithMany(c => c.StudentCourses)
                      .HasForeignKey(sc => sc.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Bonus: seed data
            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, Name = "Alice Ivanova", Email = "alice@example.com", BirthDate = new DateTime(2000, 4, 12) },
                new Student { Id = 2, Name = "Bob Petrov", Email = "bob@example.com", BirthDate = new DateTime(1999, 10, 3) }
            );

            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Title = "Algorithms", Credit = 3 },
                new Course { Id = 2, Title = "Databases", Credit = 4 }
            );

            modelBuilder.Entity<StudentCourse>().HasData(
                new StudentCourse { StudentId = 1, CourseId = 1 },
                new StudentCourse { StudentId = 1, CourseId = 2 },
                new StudentCourse { StudentId = 2, CourseId = 2 }
            );
        }
    }
}

// Controllers/StudentController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly UniversityContext _db;
        public StudentController(UniversityContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _db.Students
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .ToListAsync();
            return Ok(students);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.Students.Add(student);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = student.Id }, student);
        }

        [HttpGet("{id}/courses")]
        public async Task<IActionResult> GetCoursesByStudentId(int id)
        {
            var student = await _db.Students
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();
            var courses = student.StudentCourses.Select(sc => sc.Course);
            return Ok(courses);
        }
    }
}

// Controllers/CourseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly UniversityContext _db;
        public CourseController(UniversityContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] Course course)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.Courses.Add(course);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(AddCourse), new { id = course.Id }, course);
        }

        [HttpPost("{courseId}/assign/{studentId}")]
        public async Task<IActionResult> AssignStudentToCourse(int courseId, int studentId)
        {
            var course = await _db.Courses.FindAsync(courseId);
            var student = await _db.Students.FindAsync(studentId);
            if (course == null || student == null) return NotFound();

            var exists = await _db.StudentCourses.FindAsync(studentId, courseId);
            if (exists != null) return Conflict("Student already assigned to the course.");

            var sc = new StudentCourse { StudentId = studentId, CourseId = courseId };
            _db.StudentCourses.Add(sc);
            await _db.SaveChangesAsync();
            return Ok(sc);
        }
    }
}


*/
