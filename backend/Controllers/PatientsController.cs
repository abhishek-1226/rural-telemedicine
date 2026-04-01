using Microsoft.AspNetCore.Mvc;
using TelemedicineAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    // GET /api/patients/{id} (Loads the profile data)
    [HttpGet("{id}")]
    public IActionResult GetPatient(int id)
    {
        var patient = MockDb.Patients.FirstOrDefault(p => p.Id == id);
        if (patient == null) return NotFound("Patient not found");
        return Ok(patient);
    }

    // PATCH /api/patients/{id} (Saves the updated info)
    [HttpPatch("{id}")]
    public IActionResult UpdatePatientInfo(int id, [FromBody] Patient updatedInfo)
    {
        var patient = MockDb.Patients.FirstOrDefault(p => p.Id == id);
        if (patient == null) return NotFound("Patient not found");

        // Update the fields if the frontend sent them
        if (updatedInfo.Description != null) patient.Description = updatedInfo.Description;
        if (updatedInfo.Age.HasValue) patient.Age = updatedInfo.Age;
        if (updatedInfo.Sex != null) patient.Sex = updatedInfo.Sex;
        if (updatedInfo.Height != null) patient.Height = updatedInfo.Height;
        if (updatedInfo.Weight != null) patient.Weight = updatedInfo.Weight;
        
        return Ok(patient);
    }
}