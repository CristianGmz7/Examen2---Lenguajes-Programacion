import { Navigate, Route, Routes } from "react-router-dom"
import { HomePage } from "../pages/HomePage"
import { Nav } from "../components/Nav"
import { Footer } from "../components/Footer"
import { EntriesList } from "../components/EntriesList"
import { AccountTable } from "../pages/AccountTable"
import { CreateAccountPage } from "../pages/CreateAccountPage"
import { CreateEntryPage } from "../pages/CreateEntryPage"
import { LogPage } from "../pages/LogPage"


export const ClientRouter = () => {
  return (
    <div className="overflow-x-hidden bg-gray-100 w-screen h-screen">
      <Nav/>
      <div className="px-6 py-8">
        <div className="container flex justify-between mx-auto">
          <Routes>
            <Route path="/home" element={<HomePage/>}/>
            <Route path='/*' element={<Navigate to={"/home"} />} />
            <Route path="/entriesList" element={<EntriesList />} />
            <Route path="/accountTable" element={<AccountTable />} />
            <Route path="/createAccountPage" element={<CreateAccountPage/>} />
            <Route path="/createEntryPage" element={<CreateEntryPage/>} />
            <Route path="/viewLog" element={<LogPage/>} />
          </Routes>
        </div>
      </div>
      <Footer/>
    </div>
  )
}
