'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import API from '@/services/api';
import Link from 'next/link';

export default function RegisterPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({ fullName: '', email: '', password: '', role: 'JobSeeker' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await API.post('/auth/register', formData);
      alert('Registration successful! Please login.');
      router.push('/login'); // সাকসেস হলে লগইন পেজে নিয়ে যাবে
    } catch (err) {
      const error = err as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 px-4">
      <div className="max-w-md w-full space-y-8 p-8 bg-white dark:bg-gray-800 rounded-xl shadow-md">
        <h2 className="text-center text-3xl font-extrabold text-gray-900 dark:text-white">Create your account</h2>
        
        {error && <p className="text-red-500 text-sm text-center bg-red-100 p-2 rounded">{error}</p>}

        <form className="mt-8 space-y-4" onSubmit={handleSubmit}>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Full Name</label>
            <input
              type="text" required
              className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white"
              onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Email Address</label>
            <input
              type="email" required
              className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white"
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Password</label>
            <input
              type="password" required
              className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white"
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Join As a</label>
            <select
              className="mt-1 block w-full px-3 py-2 border rounded-md dark:bg-gray-700 dark:text-white"
              onChange={(e) => setFormData({ ...formData, role: e.target.value })}
            >
              <option value="JobSeeker">Job Seeker (চাকরি প্রার্থী)</option>
              <option value="Employer">Employer (কোম্পানি/মালিক)</option>
            </select>
          </div>
          <button
            type="submit" disabled={loading}
            className="w-full py-2 px-4 bg-blue-600 hover:bg-blue-700 text-white rounded-md font-medium disabled:bg-blue-400"
          >
            {loading ? 'Registering...' : 'Sign Up'}
          </button>
        </form>
        <p className="text-center text-sm text-gray-600 dark:text-gray-400">
          Already have an account? <Link href="/login" className="text-blue-500 hover:underline">Login here</Link>
        </p>
      </div>
    </div>
  );
}