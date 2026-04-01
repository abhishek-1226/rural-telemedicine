using Microsoft.AspNetCore.Mvc;
using TelemedicineAPI.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    // 3. POST /api/appointments (Book an appointment)
    [HttpPost]
    public IActionResult BookAppointment([FromBody] Appointment request)
    {
        request.Id = MockDb.NextAppointmentId++;
        request.Status = "Pending"; // Always starts as pending
        MockDb.Appointments.Add(request);
        
        return CreatedAtAction(nameof(GetAppointmentStatus), new { id = request.Id }, request);
    }

    // 4. GET /api/appointments/{id} (Check current status)
    [HttpGet("{id}")]
    public IActionResult GetAppointmentStatus(int id)
    {
        var appointment = MockDb.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null) return NotFound("Appointment not found");
        
        // --- 🤖 DOCTOR SIMULATOR HACK START ---
        // If the status is pending, randomly Approve or Reject it for testing purposes
        if (appointment.Status == "Pending")
        {
            var random = new Random();
            // 50% chance to approve, 50% to reject
            appointment.Status = random.Next(0, 2) == 0 ? "Approved" : "Rejected"; 
        }
        // --- 🤖 DOCTOR SIMULATOR HACK END ---

        return Ok(appointment);
    }

    // 5 & 6. PATCH /api/appointments/{id}/reschedule (Update timing)
    // Used for both proactive rescheduling and picking a new slot after rejection
    [HttpPatch("{id}/reschedule")]
    public IActionResult RescheduleAppointment(int id, [FromBody] string newTime)
    {
        var appointment = MockDb.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null) return NotFound("Appointment not found");

        appointment.RequestedTime = newTime;
        appointment.Status = "Pending"; // Reset status for doctor to review the new time

        return Ok(appointment);
    }

    // 7. DELETE /api/appointments/{id} (Cancel appointment)
    [HttpDelete("{id}")]
    public IActionResult CancelAppointment(int id)
    {
        var appointment = MockDb.Appointments.FirstOrDefault(a => a.Id == id);
        if (appointment == null) return NotFound("Appointment not found");

        MockDb.Appointments.Remove(appointment);
        return Ok("Appointment cancelled successfully");
    }
}