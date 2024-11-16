import { useState } from "react";
import { useNavigate } from "react-router-dom";

export function CreateEntryPage() {
  const navigate = useNavigate();
  const [items, setItems] = useState([
    { accountNumber: "", description: "", debit: 0, credit: 0 },
    { accountNumber: "", description: "", debit: 0, credit: 0 },
  ]);
  const [totalDebit, setTotalDebit] = useState(0);
  const [totalCredit, setTotalCredit] = useState(0);

  const handleAddRow = () => {
    setItems([
      ...items,
      { accountNumber: "", description: "", debit: 0, credit: 0 },
    ]);
  };

  const handleRemoveRow = (index) => {
    if (items.length > 2) {
      const updatedItems = items.filter((_, i) => i !== index);
      setItems(updatedItems);

      // Recalcular totales
      const totalDebit = updatedItems.reduce((sum, item) => sum + item.debit, 0);
      const totalCredit = updatedItems.reduce((sum, item) => sum + item.credit, 0);
      setTotalDebit(totalDebit);
      setTotalCredit(totalCredit);
    } else {
      alert("Debe haber al menos dos filas.");
    }
  };

  const handleInputChange = (index, field, value) => {
    const updatedItems = [...items];
    updatedItems[index][field] = field === "debit" || field === "credit" ? Number(value) : value;
    setItems(updatedItems);

    // Recalcular totales
    const totalDebit = updatedItems.reduce((sum, item) => sum + item.debit, 0);
    const totalCredit = updatedItems.reduce((sum, item) => sum + item.credit, 0);
    setTotalDebit(totalDebit);
    setTotalCredit(totalCredit);
  };

  const handleSaveEntry = () => {
    alert("Partida contable guardada");
    navigate("/entries");
  };

  return (
    <div className="w-full max-w-4xl mx-auto p-4">
      <div className="bg-white shadow-lg rounded-lg">
        <div className="flex justify-between items-center p-4 border-b">
          <h1 className="text-2xl font-bold">Crear Partida Contable</h1>
          <button
            className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 focus:outline-none"
            onClick={() => navigate("/entries")}
          >
            Cancelar
          </button>
        </div>
        {/* Tabla de cuentas */}
        <div className="p-4">
          <table className="w-full">
            <thead>
              <tr>
                <th className="text-left text-sm font-semibold text-gray-700">N° de Cuenta</th>
                <th className="text-left text-sm font-semibold text-gray-700">Descripción</th>
                <th className="text-right text-sm font-semibold text-gray-700">Débito</th>
                <th className="text-right text-sm font-semibold text-gray-700">Crédito</th>
                <th className="text-center text-sm font-semibold text-gray-700">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item, index) => (
                <tr key={index} className="border-t">
                  <td>
                    <input
                      type="text"
                      className="w-full p-2 border rounded"
                      value={item.accountNumber}
                      onChange={(e) =>
                        handleInputChange(index, "accountNumber", e.target.value)
                      }
                    />
                  </td>
                  <td>
                    <input
                      type="text"
                      className="w-full p-2 border rounded"
                      value={item.description}
                      onChange={(e) =>
                        handleInputChange(index, "description", e.target.value)
                      }
                    />
                  </td>
                  <td>
                    <input
                      type="number"
                      className="w-full p-2 border rounded text-right"
                      value={item.debit}
                      onChange={(e) =>
                        handleInputChange(index, "debit", e.target.value)
                      }
                    />
                  </td>
                  <td>
                    <input
                      type="number"
                      className="w-full p-2 border rounded text-right"
                      value={item.credit}
                      onChange={(e) =>
                        handleInputChange(index, "credit", e.target.value)
                      }
                    />
                  </td>
                  <td className="text-center">
                    <button
                      className="px-2 py-1 bg-red-500 text-white rounded hover:bg-red-600"
                      onClick={() => handleRemoveRow(index)}
                    >
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <button
            className="mt-4 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 focus:outline-none"
            onClick={handleAddRow}
          >
            Agregar Cuenta
          </button>
        </div>
        {/* Totales */}
        <div className="p-4">
          <div className="flex justify-between">
            <span className="text-lg font-semibold">Total Débito: {totalDebit}</span>
            <span className="text-lg font-semibold">Total Crédito: {totalCredit}</span>
          </div>
        </div>
        {/* Guardar */}
        <div className="p-4 flex justify-end">
          <button
            className="px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600 focus:outline-none"
            onClick={handleSaveEntry}
          >
            Guardar Partida
          </button>
        </div>
      </div>
    </div>
  );
}
