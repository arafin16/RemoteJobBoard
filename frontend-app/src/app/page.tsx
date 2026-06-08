'use client';

import dynamic from 'next/dynamic';
import { useEffect, useState } from 'react';
import API from '@/services/api';
import Link from 'next/link';

interface Job {
  id: string;
  title: string;
  description: string;
  category: string;
  salaryRange: string;
  createdAt: string;
}

function HomeBase() {
  const [jobs, setJobs] = useState<Job[]>([]);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [loading, setLoading] = useState(true);
  
  // Auth States
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userRole, setUserRole] = useState<'JobSeeker' | 'Employer' | ''>('');

  // ১. পেজ লোড হওয়ার সময় লগইন স্ট্যাটাস চেক করা
  useEffect(() => {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    
    if (token && userStr) {
      setIsLoggedIn(true);
      const user = JSON.parse(userStr);
      setUserRole(user.role);
    }
  }, []);

  // ২. প্রপার লগআউট হ্যান্ডেলার
  const handleLogout = () => {
    localStorage.clear(); // ব্রাউজারের টোকেন ও ইউজার ডাটা মুছে ফেলা
    setIsLoggedIn(false);
    setUserRole('');
  };

  // ব্যাকএন্ড থেকে চাকরির লিস্ট নিয়ে আসার ফাংশন
  const fetchJobs = async () => {
    setLoading(true);
    try {
      const response = await API.get('/jobs', {
        params: { search, category }
      });
      setJobs(response.data);
    } catch (error) {
      console.error('Error fetching jobs:', error);
    } finally {
      setLoading(false);
    }
  };

  // ক্যাটাগরি ও সার্চ চেঞ্জ হলে অটোমেটিক রিলোড হবে
  useEffect(() => {
    const loadJobs = async () => {
      setLoading(true);
      try {
        const response = await API.get('/jobs', {
          params: { search, category }
        });
        setJobs(response.data);
      } catch (error) {
        console.error('Error fetching jobs:', error);
      } finally {
        setLoading(false);
      }
    };
    loadJobs();
  }, [category, search]);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Navbar */}
      <nav className="bg-white dark:bg-gray-800 shadow-sm px-6 py-4 flex justify-between items-center">
        <Link href="/">
          <h1 className="text-2xl font-bold text-blue-600 cursor-pointer">RemoteJobBoard</h1>
        </Link>
        
        <div className="flex items-center space-x-4">
          {isLoggedIn ? (
            <>
              {/* ইউজার যদি JobSeeker হয় তবে "Your Applications" বাটন দেখাবে */}
              {userRole === 'JobSeeker' && (
                <Link href="/dashboard/seeker" className="text-sm font-semibold text-blue-600 hover:text-blue-700 bg-blue-50 px-4 py-2 rounded-md transition">
                  💼 Your Applications
                </Link>
              )}

              {/* ইউজার যদি Employer হয় তবে "Employer Dashboard" বাটন দেখাবে */}
              {userRole === 'Employer' && (
                <Link href="/dashboard/employer" className="text-sm font-semibold text-blue-600 hover:text-blue-700 bg-blue-50 px-4 py-2 rounded-md transition">
                  🚀 Employer Dashboard
                </Link>
              )}

              {/* লগআউট বাটন */}
              <button 
                onClick={handleLogout} 
                className="text-sm font-medium text-gray-600 dark:text-gray-300 hover:text-red-600 transition"
              >
                Logout
              </button>
            </>
          ) : (
            <>
              {/* ইউজার লগইন না থাকলে ডিফল্ট বাটনগুলো দেখাবে */}
              <Link href="/login" className="text-gray-600 dark:text-gray-300 hover:text-blue-600 font-medium">Login</Link>
              <Link href="/register" className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 font-medium transition">Register</Link>
            </>
          )}
        </div>
      </nav>

      {/* Search & Filter Section */}
      <div className="max-w-6xl mx-auto px-4 py-8">
        <div className="flex flex-col md:flex-row gap-4 mb-8">
          <input
            type="text"
            placeholder="Search by Job Title or Keywords..."
            className="flex-1 px-4 py-2 border rounded-md dark:bg-gray-800 dark:text-white"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <select
            className="px-4 py-2 border rounded-md dark:bg-gray-800 dark:text-white"
            value={category}
            onChange={(e) => setCategory(e.target.value)}
          >
            <option value="">All Categories</option>
            <option value="Remote">Remote</option>
            <option value="Hybrid">Hybrid</option>
            <option value="Full-time">Full-time</option>
          </select>
          <button
            onClick={fetchJobs}
            className="bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700"
          >
            Search
          </button>
        </div>

        {/* Job Cards */}
        <h2 className="text-xl font-bold text-gray-800 dark:text-white mb-4">Latest Remote Jobs</h2>
        
        {loading ? (
          <p className="text-center text-gray-500">Loading jobs...</p>
        ) : jobs.length === 0 ? (
          <p className="text-center text-gray-500">No jobs found.</p>
        ) : (
          <div className="grid gap-4">
            {jobs.map((job) => (
              <div key={job.id} className="p-6 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-100 dark:border-gray-700 flex justify-between items-center">
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white">{job.title}</h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{job.description.substring(0, 150)}...</p>
                  <div className="flex gap-4 mt-3 text-xs font-medium text-gray-600 dark:text-gray-400">
                    <span className="bg-blue-50 text-blue-700 px-2.5 py-0.5 rounded-full dark:bg-blue-900 dark:text-blue-200">{job.category}</span>
                    <span className="bg-green-50 text-green-700 px-2.5 py-0.5 rounded-full dark:bg-green-900 dark:text-green-200">{job.salaryRange}</span>
                  </div>
                </div>
                <Link href={`/jobs/${job.id}`} className="bg-gray-100 hover:bg-blue-600 hover:text-white text-gray-700 px-4 py-2 rounded-md font-medium text-sm transition">
                  View Details
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

// হাইড্রেশন এরর ফিক্স করার জন্য ডাইনামিক এক্সপোর্ট
const HomePage = dynamic(() => Promise.resolve(HomeBase), { ssr: false });
export default HomePage;