import axios from 'axios';


const API = axios.create({
  baseURL: 'https://remotejobboard-ik8o.onrender.com',
  headers: {
    'Content-Type': 'application/json',
  },
});


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