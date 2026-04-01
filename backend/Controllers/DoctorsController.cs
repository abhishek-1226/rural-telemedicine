using Microsoft.AspNetCore.Mvc;
using TelemedicineAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    // 1. GET /api/doctors
    [HttpGet]
    public IActionResult GetDoctors()
    {
        return Ok(MockDb.Doctors);
    }

    // 2. GET /api/doctors/{id}
    [HttpGet("{id}")]
    public IActionResult GetDoctor(int id)
    {
        var doctor = MockDb.Doctors.FirstOrDefault(d => d.Id == id);
        if (doctor == null) return NotFound("Doctor not found");
        
        return Ok(doctor);
    }
}