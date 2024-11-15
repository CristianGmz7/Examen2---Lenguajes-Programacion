import { EntryCard } from "./EntryCard";

export function EntriesList() {
  const entries = [
    {
      id: 1,
      date: "15 de noviembre",
      items: [
        { description: "Bancos", debit: 500, credit: 0 },
        { description: "Acreedores", debit: 0, credit: 500 },
      ],
      total: 500,
    },
    {
      id: 2,
      date: "15 de noviembre",
      items: [
        { description: "Terrenos", debit: 1000, credit: 0 },
        { description: "Bancos", debit: 0, credit: 500 },
        { description: "Acreedores", debit: 0, credit: 500 },
      ],
      total: 1000,
    },
    {
      id: 3,
      date: "15 de noviembre",
      items: [
        { description: "Ventas", debit: 500, credit: 0 },
        { description: "Otros Ingresos", debit: 500, credit: 0 },
        { description: "Perdidas y Ganancias", debit: 0, credit: 1000 },
      ],
      total: 1000,
    },
  ];

  const handleAddEntry = () => {
    alert("Agregar nueva partida contable");
  };

  return (
    <div className="w-full max-w-4xl mx-auto p-4">
      <div className="bg-white shadow-lg rounded-lg">
        {/* Header con título y botón */}
        <div className="flex justify-between items-center p-4 border-b">
          <h1 className="text-2xl font-bold">Partidas Contables</h1>
          <button
            className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 focus:outline-none"
            onClick={handleAddEntry}
          >
            Agregar Partida
          </button>
        </div>
        {/* Lista de Partidas */}
        <div className="space-y-6 p-4">
          {entries.map((entry) => (
            <EntryCard key={entry.id} entry={entry} />
          ))}
        </div>
      </div>
    </div>
  );
}
