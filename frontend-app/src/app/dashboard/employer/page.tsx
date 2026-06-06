'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import API from '@/services/api';
import dynamic from 'next/dynamic';

interface Job {
  id: string;
  title: string;
  description: string;
  category: string;
  salaryRange: string;
  requirements: string;
}

interface Applicant {
  id: string;
  resumeUrl: string;
  coverLetter: string;
  status: string;
  appliedAt: string;
  applicant: { fullName: string; email: string };
}

function EmployerDashboardBase() {
  const router = useRouter();
  const [myJobs, setMyJobs] = useState<Job[]>([]);
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null);
  const [applicants, setApplicants] = useState<Applicant[]>([]);
  
  // Job Form State
  const [newJob, setNewJob] = useState({ title: '', description: '', category: 'Remote', salaryRange: '', requirements: '' });
  const [loading, setLoading] = useState(false);
  
  // Edit Mode State
  const [isEditing, setIsEditing] = useState(false);
  const [editJobId, setEditJobId] = useState<string | null>(null);

  useEffect(() => {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      router.push('/login');
      return;
    }
    const user = JSON.parse(userStr);
    if (user.role !== 'Employer') {
      alert('Access Denied. Seeker cannot access Employer Dashboard.');
      router.push('/');
      return;
    }

    fetchEmployerJobs();
  }, []);

  const fetchEmployerJobs = async () => {
    try {
      const response = await API.get('/jobs');
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const filteredJobs = response.data.filter((j: any) => j.employerId === user.id);
      setMyJobs(filteredJobs);
    } catch (error) {
      console.error(error);
    }
  };

  const viewApplicants = async (jobId: string) => {
    setSelectedJobId(jobId);
    try {
      const response = await API.get(`/applications/job/${jobId}`);
      setApplicants(response.data);
    } catch (error) {
      alert('Failed to fetch applicants.');
    }
  };

  const handleStatusChange = async (appId: string, newStatus: string) => {
    try {
      await API.patch(`/applications/${appId}/status`, { status: newStatus });
      alert(`Status updated to ${newStatus}`);
      if (selectedJobId) viewApplicants(selectedJobId);
    } catch (error) {
      alert('Failed to update status.');
    }
  };

  // ১. ক্রিয়েট এবং আপডেট হ্যান্ডেলার (Create & Update Logic)
  const handleSubmitJob = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (isEditing && editJobId) {
        // এডিট মোড চালু থাকলে PUT রিকোয়েস্ট যাবে
        await API.put(`/jobs/${editJobId}`, newJob);
        alert('Job updated successfully!');
      } else {
        // নতুন জব হলে POST রিকোয়েস্ট যাবে
        await API.post('/jobs', newJob);
        alert('Job posted successfully!');
      }
      
      // ফর্ম রিসেট করা
      setNewJob({ title: '', description: '', category: 'Remote', salaryRange: '', requirements: '' });
      setIsEditing(false);
      setEditJobId(null);
      fetchEmployerJobs();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Failed to submit job.');
    } finally {
      setLoading(false);
    }
  };

  // ২. এডিট মোড অ্যাক্টিভেট করা
  const startEdit = (job: Job) => {
    setIsEditing(true);
    setEditJobId(job.id);
    setNewJob({
      title: job.title,
      description: job.description,
      category: job.category,
      salaryRange: job.salaryRange,
      requirements: job.requirements || '',
    });
  };

  // ৩. জব ডিলিট করার লজিক (Delete Logic)
  const handleDeleteJob = async (jobId: string) => {
    if (!confirm('Are you sure you want to delete this job?')) return;

    try {
      await API.delete(`/jobs/${jobId}`);
      alert('Job deleted successfully!');
      fetchEmployerJobs();
      if (selectedJobId === jobId) {
        setSelectedJobId(null);
        setApplicants([]);
      }
    } catch (error) {
      alert('Failed to delete job.');
    }
  };

  const handleLogout = () => {
    localStorage.clear();
    router.push('/login');
  };

  return (
    <div className="min-h-screen bg-gray-100 dark:bg-gray-900 p-6">
      <div className="max-w-6xl mx-auto flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Employer Dashboard</h1>
        <button onClick={handleLogout} className="bg-red-500 text-white px-4 py-2 rounded-md hover:bg-red-600">
          Logout
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* কলাম ১: ফর্ম (পোস্ট অথবা এডিট) */}
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm h-fit">
          <h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">
            {isEditing ? '✏️ Edit Job Opening' : '🚀 Post a New Remote Job'}
          </h2>
          <form onSubmit={handleSubmitJob} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Job Title</label>
              <input type="text" required className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white" value={newJob.title} onChange={(e) => setNewJob({ ...newJob, title: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Category</label>
              <select className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white" value={newJob.category} onChange={(e) => setNewJob({ ...newJob, category: e.target.value })}>
                <option value="Remote">Remote</option>
                <option value="Hybrid">Hybrid</option>
                <option value="Full-time">Full-time</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Salary Range</label>
              <input type="text" required placeholder="e.g., $40k - $60k / Year" className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white" value={newJob.salaryRange} onChange={(e) => setNewJob({ ...newJob, salaryRange: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Description</label>
              <textarea rows={3} required className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white" value={newJob.description} onChange={(e) => setNewJob({ ...newJob, description: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Requirements</label>
              <textarea rows={2} className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white" value={newJob.requirements} onChange={(e) => setNewJob({ ...newJob, requirements: e.target.value })} />
            </div>
            <div className="flex gap-2">
              <button type="submit" disabled={loading} className="flex-1 bg-blue-600 text-white py-2 rounded-md font-medium hover:bg-blue-700 disabled:bg-blue-400">
                {loading ? 'Processing...' : isEditing ? 'Save Changes' : 'Post Job'}
              </button>
              {isEditing && (
                <button type="button" className="bg-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-400" onClick={() => {
                  setIsEditing(false);
                  setNewJob({ title: '', description: '', category: 'Remote', salaryRange: '', requirements: '' });
                }}>
                  Cancel
                </button>
              )}
            </div>
          </form>
        </div>

        {/* কলাম ২: আপনার জব লিস্ট (এডিট ও ডিলিট বাটনসহ) */}
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm">
          <h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">Your Job Openings</h2>
          {myJobs.length === 0 ? <p className="text-gray-500">You haven't posted any jobs yet.</p> : (
            <div className="space-y-4">
              {myJobs.map((job) => (
                <div key={job.id} className="p-4 border rounded-md dark:border-gray-700 bg-gray-50 dark:bg-gray-750 space-y-3">
                  <div>
                    <h4 className="font-semibold text-gray-900 dark:text-white text-base">{job.title}</h4>
                    <p className="text-xs text-gray-500 mt-0.5">{job.category} | {job.salaryRange}</p>
                  </div>
                  
                  {/* বাটন্স প্যানেল */}
                  <div className="flex gap-2 pt-1 border-t dark:border-gray-700 justify-between">
                    <button onClick={() => viewApplicants(job.id)} className="text-xs bg-blue-50 text-blue-600 hover:bg-blue-600 hover:text-white px-2 py-1 rounded font-medium transition">
                      Applicants ({selectedJobId === job.id ? applicants.length : 'View'})
                    </button>
                    <div className="flex gap-1.5">
                      <button onClick={() => startEdit(job)} className="text-xs bg-amber-50 text-amber-600 hover:bg-amber-500 hover:text-white px-2 py-1 rounded font-medium transition">
                        Edit
                      </button>
                      <button onClick={() => handleDeleteJob(job.id)} className="text-xs bg-red-50 text-red-600 hover:bg-red-500 hover:text-white px-2 py-1 rounded font-medium transition">
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* কলাম ৩: অ্যাপ্লিকেন্ট ট্র্যাকিং */}
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm">
          <h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">Applicant Tracking System</h2>
          {!selectedJobId ? <p className="text-gray-500">Select a job opening to view applicants.</p> : applicants.length === 0 ? <p className="text-gray-500">No one has applied for this job yet.</p> : (
            <div className="space-y-4">
              {applicants.map((app) => (
                <div key={app.id} className="p-4 border rounded-md dark:border-gray-700 space-y-2">
                  <h4 className="font-semibold text-gray-900 dark:text-white">{app.applicant.fullName}</h4>
                  <p className="text-xs text-gray-500">{app.applicant.email}</p>
                  <p className="text-xs italic text-gray-600 dark:text-gray-400">" {app.coverLetter || 'No cover letter submitted'} "</p>
                  
                  <div className="flex justify-between items-center pt-2">
                    <a href={app.resumeUrl} target="_blank" rel="noopener noreferrer" className="text-xs text-blue-500 hover:underline font-medium">
                      View Resume ↗
                    </a>
                    <select value={app.status} onChange={(e) => handleStatusChange(app.id, e.target.value)} className="text-xs border rounded px-2 py-1 bg-gray-50 dark:bg-gray-700 dark:text-white">
                      <option value="Pending">Pending</option>
                      <option value="Interviewing">Interviewing</option>
                      <option value="Hired">Hired</option>
                      <option value="Rejected">Rejected</option>
                    </select>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

const EmployerDashboard = dynamic(() => Promise.resolve(EmployerDashboardBase), { ssr: false });
export default EmployerDashboard;