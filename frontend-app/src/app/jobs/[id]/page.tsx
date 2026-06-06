'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import API from '@/services/api';

interface JobDetails {
  id: string;
  title: string;
  description: string;
  category: string;
  salaryRange: string;
  requirements: string;
  employer: { fullName: string; email: string };
}

export default function JobDetailsPage() {
  const { id } = useParams();
  const router = useRouter();
  const [job, setJob] = useState<JobDetails | null>(null);
  const [resumeUrl, setResumeUrl] = useState('');
  const [coverLetter, setCoverLetter] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    API.get(`/jobs/${id}`)
      .then((res) => setJob(res.data))
      .catch((err) => console.error(err));
  }, [id]);

  const handleApply = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);

    try {
      // আমরা ব্যাকএন্ডের /api/applications/apply এন্ডপয়েন্টে ডাটা পাঠাচ্ছি
      await API.post('/applications/apply', { jobId: id, resumeUrl, coverLetter });
      alert('Application submitted successfully!');
      router.push('/');
    } catch (error: unknown) {
      const axiosError = error as {
        response?: { data?: { message?: string } };
      };
      alert(axiosError.response?.data?.message || 'Failed to apply. Make sure you are logged in as a JobSeeker.');
    } finally {
      setSubmitting(false);
    }
  };

  if (!job) return <p className="text-center py-10">Loading job details...</p>;

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 py-10 px-4">
      <div className="max-w-3xl mx-auto bg-white dark:bg-gray-800 p-8 rounded-xl shadow-md">
        <h1 className="text-3xl font-bold text-gray-950 dark:text-white">{job.title}</h1>
        <p className="text-blue-600 font-medium mt-2">Posted by: {job.employer?.fullName} ({job.employer?.email})</p>
        
        <div className="flex gap-4 mt-4 mb-6">
          <span className="bg-blue-100 text-blue-800 px-3 py-1 rounded text-sm">{job.category}</span>
          <span className="bg-green-100 text-green-800 px-3 py-1 rounded text-sm">{job.salaryRange}</span>
        </div>

        <div className="mb-6">
          <h3 className="text-xl font-bold mb-2 text-gray-900 dark:text-white">Job Description</h3>
          <p className="text-gray-700 dark:text-gray-300 whitespace-pre-line">{job.description}</p>
        </div>

        <div className="mb-8">
          <h3 className="text-xl font-bold mb-2 text-gray-900 dark:text-white">Requirements</h3>
          <p className="text-gray-700 dark:text-gray-300 whitespace-pre-line">{job.requirements}</p>
        </div>

        {/* Application Form */}
        <div className="border-t pt-6">
          <h3 className="text-xl font-bold mb-4 text-gray-900 dark:text-white">Apply for this Position</h3>
          <form onSubmit={handleApply} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Resume URL (Google Drive / Dropbox Link)</label>
              <input
                type="url" required
                placeholder="https://drive.google.com/..."
                className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white"
                value={resumeUrl}
                onChange={(e) => setResumeUrl(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Cover Letter</label>
              <textarea
                rows={4}
                placeholder="Write your cover letter here..."
                className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white"
                value={coverLetter}
                onChange={(e) => setCoverLetter(e.target.value)}
              />
            </div>
            <button
              type="submit" disabled={submitting}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-md font-medium disabled:bg-blue-400"
            >
              {submitting ? 'Submitting Application...' : 'Submit Application'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}