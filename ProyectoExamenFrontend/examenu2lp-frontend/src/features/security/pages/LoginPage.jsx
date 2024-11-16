import { useState } from "react";
import { FaArrowRight } from "react-icons/fa"
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../store";
import { useFormik } from "formik";
import { loginInitValues, loginValidationSchema } from "../forms/login.data";

export const LoginPage = () => {

  const handleSubmit = () => {

  }

  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const login = useAuthStore((state) => state.login);
  const error = useAuthStore((state) => state.error);
  const message = useAuthStore((state) => state.message);

  
  const formik = useFormik({
    initialValues: loginInitValues,
    validationSchema: loginValidationSchema,
    validateOnChange: true,
    onSubmit: async (formValues) => {
      // console.log(formValues);
      setLoading(true);
      await login(formValues);
      setLoading(false);
    },
  });


  return (

    

    <div className="p-10 xs:p-0 mx-auto md:w-full md:max-w-md my-4 ">
    <h1 className="font-bold text-center text-2xl mb-5 text-unah-blue">
      Iniciar Sesión
    </h1>
    <div className="bg-white shadow w-full rounded-lg divide-gray-200 divide-y">
      {
        error 
        ? (
          <span className="p-4 block bg-red-500 text-white text-center rounded-t-lg">
            {message}
            {/* como puedo producir un error */}
          </span>
        ) 
        : (
          ""
        )
      }
    </div>
    <div className="bg-white shadow text-sm rounded-lg divide-y divide-gray-200">
      <form className="px-5 py-7" onSubmit={formik.handleSubmit}>
      {/* <form className="px-5 py-7" onSubmit={handleSubmit}> */}
        <div className="mb-4">
          <label
            className="font-semibold text-sm text-gray-600 pb-1 block"
            htmlFor="email"
          >
            Correo electrónico
          </label>
          <input
            type="email"
            id="email"
            name="email"
            value={formik.values.email}
            onChange={formik.handleChange}
            className=" border rounded-lg px-3 py-2 mt-1 mb-5 text-sm w-full"
          />

          {formik.touched.email && formik.errors.email && (
            <div className="text-red-500 text-xs mb-2">
              {formik.errors.email}
            </div>
          )}
        </div>

        <div className="mb-4">
          <label
            className="font-semibold text-sm text-gray-600 pb-1 block"
            htmlFor="password"
          >
            Contraseña
          </label>
          <input
            type="password"
            id="password"
            name="password"
            value={formik.values.password}
            onChange={formik.handleChange}
            className=" border rounded-lg px-3 py-2 mt-1 mb-5 text-sm w-full"
          />

          {formik.touched.password && formik.errors.password && (
            <div className="text-red-500 text-xs mb-2">
              {formik.errors.password}
            </div>
          )}
        </div>

        <button
          type="submit"
          className=" transition duration-200 bg-blue-500 hover:bg-blue-300 focus:bg-blue-300 focus:shadow-sm focus:ring-4 text-white w-full py-2.5 rounded-lg text-sm shadow-sm hover:shadow-md font-semibold text-center inline-block"
        >
          <span className=" inline-block mr-2">Ingresar</span>
          <FaArrowRight className="w-4 h-4 inline-block" />
        </button>
      </form>
    </div>
  </div>
  )
}
