import { Navigate, Route, Routes } from "react-router-dom"
import { HomePage } from "../pages/HomePage"
import { Nav } from "../components/Nav"
import { Footer } from "../components/Footer"

export const ClientRouter = () => {
  return (
    <div className="overflow-x-hidden bg-gray-100 w-screen h-screen">
      <Nav/>
      <div className="px-6 py-8">
        <div className="container flex justify-between mx-auto">
          <Routes>
            <Route path="/home" element={<HomePage/>}/>
            <Route path='/*' element={<Navigate to={"/home"} />} />
          </Routes>
        </div>
      </div>
      <Footer/>
    </div>
  )
}
