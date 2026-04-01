import { useState, useEffect } from 'react';

// Make sure this port matches your running C# backend!
const API_BASE_URL = "http://localhost:5121/api"; 
const PATIENT_ID = 1;

export default function App() {
  // Navigation State
  const [view, setView] = useState('doctors'); // 'doctors', 'booking', 'status', 'profile'
  
  // Data State
  const [doctors, setDoctors] = useState([]);
  const [selectedDoctor, setSelectedDoctor] = useState(null);
  const [appointment, setAppointment] = useState(null);
  const [patient, setPatient] = useState({ name: '', age: '', sex: '', height: '', weight: '', description: '' });

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newTimeInput, setNewTimeInput] = useState("");

  // 1. App Load: Fetch doctors & check memory for existing appointments
  useEffect(() => {
    fetch(`${API_BASE_URL}/doctors`)
      .then(res => res.json())
      .then(data => setDoctors(data))
      .catch(err => console.error("Failed to load doctors:", err));

    const savedApptId = localStorage.getItem('appointmentId');
    if (savedApptId) {
      checkStatus(savedApptId);
    }
  }, []);

  // 2. Select Doctor
  const handleSelectDoctor = async (doctorId) => {
    const res = await fetch(`${API_BASE_URL}/doctors/${doctorId}`);
    const data = await res.json();
    setSelectedDoctor(data);
    setView('booking');
  };

  // 3. Book Appointment
  const bookAppointment = async (e) => {
    e.preventDefault();
    const payload = {
      doctorId: selectedDoctor.id,
      patientId: PATIENT_ID,
      requestedTime: e.target.timeSlot.value,
      symptoms: e.target.symptoms.value
    };

    const res = await fetch(`${API_BASE_URL}/appointments`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });
    const newAppt = await res.json();
    
    localStorage.setItem('appointmentId', newAppt.id); // Save to memory
    setAppointment(newAppt);
    setView('status');
  };

  // 4. Check Appointment Status
  const checkStatus = async (idToFetch) => {
    const id = idToFetch || appointment?.id;
    if (!id) return;

    try {
      const res = await fetch(`${API_BASE_URL}/appointments/${id}`);
      if (!res.ok) throw new Error("Appointment not found");
      const data = await res.json();
      setAppointment(data);
      setView('status');
    } catch (err) {
      localStorage.removeItem('appointmentId');
      setAppointment(null);
      setView('doctors');
    }
  };

  // 5. Reschedule Logic (Custom Modal)
  const openRescheduleModal = () => {
    setNewTimeInput(""); 
    setIsModalOpen(true); 
  };

  const submitNewTime = async () => {
    if (!newTimeInput) return; 

    const res = await fetch(`${API_BASE_URL}/appointments/${appointment.id}/reschedule`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(newTimeInput)
    });
    
    const updatedAppt = await res.json();
    setAppointment(updatedAppt);
    setIsModalOpen(false); 
  };

  // 6. Cancel Appointment
  const cancelAppointment = async () => {
    await fetch(`${API_BASE_URL}/appointments/${appointment.id}`, { method: "DELETE" });
    localStorage.removeItem('appointmentId');
    setAppointment(null);
    setView('doctors');
  };

  // 7. Load Profile
  const loadProfile = async () => {
    const res = await fetch(`${API_BASE_URL}/patients/${PATIENT_ID}`);
    const data = await res.json();
    setPatient(data);
    setView('profile');
  };

  // 8. Save Profile
  const saveProfile = async (e) => {
    e.preventDefault();
    await fetch(`${API_BASE_URL}/patients/${PATIENT_ID}`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(patient)
    });
    alert("Profile saved successfully!");
    setView(appointment ? 'status' : 'doctors');
  };

  return (
    <div className="min-h-screen bg-gray-50 text-gray-800 font-sans p-6">
      <div className="max-w-3xl mx-auto">
        
        {/* Navigation Bar */}
        <nav className="flex justify-between items-center bg-white p-4 rounded-xl shadow-sm mb-6">
          <h1 className="text-2xl font-bold text-blue-600">Patient Portal</h1>
          <div className="space-x-4">
            <button onClick={() => setView(appointment ? 'status' : 'doctors')} className="text-gray-600 hover:text-blue-600 font-medium transition">Home</button>
            <button onClick={loadProfile} className="text-gray-600 hover:text-blue-600 font-medium transition">My Profile</button>
          </div>
        </nav>

        {/* VIEW: Doctors List */}
        {view === 'doctors' && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {doctors.map(doc => (
              <div key={doc.id} className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col items-start">
                <h3 className="text-xl font-semibold">{doc.name}</h3>
                <p className="text-blue-500 mb-4">{doc.specialty}</p>
                <button 
                  onClick={() => handleSelectDoctor(doc.id)}
                  className="mt-auto w-full bg-blue-50 text-blue-600 font-semibold py-2 rounded-lg hover:bg-blue-100 transition"
                >
                  Book Appointment
                </button>
              </div>
            ))}
          </div>
        )}

        {/* VIEW: Booking Form */}
        {view === 'booking' && selectedDoctor && (
          <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-100">
            <h2 className="text-2xl font-bold mb-4">Book with {selectedDoctor.name}</h2>
            <form onSubmit={bookAppointment} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Select Time</label>
                <select name="timeSlot" className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500">
                  {selectedDoctor.availableSlots?.map(slot => (
                    <option key={slot} value={slot}>{slot}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Symptoms</label>
                <input required name="symptoms" type="text" placeholder="Briefly describe your issue..." className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div className="flex space-x-3 pt-2">
                <button type="submit" className="flex-1 bg-blue-600 text-white font-semibold py-2.5 rounded-lg hover:bg-blue-700 transition">Request Appointment</button>
                <button type="button" onClick={() => setView('doctors')} className="flex-1 bg-gray-100 text-gray-700 font-semibold py-2.5 rounded-lg hover:bg-gray-200 transition">Cancel</button>
              </div>
            </form>
          </div>
        )}

        {/* VIEW: Status Dashboard */}
        {view === 'status' && appointment && (
          <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-100 text-center space-y-4">
            <h2 className="text-2xl font-bold">Request Status</h2>
            
            <div className={`inline-block px-4 py-2 rounded-full font-bold text-sm ${
              appointment.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' :
              appointment.status === 'Approved' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
            }`}>
              {appointment.status.toUpperCase()}
            </div>
            
            <p className="text-gray-600 text-lg">Requested Time: <strong>{appointment.requestedTime}</strong></p>
            
            {appointment.status === 'Rejected' && (
              <p className="text-red-500 font-medium">The doctor rejected this time. Please pick another slot.</p>
            )}

            <div className="flex justify-center space-x-3 pt-4">
              {appointment.status === 'Pending' && <button onClick={() => checkStatus()} className="bg-blue-50 text-blue-600 px-4 py-2 rounded-lg hover:bg-blue-100 font-medium transition">Refresh Status</button>}
              {(appointment.status === 'Approved' || appointment.status === 'Rejected') && <button onClick={openRescheduleModal} className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 font-medium transition">Update Timing</button>}
              <button onClick={cancelAppointment} className="bg-red-50 text-red-600 px-4 py-2 rounded-lg hover:bg-red-100 font-medium transition">Cancel Request</button>
            </div>
          </div>
        )}

        {/* VIEW: My Profile */}
        {view === 'profile' && (
          <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-100">
            <h2 className="text-2xl font-bold mb-6">My Medical Profile</h2>
            <form onSubmit={saveProfile} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Age</label>
                  <input type="number" value={patient.age || ''} onChange={e => setPatient({...patient, age: e.target.value})} className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Sex</label>
                  <select value={patient.sex || ''} onChange={e => setPatient({...patient, sex: e.target.value})} className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500">
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Height</label>
                  <input type="text" value={patient.height || ''} onChange={e => setPatient({...patient, height: e.target.value})} placeholder="e.g. 175cm" className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Weight</label>
                  <input type="text" value={patient.weight || ''} onChange={e => setPatient({...patient, weight: e.target.value})} placeholder="e.g. 70kg" className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Medical History</label>
                <textarea rows="3" value={patient.description || ''} onChange={e => setPatient({...patient, description: e.target.value})} className="w-full border border-gray-300 rounded-lg p-2.5 outline-none focus:ring-2 focus:ring-blue-500"></textarea>
              </div>
              <button type="submit" className="w-full bg-blue-600 text-white font-semibold py-2.5 rounded-lg hover:bg-blue-700 transition">Save Profile</button>
            </form>
          </div>
        )}

        {/* --- CUSTOM MODAL --- */}
        {isModalOpen && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 px-4">
            <div className="bg-white p-6 rounded-2xl shadow-xl w-full max-w-sm">
              <h3 className="text-xl font-bold text-gray-800 mb-2">Reschedule Appointment</h3>
              <p className="text-sm text-gray-500 mb-4">Enter a new time below to send a request to your doctor.</p>
              
              <input 
                type="text" 
                value={newTimeInput}
                onChange={(e) => setNewTimeInput(e.target.value)}
                placeholder="e.g., 10:00 AM" 
                className="w-full border border-gray-300 rounded-lg p-3 mb-6 outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
                autoFocus
              />
              
              <div className="flex justify-end space-x-3">
                <button 
                  onClick={() => setIsModalOpen(false)} 
                  className="px-4 py-2 text-gray-600 font-medium hover:bg-gray-100 rounded-lg transition"
                >
                  Cancel
                </button>
                <button 
                  onClick={submitNewTime} 
                  className="px-4 py-2 bg-blue-600 text-white font-medium hover:bg-blue-700 rounded-lg transition"
                >
                  Confirm Time
                </button>
              </div>
            </div>
          </div>
        )}

      </div>
    </div>
  );
}