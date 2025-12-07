

using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;

public class EfCoreQueries
{
    private readonly UniversityAdvancedDbContext _context;

    public EfCoreQueries(UniversityAdvancedDbContext context)
    {
        _context = context;
    }

    public async Task Task1_Student_Profile()
    {
        
        var studentsWithProfiles = await _context.Students
            .Include(s => s.Profile)
            .ToListAsync();

  
        var student = await _context.Students.FirstAsync();
        await _context.Entry(student).Reference(s => s.Profile).LoadAsync();
    }

    public async Task Task2_Student_Enrollments_Courses()
    {
       
        var studentsWithCourses = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .ToListAsync();


        var s = await _context.Students.FirstAsync();
        await _context.Entry(s).Collection(st => st.Enrollments).Query().Include(e => e.Course).LoadAsync();
    }

    public async Task Task3_Instructor_Courses()
    {
       
        var instructorsWithCourses = await _context.Instructors
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
            .ToListAsync();

       
        var inst = await _context.Instructors.FirstAsync();
        await _context.Entry(inst).Collection(i => i.CourseAssignments).Query().Include(ca => ca.Course).LoadAsync();
    }

    public async Task Task4_Instructor_OfficeAssignment()
    {
        
        var instructorsWithOffice = await _context.Instructors.Include(i => i.OfficeAssignment).ToListAsync();

     
        var instructor = await _context.Instructors.FirstAsync();
        await _context.Entry(instructor).Reference(i => i.OfficeAssignment).LoadAsync();
    }

    public async Task Task5_Course_Department()
    {
        // Eager Loading
        var coursesWithDepartment = await _context.Courses.Include(c => c.Department).ToListAsync();

      
        var course = await _context.Courses.FirstAsync();
        await _context.Entry(course).Reference(c => c.Department).LoadAsync();
    }

    public async Task Task6_Exam_Course()
    {
     
        var examsWithCourse = await _context.Exams.Include(e => e.Course).ToListAsync();

       
        var exam = await _context.Exams.FirstAsync();
        await _context.Entry(exam).Reference(e => e.Course).LoadAsync();
    }

    public async Task Task7_ExamResult_Exam_Student()
    {
    
        var results = await _context.ExamResults.Include(er => er.Exam).Include(er => er.Student).ToListAsync();

       
        var res = await _context.ExamResults.FirstAsync();
        await _context.Entry(res).Reference(er => er.Exam).LoadAsync();
        await _context.Entry(res).Reference(er => er.Student).LoadAsync();
    }

    public async Task Task8_Department_Instructors()
    {
       
        var departments = await _context.Departments.Include(d => d.Instructors).ToListAsync();


        var dept = await _context.Departments.FirstAsync();
        await _context.Entry(dept).Collection(d => d.Instructors).LoadAsync();
    }

    public async Task Task9_Student_Enrollments_Courses_Exams()
    {
   
        var students = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                    .ThenInclude(c => c.Exams)
            .ToListAsync();

  
        var st = await _context.Students.FirstAsync();
        await _context.Entry(st).Collection(s => s.Enrollments).Query().Include(e => e.Course).ThenInclude(c => c.Exams).LoadAsync();
    }

    public async Task Task10_Course_Exam_ExamResult_Student()
    {
       
        var courses = await _context.Courses
            .Include(c => c.Exams)
                .ThenInclude(ex => ex.ExamResults)
                    .ThenInclude(er => er.Student)
            .ToListAsync();

    
        var c = await _context.Courses.FirstAsync();
        await _context.Entry(c).Collection(c => c.Exams).Query().Include(ex => ex.ExamResults).ThenInclude(er => er.Student).LoadAsync();
    }

    public async Task Task11_Instructor_Course_Exam()
    {
     
        var instructors = await _context.Instructors
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
                    .ThenInclude(c => c.Exams)
            .ToListAsync();

    
        var i1 = await _context.Instructors.FirstAsync();
        await _context.Entry(i1).Collection(i => i.CourseAssignments).Query().Include(ca => ca.Course).ThenInclude(c => c.Exams).LoadAsync();
    }

    public async Task Task12_Students_With_Exam_For_Course(int courseId)
    {
    
        var students = await _context.Students
            .Where(s => s.ExamResults.Any(er => er.Exam.CourseId == courseId))
            .Include(s => s.ExamResults.Where(er => er.Exam.CourseId == courseId))
                .ThenInclude(er => er.Exam)
            .ToListAsync();

        var st = await _context.Students.FirstAsync();
        await _context.Entry(st).Collection(s => s.ExamResults).Query().Where(er => er.Exam.CourseId == courseId).Include(er => er.Exam).LoadAsync();
    }

    public async Task Task13_Courses_Without_Exams()
    {
        var courses = await _context.Courses.Include(c => c.Exams).Where(c => !c.Exams.Any()).ToListAsync();

       
        var c = await _context.Courses.FirstAsync();
        await _context.Entry(c).Collection(c => c.Exams).LoadAsync();
    }

    public async Task Task14_Students_With_ExamCount_GreaterThan3()
    {
   
        var students = await _context.Students
            .Include(s => s.ExamResults)
            .Where(s => s.ExamResults.Count > 3)
            .ToListAsync();

  
        var st = await _context.Students.FirstAsync();
        await _context.Entry(st).Collection(s => s.ExamResults).LoadAsync();
    }

    public async Task Task15_Instructors_Without_CourseAssignments()
    {
   
        var instructors = await _context.Instructors
            .Include(i => i.CourseAssignments)
            .Where(i => !i.CourseAssignments.Any())
            .ToListAsync();

        var instr = await _context.Instructors.FirstAsync();
        await _context.Entry(instr).Collection(i => i.CourseAssignments).LoadAsync();
    }
}
