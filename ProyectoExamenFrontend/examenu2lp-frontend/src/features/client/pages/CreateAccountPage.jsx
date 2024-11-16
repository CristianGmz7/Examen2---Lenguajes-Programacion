import { useState } from "react";

export function CreateAccountPage() {
  const existingAccounts = [
    { accountNumber: "001", name: "Caja", type: "Debe" },
    { accountNumber: "002", name: "Bancos", type: "Debe" },
    { accountNumber: "003", name: "Clientes", type: "Debe" },
  ];

  const [showAccounts, setShowAccounts] = useState(false);
  const [selectedAccount, setSelectedAccount] = useState(null);

  const handleAccountClick = (account) => {
    setSelectedAccount(account);
  };

  return (
    <div className="w-full max-w-3xl mx-auto p-4">
      <h1 className="text-3xl font-bold text-center mb-6">Agregar Nueva Cuenta</h1>
      <div className="flex flex-col space-y-4">
        <button
          className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600"
          onClick={() => setShowAccounts(!showAccounts)}
        >
          {showAccounts ? "Ocultar" : "Mostrar"} Cuentas Existentes
        </button>
        {showAccounts && (
          <ul className="bg-gray-100 p-4 rounded-lg shadow">
            {existingAccounts.map((account, index) => (
              <li
                key={index}
                className="border-b py-2 hover:cursor-pointer hover:bg-gray-200"
                onClick={() => handleAccountClick(account)}
              >
                No. Cuenta: {account.accountNumber} - Nombre: {account.name} - Tipo: {account.type}
              </li>
            ))}
          </ul>
        )}
        {selectedAccount && (
          <div className="p-4 mt-4 bg-blue-50 border border-blue-200 rounded-lg shadow">
            <h2 className="text-xl font-semibold">Cuenta Seleccionada</h2>
            <p><strong>No. Cuenta:</strong> {selectedAccount.accountNumber}</p>
            <p><strong>Nombre:</strong> {selectedAccount.name}</p>
            <p><strong>Tipo:</strong> {selectedAccount.type}</p>
          </div>
        )}
        <div className="flex items-center space-x-4 mt-6">
          <input
            type="text"
            placeholder="Nombre de la cuenta"
            className="w-1/2 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring"
          />
        </div>
        <div className="flex justify-end mt-4">
          <button className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">
            Crear Cuenta
          </button>
        </div>
      </div>
    </div>
  );
}
