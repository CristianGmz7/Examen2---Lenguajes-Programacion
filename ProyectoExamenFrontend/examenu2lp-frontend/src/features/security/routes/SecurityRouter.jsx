import { Navigate, Route, Routes } from "react-router-dom"
import { Footer } from "../../client/components/Footer"
import { Nav } from "../../client/components/Nav"
import { LoginPage } from "../pages/LoginPage"

export const SecurityRouter = () => {
  return (
    <div className="overflow-x-hidden bg-gray-100 w-screen h-screen">
      <Nav/>
      <div className="px-6 py-8">
        <div className="container flex justify-between mx-auto">
          <Routes>
            <Route path="/login" element={<LoginPage/>}/>
            <Route path='/*' element={<Navigate to={"/security/login"} />} />
          </Routes>
        </div>
      </div>
      <Footer/>
    </div>
  )
}
