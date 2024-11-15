export  function EntryCard({ entry }) {
  return (
    <div className="border rounded-lg">
      <table className="table-auto w-full text-left">
        <thead className="bg-gray-100">
          <tr>
            <th className="p-2 w-[200px]">Partida #{entry.id}</th>
            <th className="p-2">{entry.date}</th>
            <th className="p-2 text-right">Debe</th>
            <th className="p-2 text-right">Haber</th>
          </tr>
        </thead>
        <tbody>
          {entry.items.map((item, index) => (
            <tr key={index} className="border-t">
              <td className="p-2"></td>
              <td className="p-2">{item.description}</td>
              <td className="p-2 text-right">
                {item.debit > 0 ? item.debit : ""}
              </td>
              <td className="p-2 text-right">
                {item.credit > 0 ? item.credit : ""}
              </td>
            </tr>
          ))}
          <tr className="border-t bg-gray-50">
            <td className="p-2 font-semibold">Descripción</td>
            <td className="p-2"></td>
            <td className="p-2 text-right font-medium">{entry.total}</td>
            <td className="p-2 text-right font-medium">{entry.total}</td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}
