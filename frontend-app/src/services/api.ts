import axios from 'axios';

// ব্যাকএন্ডের বেস ইউআরএল সেট করা
const API = axios.create({
  baseURL: 'http://localhost:5265/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// প্রতিটা রিকোয়েস্ট পাঠানোর আগে অটোমেটিক JWT Token যুক্ত করার মিডলওয়্যার
API.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

export default API;