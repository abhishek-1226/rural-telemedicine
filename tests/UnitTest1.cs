using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TelemedicineAPI.Models;
using Xunit;

namespace tests;

public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UnitTest1(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ─── DOCTORS ───────────────────────────────────────────

    // User Story: As a rural patient, I want to see available doctors
    [Fact]
    public async Task GetDoctors_ReturnsOkWithDoctorList()
    {
        var response = await _client.GetAsync("/api/doctors");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doctors = await response.Content.ReadFromJsonAsync<List<Doctor>>();
        Assert.NotNull(doctors);
        Assert.True(doctors.Count > 0);
    }

    // User Story: As a patient, I want to see a specific doctor's profile and time slots
    [Fact]
    public async Task GetDoctorById_ValidId_ReturnsDoctor()
    {
        var response = await _client.GetAsync("/api/doctors/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doctor = await response.Content.ReadFromJsonAsync<Doctor>();
        Assert.NotNull(doctor);
        Assert.Equal(1, doctor.Id);
    }

    // User Story: Doctor not found returns 404
    [Fact]
    public async Task GetDoctorById_InvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/doctors/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ─── APPOINTMENTS ──────────────────────────────────────

    // User Story: As a patient, I want to book an appointment online
    [Fact]
    public async Task BookAppointment_ValidRequest_ReturnsCreated()
    {
        var appointment = new Appointment
        {
            DoctorId = 1,
            PatientId = 1,
            RequestedTime = "10:00 AM",
            Symptoms = "Fever and headache"
        };

        var response = await _client.PostAsJsonAsync("/api/appointments", appointment);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Appointment>();
        Assert.NotNull(created);
        Assert.Equal("Pending", created.Status);
        Assert.True(created.Id > 0);
    }

    // User Story: Invalid booking is rejected
    [Fact]
    public async Task BookAppointment_NullBody_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync<Appointment?>("/api/appointments", null);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // User Story: As a patient, I want to check my appointment status
    [Fact]
    public async Task GetAppointmentStatus_ValidId_ReturnsAppointment()
    {
        // First book one
        var appointment = new Appointment
        {
            DoctorId = 1,
            PatientId = 1,
            RequestedTime = "02:00 PM",
            Symptoms = "Cough"
        };
        var postResponse = await _client.PostAsJsonAsync("/api/appointments", appointment);
        var created = await postResponse.Content.ReadFromJsonAsync<Appointment>();

        // Then check its status
        var response = await _client.GetAsync($"/api/appointments/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Appointment>();
        Assert.NotNull(result);
        Assert.True(result.Status == "Approved" || result.Status == "Rejected");
    }

    // User Story: As a patient, I want to reschedule my appointment
    [Fact]
    public async Task RescheduleAppointment_ValidId_ReturnsOkAndResetsPending()
    {
        // Book first
        var appointment = new Appointment
        {
            DoctorId = 2,
            PatientId = 1,
            RequestedTime = "09:00 AM",
            Symptoms = "Back pain"
        };
        var postResponse = await _client.PostAsJsonAsync("/api/appointments", appointment);
        var created = await postResponse.Content.ReadFromJsonAsync<Appointment>();

        // Reschedule it
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/appointments/{created!.Id}/reschedule")
        {
            Content = JsonContent.Create("11:30 AM")
        };
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Appointment>();
        Assert.Equal("Pending", updated!.Status);
        Assert.Equal("11:30 AM", updated.RequestedTime);
    }

    // User Story: As a patient, I want to cancel my appointment
    [Fact]
    public async Task CancelAppointment_ValidId_ReturnsOk()
    {
        // Book first
        var appointment = new Appointment
        {
            DoctorId = 1,
            PatientId = 1,
            RequestedTime = "04:00 PM",
            Symptoms = "Checkup"
        };
        var postResponse = await _client.PostAsJsonAsync("/api/appointments", appointment);
        var created = await postResponse.Content.ReadFromJsonAsync<Appointment>();

        // Cancel it
        var response = await _client.DeleteAsync($"/api/appointments/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ─── PATIENTS ──────────────────────────────────────────

    // User Story: As a patient, I want to view my profile
    [Fact]
    public async Task GetPatient_ValidId_ReturnsPatient()
    {
        var response = await _client.GetAsync("/api/patients/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patient = await response.Content.ReadFromJsonAsync<Patient>();
        Assert.NotNull(patient);
        Assert.Equal(1, patient.Id);
    }

    // User Story: Patient not found returns 404
    [Fact]
    public async Task GetPatient_InvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/patients/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // User Story: As a patient, I want to update my medical details
    [Fact]
    public async Task UpdatePatient_ValidId_ReturnsUpdatedPatient()
    {
        var updatedInfo = new Patient
        {
            Age = 25,
            Sex = "Female",
            Height = "165 cm",
            Weight = "60 kg",
            Description = "Allergic to penicillin"
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/patients/1")
        {
            Content = JsonContent.Create(updatedInfo)
        };
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Patient>();
        Assert.Equal(25, result!.Age);
        Assert.Equal("Female", result.Sex);
    }
}
