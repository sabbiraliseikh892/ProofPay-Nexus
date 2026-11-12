import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7040/api",
  headers: {
    Accept: "application/json",
  },
});

export default api;
