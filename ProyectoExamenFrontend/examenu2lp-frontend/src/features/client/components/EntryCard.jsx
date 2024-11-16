export function EntryCard({ entry }) {
  return (
    <div className="border rounded-lg shadow p-4">
      {/* Encabezado */}
      <div className="pb-4">
        <h2 className="text-lg font-semibold">Partida #{entry.id}</h2>
        <p className="text-sm text-gray-500">{entry.date}</p>
      </div>
      {/* Tabla de Items */}
      <table className="w-full mt-2">
        <thead>
          <tr>
            <th className="text-left text-sm font-semibold text-gray-700">
              N° de Cuenta
            </th>
            <th className="text-left text-sm font-semibold text-gray-700">
              
            </th>
            <th className="text-right text-sm font-semibold text-gray-700">
              Débito
            </th>
            <th className="text-right text-sm font-semibold text-gray-700">
              Crédito
            </th>
          </tr>
        </thead>
        <tbody>
          {entry.items.map((item, index) => (
            <tr key={index} className="border-t">
              <td className="py-2 text-gray-800">{item.accountNumber}</td>
              <td className="py-2 text-gray-800">{item.description}</td>
              <td className="py-2 text-right text-gray-800">{item.debit}</td>
              <td className="py-2 text-right text-gray-800">{item.credit}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {/* Descripción adicional después de la tabla */}
      <div className="mt-4 text-gray-600">
        <p>
          <strong>Descripción: </strong>Este es el detalle de la partida contable
          #{entry.id}.
        </p>
      </div>
      {/* Total */}
      <div className="flex justify-end mt-4">
        <span className="text-right font-semibold">Total: {entry.total}</span>
      </div>
    </div>
  );
}
