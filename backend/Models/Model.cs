using System.Collections.Generic;
using System.Linq;

namespace TelemedicineAPI.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string? Name { get; set; } // Notice the ?
        public string? Specialty { get; set; }
        public List<string>? AvailableSlots { get; set; } = new List<string>();
    }

    public class Patient
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        // --- NEW FIELDS ---
        public int? Age { get; set; }
        public string? Sex { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string? RequestedTime { get; set; } // Notice the ?
        public string? Status { get; set; }      // Notice the ?
        public string? Symptoms { get; set; }    // Notice the ?
    }

    // Keep your MockDb exactly the same below...

    // In-Memory Mock Database
    public static class MockDb
    {
        public static List<Doctor> Doctors = new List<Doctor>
        {
            new Doctor { Id = 1, Name = "Dr. Smith", Specialty = "General Practice", AvailableSlots = new List<string> { "10:00 AM", "02:00 PM" } },
            new Doctor { Id = 2, Name = "Dr. Jones", Specialty = "Pediatrics", AvailableSlots = new List<string> { "09:00 AM", "11:30 AM", "04:00 PM" } }
        };

        
    // Inside your MockDb class, update the mock patient:
        public static List<Patient> Patients = new List<Patient>
        {
            new Patient { 
                Id = 1, 
                Name = "John Doe", 
                Description = "No known allergies.", 
                Age = 30, 
                Sex = "Male", 
                Height = "175 cm", 
                Weight = "75 kg" 
            }
        };

        public static List<Appointment> Appointments = new List<Appointment>();
        public static int NextAppointmentId = 1;
    }
}