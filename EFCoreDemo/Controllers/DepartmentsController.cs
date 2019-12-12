using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EFCoreDemo.Models;

namespace EFCoreDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ContosouniversityContext _context;

        public DepartmentsController(ContosouniversityContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartment()
        {
            return await _context.Department.Where(d => d.IsDeleted == false).ToListAsync();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Department
                .FirstOrDefaultAsync(d => d.DepartmentId == id && d.IsDeleted == false);

            if (department == null)
            {
                return NotFound();
            }

            return department;
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest();
            }

            var rowVersions = await _context.Department.FromSqlRaw(
                "EXEC [dbo].[Department_Update] {0},{1},{2},{3},{4},{5}",
                department.DepartmentId,
                department.Name,
                department.Budget,
                department.StartDate,
                department.InstructorId,
                department.RowVersion).Select(d => d.RowVersion).ToListAsync();

            if (rowVersions.FirstOrDefault() == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Departments
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            var departmentIds = await _context.Department.FromSqlRaw(
                "EXEC [dbo].[Department_Insert] {0},{1},{2},{3}",
                department.Name,
                department.Budget,
                department.StartDate,
                department.InstructorId).Select(d => d.DepartmentId).ToListAsync();

            department.DepartmentId = departmentIds.FirstOrDefault();

            return CreatedAtAction("GetDepartment", new { id = department.DepartmentId }, department);
        }

        //DELETE: api/Departments/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<Department>> DeleteDepartment(int id)
        {
            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            await _context.Database.ExecuteSqlRawAsync(
               "EXEC [dbo].[Department_Delete] {0}, {1}",
               department.DepartmentId,
               department.RowVersion);

            return department;
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Department>> HideDepartment(int id)
        {
            var department = await _context.Department.FirstOrDefaultAsync(d => d.DepartmentId == id && d.IsDeleted == false);

            if (department == null)
            {
                return NotFound();
            }

            department.IsDeleted = true;
            _context.Department.Update(department);
            await _context.SaveChangesAsync();

            return department;
        }

        // GET: api/Departments/CourseCounts
        [HttpGet("CourseCounts")]
        public async Task<ActionResult<IEnumerable<VwDepartmentCourseCount>>> GetDepartmentCourseCounts()
        {
            return await _context.VwDepartmentCourseCount
                .FromSqlRaw("SELECT * FROM vwDepartmentCourseCount")
                .ToListAsync();
        }

        // GET: api/Departments/CourseCounts/5
        [HttpGet("CourseCounts/{id}")]
        public async Task<ActionResult<VwDepartmentCourseCount>> GetDepartmentCourseCounts(int id)
        {
            var courseCounts = await _context.VwDepartmentCourseCount
                .FromSqlRaw("SELECT *  FROM vwDepartmentCourseCount WHERE DepartmentID = {0}", id)
                .ToListAsync();

            var courseCount = courseCounts.FirstOrDefault();
            if (courseCount == null)
            {
                return NotFound();
            }

            return courseCount;
        }
    }
}
