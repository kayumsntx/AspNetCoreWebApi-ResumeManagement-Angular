using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApi.Models;
using ResumeApi.Models.DTOs;
using System.Text.Json.Serialization;

namespace ResumeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly IWebHostEnvironment _env;
        public EmployeesController(ServerContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAllEmployees()
        {
            var employees = await _context.Employees.Include(e => e.Experiences)
        .ThenInclude(ex => ex.ExperienceTitle).AsNoTracking().ToListAsync();
            return Ok(employees);
        }
        [HttpGet("titles")]
        public async Task<ActionResult<IEnumerable<ExperienceTitle>>> GetExperienceTitles()
        {
            var titles = await _context.ExperienceTitles.ToListAsync();
            return Ok(titles);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.Include(e => e.Experiences).ThenInclude(ex => ex.ExperienceTitle).FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }
        [HttpPost]
        //{"ExperienceTitleId":1,"Duration":3},{"ExperienceTitleId":2,"Duration":5}
        public async Task<IActionResult> PostEmployee([FromForm] EmployeeDTO dto)
        {
            string fileName = "noimage.png";
            if (dto.ImageFile != null)
            {
                fileName = await SaveImageAsync(dto.ImageFile);
            }
            var employee = new Employee
            {
                EmployeeName = dto.EmployeeName,
                IsActive = dto.IsActive,
                JoinDate = dto.JoinDate,
                ImageName = fileName,
                ImageUrl = "/images/" + fileName
            };
            if (!string.IsNullOrWhiteSpace(dto.EXperiencesString))
            {
                try
                {
                    string rawJson = dto.EXperiencesString.Trim();
                    if (rawJson.StartsWith("{") && rawJson.EndsWith("}"))
                    {
                        rawJson = $"[{rawJson}]";
                    }
                    var options = new System.Text.Json.JsonSerializerOptions
                    {

                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    var experiences = System.Text.Json.JsonSerializer.Deserialize<List<ExperienceDTO>>(rawJson, options);
                    if (experiences != null)
                    {
                        foreach (var item in experiences)
                        {
                            var titleExists = await _context.ExperienceTitles.AnyAsync(t => t.ExperienceTitleId == item.ExperienceTitleId);
                            if (!titleExists)
                            {
                                return BadRequest("Experience title not exists");
                            }
                            employee.Experiences.Add(new Experience
                            {
                                ExperienceTitleId = item.ExperienceTitleId,
                                Duration = item.Duration
                            });
                        }
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    return BadRequest($"JSON Parsing Error: {ex.Message}. Received: {dto.EXperiencesString}");
                }
            }
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(employee);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, [FromForm] EmployeeDTO dto)
        {
            if (id == 0) return BadRequest();
            if (id != dto.EmployeeId) return BadRequest();
            var existingEmployee = await _context.Employees.Include(e => e.Experiences).ThenInclude(ex => ex.ExperienceTitle).FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (existingEmployee == null) return NotFound();
            string oldImageUrl = existingEmployee.ImageUrl;
            string fileName = existingEmployee.ImageName;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                fileName = await SaveImageAsync(dto.ImageFile);
                oldImageUrl = "/images/" + fileName;
                var imagePath = Path.Combine(_env.WebRootPath, "images", existingEmployee.ImageName ?? "");
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            existingEmployee.EmployeeId = id;
            existingEmployee.ImageName = fileName;
            existingEmployee.ImageUrl = oldImageUrl;
            existingEmployee.EmployeeName = dto.EmployeeName;
            existingEmployee.IsActive = dto.IsActive;
            existingEmployee.JoinDate = dto.JoinDate;
            var experiences = existingEmployee.Experiences;
            _context.Experiences.RemoveRange(experiences);

            if (!string.IsNullOrWhiteSpace(dto.EXperiencesString))
            {
                try
                {
                    string rawJson = dto.EXperiencesString.Trim();
                    if (rawJson.StartsWith("{") && rawJson.EndsWith("}"))
                    {
                        rawJson = $"[{rawJson}]";
                    }
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    var newExperiences = System.Text.Json.JsonSerializer.Deserialize<List<ExperienceDTO>>(rawJson, options);
                    if (newExperiences != null)
                    {
                        foreach (var item in newExperiences)
                        {
                            var titleExists = await _context.ExperienceTitles.AnyAsync(t => t.ExperienceTitleId == item.ExperienceTitleId);
                            if (!titleExists)
                            {
                                return BadRequest("Experience title not exists");
                            }
                            existingEmployee.Experiences.Add(new Experience
                            {
                                EmployeeId = id,
                                ExperienceTitleId = item.ExperienceTitleId,
                                Duration = item.Duration
                            });
                        }
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    return BadRequest($"JSON Parsing Error: {ex.Message}. Received: {dto.EXperiencesString}");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(e => e.EmployeeId == id)) return NotFound();
                throw;
            }

            return NoContent();
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadDir = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return fileName;
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            var imagePath = Path.Combine(_env.WebRootPath, "images", employee.ImageName ?? "");
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
