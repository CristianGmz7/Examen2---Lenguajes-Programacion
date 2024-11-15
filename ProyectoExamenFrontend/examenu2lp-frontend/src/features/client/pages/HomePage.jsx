// import {EntriesList} from "../components/EntriesList"
// import { partidas } from "../../../mocks/entries"
 'lucide'

import { useNavigate } from "react-router-dom";

export function HomePage() {
  const navigate = useNavigate();

  const handleNavigate = (page) => {
    if (page === "entriesList") {
      navigate("/entriesList");
    } else if (page === "accountTable") {
      navigate("/accounTable");
    }
  };


  return (
    <div className="container mx-auto px-4 py-8">
      <div className="w-full max-w-4xl mx-auto bg-white shadow-md rounded-lg overflow-hidden">
        <div className="text-center p-6 border-b">
          <h2 className="text-3xl font-bold">Bienvenido al Sistema de Gestión de Partidas Contables</h2>
          <p className="text-lg mt-2">Administra tus cuentas y partidas contables de forma eficiente y organizada</p>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
          <button 
          onClick={() => handleNavigate("accountTable")}
          className="h-20 text-lg flex items-center justify-center border border-gray-300 rounded-lg hover:bg-gray-100">
            📖 Gestionar Cuentas
          </button>
          <button 
          onClick={() => handleNavigate("entriesList")}
          className="h-20 text-lg flex items-center justify-center border border-gray-300 rounded-lg hover:bg-gray-100">
            📄 Gestionar Partidas Contables
          </button>
          <button className="h-20 text-lg flex items-center justify-center border border-gray-300 rounded-lg hover:bg-gray-100">
            ➕ Crear Nueva Cuenta
          </button>
          <button className="h-20 text-lg flex items-center justify-center border border-gray-300 rounded-lg hover:bg-gray-100">
            ➕ Crear Nueva Partida Contable
          </button>
        </div>

        <div className="bg-gray-100 p-6 rounded-lg">
          <h3 className="text-xl font-semibold mb-4">Con este sistema podrás:</h3>
          <ul className="list-disc list-inside space-y-2 pl-4">
            <li>Crear, editar, listar y borrar cuentas contables.</li>
            <li>Crear, editar, listar y borrar partidas contables.</li>
            <li>Visualizar el estado actual de tus cuentas.</li>
          </ul>
        </div>
      </div>
    </div>
    // <EntriesList partidas={partidas}/>

  )
}
