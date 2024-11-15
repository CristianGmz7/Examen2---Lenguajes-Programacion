import { examenApi } from "../../../config/api/examenApi";

export const loginAsync = async (form) => {
  try {
    const { data } = await examenApi.post(
      `/auth/login`, form
    );

    return data;

  } catch (error) {
    console.error({error});
    return error?.response?.data;
  }
}