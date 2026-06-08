'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import API from '@/services/api';
import Link from 'next/link';
import dynamic from 'next/dynamic';

interface AppliedJob {
  id: string;
  status: string;
  appliedAt: string;
  resumeUrl: string;
  job: {
    id: string;
    title: string;
    category: string;
    salaryRange: string;
  } | null;
}

function JobSeekerDashboardBase() {
  const router = useRouter();
  const [applications, setApplications] = useState<AppliedJob[]>([]);
  const [loading, setLoading] = useState(true);
  const [userName] = useState(() => {
    if (typeof window === 'undefined') return '';
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;
    return user?.fullName ?? '';
  });

  useEffect(() => {
    const userStr = localStorage.getItem('user');
    const token = localStorage.getItem('token');
    
    // টোকেন বা ইউজার না থাকলে লগইন পেজে পাঠিয়ে দেবে
    if (!userStr || !token) {
      router.push('/login');
      return;
    }
    
    const user = JSON.parse(userStr);
    if (user.role !== 'JobSeeker') {
      alert('Access Denied. Employer cannot access Seeker Dashboard.');
      router.push('/');
      return;
    }

    const fetchMyApplications = async () => {
      try {
        const response = await API.get('/applications/my-applications');
        setApplications(response.data);
      } catch (error) {
        console.error('Error fetching applications:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMyApplications();
  }, []);


  const handleLogout = () => {
    localStorage.clear();
    router.push('/login');
  };

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'hired': return 'bg-green-100 text-green-800 border-green-200';
      case 'rejected': return 'bg-red-100 text-red-800 border-red-200';
      case 'interviewing': return 'bg-amber-100 text-amber-800 border-amber-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-6">
      {/* Header */}
      <div className="max-w-5xl mx-auto flex justify-between items-center mb-8 bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Welcome back, {userName}!</h1>
          <p className="text-sm text-gray-500 mt-1">Track your remote job applications and interview status</p>
        </div>
        <div className="flex gap-3">
          <Link href="/" className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 font-medium text-sm transition">
            Browse Jobs
          </Link>
          <button onClick={handleLogout} className="bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-white px-4 py-2 rounded-md hover:bg-gray-200 font-medium text-sm transition">
            Logout
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-5xl mx-auto bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6">
        <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-6">Your Applications</h2>

        {loading ? (
          <p className="text-center text-gray-500 py-6">Loading applications...</p>
        ) : applications.length === 0 ? (
          <div className="text-center py-12 border-2 border-dashed border-gray-200 rounded-xl">
            <p className="text-gray-500 text-base">You haven&apos;t applied for any jobs yet.</p>
            <Link href="/" className="text-blue-500 hover:underline mt-2 inline-block font-medium">
              Find your dream remote job now ↗
            </Link>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b dark:border-gray-700 text-gray-600 dark:text-gray-400 text-sm">
                  <th className="pb-3 font-semibold">Job Title</th>
                  <th className="pb-3 font-semibold">Category</th>
                  <th className="pb-3 font-semibold">Salary Range</th>
                  <th className="pb-3 font-semibold">Applied Date</th>
                  <th className="pb-3 font-semibold text-center">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y dark:divide-gray-700">
                {applications.map((app) => (
                  <tr key={app.id} className="text-gray-800 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-750 transition">
                    <td className="py-4 font-medium text-gray-950 dark:text-white">
                      {app.job ? app.job.title : 'Software Engineer'}
                    </td>
                    <td className="py-4 text-sm">
                      <span className="bg-blue-50 text-blue-700 px-2 py-0.5 rounded text-xs dark:bg-blue-900 dark:text-blue-200">
                        {app.job ? app.job.category : 'Remote'}
                      </span>
                    </td>
                    <td className="py-4 text-sm font-medium text-green-600">
                      {app.job ? app.job.salaryRange : 'N/A'}
                    </td>
                    <td className="py-4 text-sm text-gray-500">
                      {new Date(app.appliedAt).toLocaleDateString()}
                    </td>
                    <td className="py-4 text-center">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold border ${getStatusColor(app.status)}`}>
                        {app.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

const JobSeekerDashboard = dynamic(() => Promise.resolve(JobSeekerDashboardBase), { ssr: false });
export default JobSeekerDashboard;