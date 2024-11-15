import axios from "axios";

const API_URL = 'https://localhost:7196';
axios.defaults.baseURL = API_URL;

//setAuthToken creo que si iria

//getAuthToken creo que si iria

//setAuthToken(); //esta linea tambien iria creo

const examenApi = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json"
  },
});

export {
  examenApi,
  API_URL
}