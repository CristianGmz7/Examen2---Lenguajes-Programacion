import { useNavigate } from "react-router-dom";

export function AccountTable() {
  const accounts = [
    { accountNumber: "001", name: "Caja", month: "Noviembre", year: 2024, balance: 5000 },
    { accountNumber: "002", name: "Bancos", month: "Noviembre", year: 2024, balance: 15000 },
    { accountNumber: "003", name: "Clientes", month: "Noviembre", year: 2024, balance: 7000 },
    { accountNumber: "004", name: "Acreedores", month: "Noviembre", year: 2024, balance: 12000 },
  ];

  const navigate = useNavigate();
  
  const handleAddAccount = () => {
    navigate("/createAccountPage");
  };

  return (
    <div className="w-full max-w-4xl mx-auto p-4">
      <div className="bg-white shadow-lg rounded-lg">
        {/* Header con título y botón */}
        <div className="flex justify-between items-center p-4 border-b">
          <h1 className="text-2xl font-bold">Saldo de las cuentas</h1>
          <button
            className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 focus:outline-none"
            onClick={handleAddAccount}
          >
            Agregar Cuenta
          </button>
        </div>
        {/* Tabla */}
        <div className="p-4">
          <table className="table-auto w-full text-left border-collapse border border-gray-200">
            <thead className="bg-gray-100">
              <tr>
                <th className="p-2 border border-gray-200">Número de cuenta</th>
                <th className="p-2 border border-gray-200">Nombre</th>
                <th className="p-2 border border-gray-200">Mes</th>
                <th className="p-2 border border-gray-200">Año</th>
                <th className="p-2 border border-gray-200 text-right">Saldo</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account, index) => (
                <tr key={index} className="hover:bg-gray-50">
                  <td className="p-2 border border-gray-200">{account.accountNumber}</td>
                  <td className="p-2 border border-gray-200">{account.name}</td>
                  <td className="p-2 border border-gray-200">{account.month}</td>
                  <td className="p-2 border border-gray-200">{account.year}</td>
                  <td className="p-2 border border-gray-200 text-right">{account.balance}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
