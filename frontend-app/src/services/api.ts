import axios from 'axios';

// Axios ইনস্ট্যান্স তৈরি করা এবং লাইভ রেন্ডার ব্যাকএন্ড এপিআই লিংক যুক্ত করা
const API = axios.create({
  baseURL: 'https://remotejobboard-ik8o.onrender.com/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// রিকোয়েস্ট ইন্টারসেপ্টর: প্রতিবার এপিআই কলের সময় টোকেন থাকলে তা অটোমেটিক হেডারে যুক্ত করবে
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